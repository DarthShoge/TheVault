
module DataStructures
open System
open MathNet

type cash = decimal
type stockPrice = decimal
type units = int

type Bar = {Hi: decimal; Low: decimal; Open: decimal; Close: decimal; Volume: decimal; AdjClose: decimal }

type OHLCPoint = {Start: DateTime; Value:Bar}

type Parameter<'a> = 
    |Param of 'a

    member x.UnWrap = 
        match x with
        |Param(g) -> g


type ActiveSignal = 
    |LiveSignal of (OHLCPoint )
    |NullSignal of (OHLCPoint )
    |DeadSignal of (OHLCPoint * OHLCPoint)

    member x.GetEntryPoint =
        match x with
        |LiveSignal(value) -> value
        |DeadSignal(value,_) -> value

    member x.IsDead = match x with 
    |DeadSignal(z) -> true 
    |_ -> false

    member x.GetPointsAfterStrike data = 
        match x with 
        |LiveSignal sign -> data 
                            |> Seq.takeWhile(fun (t : OHLCPoint) -> t.Start >= sign.Start)
                            |> Seq.toList
        |_ -> [] 
        

type Direction =
    | Up of int
    | Down  of int

    member x.Duration = match x with Up t |Down t -> t

    member x.GetFunction close openP = 
        match x with
            |Down(n) -> close >= openP
            |Up(n) -> close <= openP

let down n = Down(n)
let up n = Up(n)

type quantity = int 

[<StructuredFormatDisplay("{AsString}")>]
type Order = 
    |Long of OHLCPoint * quantity
    |Short of OHLCPoint * quantity
    |DeadOrder of Order * OHLCPoint
    |UnFilledOrder 

    member x.IsStillActive = match x with 
        |DeadOrder(_) | UnFilledOrder(_) -> false
        |Long(_)| Short(_) -> true

    member x.HasBeenFilled = match x with 
        | UnFilledOrder -> false
        |Long(_)| Short(_) |DeadOrder(_)  -> true


    member x.GetPoint = 
        match x with
        |Long(value,_)|Short(value,_) -> value
        |UnFilledOrder(_) -> 
            {Start = DateTime(); Value = {Open = 0m; Close= 0m; Hi = 0m; Low = 0m; AdjClose = 0m; Volume = 0m}}
        |DeadOrder(y,_) -> match y with |Long(value,_)|Short(value,_) -> value

    member x.ToActiveSignal =
        match x with
        |Long(v,_) | Short(v,_) -> LiveSignal(v)

    member x.GetReturnsBy f =
        match x with 
        |Long(value,contractSize) | Short(value,contractSize) -> - 0.
        |DeadOrder(Long(opened,contractSize),closed) -> f x 
        |DeadOrder(Short(opened,contractSize),closed) -> f x

    member x.AsString =
        match x with
        |Long(t,_)|Short(t,_) -> sprintf "entered @ %i/%i/%i on open %g" t.Start.Day t.Start.Month t.Start.Year t.Value.Open
        |DeadOrder(v,t) -> sprintf "%s || exited @ %i/%i/%i on open %g" (v.AsString) t.Start.Day t.Start.Month t.Start.Year t.Value.Open
        |UnFilledOrder(_) -> "unfilled"

and ReturnsCalculator() = 
    let round (d: int) (x: float) =
        let rounded = Math.Round(x,d)
        if rounded < x then x + Math.Pow(10.0,float -d) else rounded

    let calculateSimpleReturns (openf: OHLCPoint -> decimal) (closef :OHLCPoint -> decimal)  = function
        |Long(_) -> 0.
        |Short(_) -> 0.
        |UnFilledOrder(_) -> 0.
        |DeadOrder(opened,closed) ->  
                            let calculate pt p0 = 
                                if p0 = 0m && pt = 0m then 0. 
                                elif p0 = 0m && double pt > double p0 then -1.
                                else (double pt - double p0)/ double p0
                            match opened with
                            |Long(_) -> calculate (closef(closed)) (openf(opened.GetPoint))
                            |Short(_) -> calculate opened.GetPoint.Value.Open closed.Value.Open
    

    let calculateCCReturns calcSimpleReturns pt =
        match pt with 
        |Long(_) -> 0.
        |Short(_) -> 0.
        |UnFilledOrder(_) -> 0.
        |DeadOrder(_)  ->   let RT = calcSimpleReturns pt;
                            if RT > -1. then log10(1.0 +  RT) else -1. 

    let bySimpleReturnsOnOpen pt = calculateSimpleReturns (fun x -> x.Value.Open) (fun x -> x.Value.Open) pt; 

    member x.CalculateSingleCCReturnByClose pts = calculateCCReturns (calculateSimpleReturns (fun x -> x.Value.Open) (fun x -> x.Value.AdjClose) ) pts
    member x.CalculateSingleCCReturn  pts = calculateCCReturns bySimpleReturnsOnOpen pts
    member x.CalculateSimpleReturns pts =
            (pts |> Seq.map( fun (pi) -> 1.0 + (bySimpleReturnsOnOpen pi))|> Seq.reduce( fun Ri Ry -> Ri * Ry )) - 1.0

    member x.CalculateCompoundedReturns (simpleReturnsCalculation ,pts) = 
                        let Rts = pts|> Seq.map(fun pi -> calculateCCReturns simpleReturnsCalculation pi) |> Seq.toList 
                        match Rts with
                        |[] -> 0.
                        |_ -> Rts |> List.reduce(fun x y -> x + y) 

    member x.CalculateCompoundedReturns pts = x.CalculateCompoundedReturns( bySimpleReturnsOnOpen ,pts)
    member x.CalculateCompoundedReturnsByClose pts = 
        let byOpenClose pt = calculateSimpleReturns (fun x -> x.Value.Open) (fun x -> x.Value.Close) pt; 
        x.CalculateCompoundedReturns( byOpenClose ,pts)

type BackTestResults = { Balance : decimal; TradeHistory : list<Order>; AssetHistory: list<DateTime*decimal> ;Returns: list<DateTime*decimal>}
type BackTestOutcome = { Trades : list<Order>; Assets: list<DateTime*float> ;Returns: list<DateTime*float>}

type HistogramBucket(values: float array) =
    member this.Values = values
    member this.Max =  values |> Array.max
    member this.Min =  values |> Array.min

type DescriptiveStatistics = { Mean : float; Variance: float; StandardDev: float; Kurtosis: float; Skew: float; Histogram: HistogramBucket array }
type MovingAvgStrategyConfig = {Lookback:int; Lag:int}
