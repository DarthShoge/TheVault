namespace FSharpPlayHouse.Test
open NUnit.Framework
open FsUnit
open System
open FSharpPlayHouse

type Period =
    |Days
    |Weeks
    |Months


type StockWindow(lookAhead, period : Period) =
    let mutable period = period
    
    let actualLookBack = 
        match period with
        |Days -> lookAhead
        |Weeks -> lookAhead * 7
        |Months -> lookAhead * 28

    new(lookAhead) = StockWindow(lookAhead,Days)

    member this.GetHead data = (data |> List.maxBy(fun t -> t.Start))

    member this.GetWindowFrom (date: DateTime) (data : OHLCPoint seq) =
        let takeUntilDate = date.AddDays (float -actualLookBack)
        data |> Seq.sortBy(fun r -> DateTime.MaxValue - r.Start) |> Seq.takeWhile(fun d -> d.Start >= takeUntilDate)


type Signal =
//    |Short of decimal * OHLCPoint
    |Buy of OHLCPoint

type Parameter<'a> = 
    |Param of 'a

    member x.UnWrap = 
        match x with
        |Param(g) -> g

//type ActiveSignal(signal : Signal, strategy : IStrategy) = 
//    member x.OriginalSignal with get() = signal

type ActiveSignal = 
    IStrategy
    |LiveSignal of (OHLCPoint * IStrategy)
    |DeadSignal of (OHLCPoint * OHLCPoint)

and IStrategy =
    abstract Run : OHLCPoint list -> int -> Period -> Parameter<_> -> ActiveSignal option
    abstract Scan : ActiveSignal -> ActiveSignal
//    abstract Signals : seq<Signal>
//    abstract Sell : OHLCPoint list -> (int*Period)  -> unit 



type SimplePurchaseStrategy() =
    let rec carryReduce f data acc = 
        match data with
        |[] -> acc
        |_ ->   let result = f data
                let _::remainingData = data
                carryReduce f remainingData (result::acc)

    member this.buyAt (data: OHLCPoint list) (lookback : int*Period) (daysTo : Parameter<'a>) = // : Signal seq =
        let getMovement x =  x.Value.Close - x.Value.Open 
        let rec getSignals acc (points :  OHLCPoint list) days  =
            match points with
            |[] -> None
            |_ ->   let today::theRest = points
                    let todaysMve = getMovement today
                    if todaysMve > 0m then
                        match (acc |> Seq.length) with
                        |days -> Some(today::acc |> Seq.last)
                        |_ -> getSignals (today::acc) theRest days
                    else
                None

        let t,p = lookback
        let window = new StockWindow(t,p)
        let headOfWindow = window.GetHead data
        let pWindow = window.GetWindowFrom headOfWindow.Start data |> Seq.toList 
        
        let result = getSignals [] pWindow daysTo.UnWrap
        match result.IsSome with
        |true -> Some(LiveSignal(result.Value,(this :> IStrategy )))
        |false -> None

    interface IStrategy with
        member me.Run d i p v = me.buyAt d (i,p) v
        member me.Scan d = d


[<TestFixture>]
type ``Given that i try to get data using webGetter it does not fail`` () =

    
    [<Test>]
    member test.``when i ask for goog it gives me a sequence of the correct results`` () =
        let target = new StockDataProvider()
        let myResults = (target :> IStockDataProvider).GetSymbolData("GOOG")
        (myResults  |> Seq.length) > 1 |> should be True
       
    [<Test>]
    member test.``i can set a lookback period of 5 days and only 5 days of data will be shown``() =
        let target = new StockWindow(5)

        let data = (new FakeStockDataProvider()).GetStockSymbols(new DateTime(2012,01,01)) (new DateTime(2012,01,07))
        let maxDate = (data |> Seq.maxBy(fun r -> r.Start)).Start
        let window  = target.GetWindowFrom maxDate data
        let deb = window |> Seq.toList
        (window  |> Seq.length) = 5 |> should be True
        (window  |> Seq.maxBy(fun g -> g.Start)).Start = maxDate |> should be True


    [<Test>]
    member test.``i can set a lookback period of 3 weeks and only 3 weeks of data will be shown``() =
        let target = new StockWindow(3,Weeks)
        let data = (new FakeStockDataProvider()).GetStockSymbols(new DateTime(2013,9,2)) (new DateTime(2013,10,30))
        let maxDate = (data |> Seq.maxBy(fun r -> r.Start)).Start
        let window = target.GetWindowFrom maxDate data
        let deb = window |> Seq.toList
        (window  |> Seq.length) = 16 |> should be True
        (window  |> Seq.maxBy(fun g -> g.Start)).Start = maxDate |> should be True

        
    [<Test>]
    member test.``i can set a lookback period of 1 month and only 1 month of data will be shown``() =
        let target = new StockWindow(1,Months)
        let data = (new FakeStockDataProvider()).GetStockSymbols(new DateTime(2012,9,2)) (new DateTime(2013,10,3))
        let maxDate = (data |> Seq.maxBy(fun r -> r.Start)).Start
        let window = target.GetWindowFrom maxDate data
        let deb = window |> Seq.toList
        (window  |> Seq.length) = 21 |> should be True
        (window  |> Seq.maxBy(fun g -> g.Start)).Start = maxDate |> should be True



[<TestFixture>]
type ``strategies can buy and sell successfully ``() =
    let seedDate n = (new DateTime(2013,01,01)).AddDays n
    let ohlc o h l c v = {Open = o; Hi = h; Low = l; Close = c; Volume = v} 

    [<Test>]
    member test.``a buy signal is generated on 3 moves up`` () =
        //arrange
        let data = [
                {Start = seedDate 0.0;Value = ohlc 5m 8m 5m 7m 1000m};
                {Start = seedDate 1.0;Value = ohlc 7m 11m 6m 11m 1000m};
                {Start = seedDate 2.0;Value = ohlc 11m 16m 9m 14m 1000m};
                ]
        let strategy = new SimplePurchaseStrategy()
        let lookBackFor3 = Param(3)
        //act
        let signal = (strategy :> IStrategy).Run data 10 Days lookBackFor3
        let signalT = match signal.Value with | LiveSignal(p,t) -> p
        //assert
        signal.IsSome |> should be True
        signalT.Start = seedDate 2.0 |> should be True


    [<Test>]
    member test.``a sell signal is generated when a active signal raises a sell``() =
               let data = [
                {Start = seedDate 0.0;Value = ohlc 5m 8m 5m 7m 1000m};
                {Start = seedDate 1.0;Value = ohlc 7m 11m 6m 11m 1000m};
                {Start = seedDate 2.0;Value = ohlc 11m 16m 9m 14m 1000m};
                ]


//
//true |> should be True
//
//false |> should not (be True)
//
//[] |> should be Empty
//
//[1] |> should not (be Empty)
//
//"" |> should be EmptyString
//
//"" |> should be NullOrEmptyString
//
//null |> should be NullOrEmptyString
//
//null |> should be Null
//
//anObj |> should not (be Null)
//
//anObj |> should be (sameAs anObj)
//
//anObj |> should not (be sameAs otherObj)