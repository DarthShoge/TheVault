module NewStrategyModule

open DataStructures
open ShogeStrategies
open System
open System.Diagnostics
open Calculators
open Utilities


let rec last = function
| hd :: [] -> hd
| hd :: tl -> last tl
| _ -> failwith "Empty list."



let unRavel list =
    let rec innerUnravel list capturedRavels = 
        match list with
            |[] -> capturedRavels
            |h::[] -> match h with Some(x) -> x::capturedRavels
            |h::t -> match h with Some(x) ->
                let newlyCaptureds = x::capturedRavels
                innerUnravel t newlyCaptureds
    innerUnravel (list |> List.filter(fun x -> x.IsSome) )[]



let (| IsTrending |) (dir: Direction) yieldFunc wndowAt  = 

    let getPossibleTrendPoint directionFunc topPoint =  
        if (directionFunc topPoint.Value.Close topPoint.Value.Open) then
            None
        else
            Some(topPoint)

    let directionFunc = dir.GetFunction
    let rec followTrend remainingPoints currentTrend =
        match remainingPoints with
        |[] -> []
        |topPoint::_ when currentTrend |> List.length = dir.Duration -> Some(topPoint)::currentTrend
        |topPoint::theRest ->
                let maybePoint = getPossibleTrendPoint directionFunc topPoint
                let trendToCarry = match maybePoint with None -> [] | Some(x) -> currentTrend
                followTrend theRest (maybePoint::trendToCarry |> List.filter(fun x -> x.IsSome))

    match followTrend wndowAt [] |> List.filter(fun x-> x.IsSome) with
    |[] -> (None,None)
    |x::y -> 
            let reversedPoints = y |> List.rev 
            let trendStart = List.nth reversedPoints 0
            (Some( yieldFunc x.Value),trendStart)

type DateInclusivity =
    |Inclusive
    |Unenclusive

    member x.InclusivityFunction  t y = 
        match x with
        |Inclusive -> t >= y
        |Unenclusive -> t > y


let getPointsOnwards (inclusivity : DateInclusivity) data (toTakeFrom : OHLCPoint option) =

    match toTakeFrom with
    |Some(t) -> 
        data 
        |> Seq.sortBy(fun x -> dateSortDesc x.Start) 
        |> Seq.takeWhile(fun f -> inclusivity.InclusivityFunction f.Start t.Start ) 
        |> Seq.sortBy(fun f -> f.Start)

    |None -> 
        let h::t = data |> Seq.toList
        t |> List.toSeq

type TrendFollowerEntry() =
    
    let getOrderType dir point quantity  = 
        match dir with
        |Up(_) -> Long(point,quantity)
        |Down(_) -> Short(point,quantity)
        
    let getNewBalance riskFunc balance (order: Order option) = 
        match order with
        Some(o)->
            match o with
            |Short(value,_)
            |Long(value,_) -> 
                let ohcl = o.GetPoint.Value
                let tradeSpend =  (snd((riskFunc balance ohcl.Open)) |> decimal) * ohcl.Open
                balance - tradeSpend
            |_ -> balance
        |None -> balance
    
    let tryTake (n : int) (s : _ seq) =
        let e = s.GetEnumerator ()
        let i = ref 0
        seq {
            while e.MoveNext () && !i < n do
                i := !i + 1
                yield e.Current
        }
    let optionFilter v = 
        let list = v |> List.filter(fun (x : 'a option) -> x.IsSome)
        match list with
        |[] -> []
        |_ -> list |> List.map(fun x -> x.Value)

    let  buy (inDirection : Direction) (window : OHLCPoint seq) (riskFunc : decimal -> decimal -> decimal*int) balance  raisefunc = 
        let rec innerBuy (window : OHLCPoint seq) balance trends = 
            let orderedWindow = window |> Seq.sortBy(fun x -> x.Start)|> Seq.toList
            let actualWindow = orderedWindow |> tryTake (inDirection.Duration + 1) |> Seq.toList
            let riskToApply = riskFunc balance
            let newBalance = getNewBalance riskFunc balance
            let buyFunc point = getOrderType inDirection point (snd(riskToApply point.Value.Open))

            match actualWindow |> Seq.toList with
            |[] -> trends 
            |IsTrending inDirection buyFunc (raised, trendStart) -> 
                let trendResult = raisefunc(raised)::trends
                innerBuy ( getPointsOnwards Unenclusive orderedWindow trendStart) (newBalance raised) trendResult
        innerBuy window balance [] |> optionFilter 

    let enterToOrder direction balance riskFunc (data : OHLCPoint seq) =
        buy direction data riskFunc balance (fun x -> x) 
        |> List.sortBy(fun x -> x.GetPoint.Start)
           
    member I.Enter dir window =  
        buy dir window (fun x y ->(0m,1)) 0m (fun x -> match x with Some(v) -> Some(v.ToActiveSignal)|None -> None)
        |> List.sortBy(fun x -> x.GetEntryPoint.Start)
    member I.EnterToOrder = enterToOrder


type TrendFollowerExit() = 

    let exitOrderOn dir data order =
        match order with
        |Long(x,_) | Short(x,_) -> 
            let pointsOnWards = getPointsOnwards Inclusive data (Some(x)) 
                                |> Seq.toList
            let deadOrder exitPoint = DeadOrder(order,exitPoint)
            match pointsOnWards with
            |IsTrending dir deadOrder (raised,trend) ->
                        match raised with Some(_) -> raised.Value |None -> order
            |_ -> order
        |DeadOrder(_) -> order

    let exitOn dir data signal = 
        match signal with
        |LiveSignal(x) -> 
            let pointsOnWards = getPointsOnwards Inclusive data (Some(x)) |> Seq.toList
            let deadFunc y = DeadSignal(x,y)

            match pointsOnWards with
            |IsTrending dir deadFunc (raised,trend) -> 
                match raised with Some(_) -> raised.Value |None -> signal
            |_ -> signal

        |DeadSignal(v) ->signal


    let exit dir data raisedSignals exitFunction = 
        let rec exitLoop onExitFunc tradesToProcess newTrades = 
            match tradesToProcess with
            |h::t -> 
                let newSignal = onExitFunc h
                exitLoop onExitFunc t (newSignal::newTrades)
            |[] -> newTrades
        
        let unravelledSignals = raisedSignals 
        match data |> Seq.toList with
        |[]->  raisedSignals
        |h::_ ->  exitLoop exitFunction unravelledSignals []

    member x.Exit dir signals data = exit dir data signals (exitOn dir data)
    member x.ExitOrder dir orders data = exit dir data orders (exitOrderOn dir data)

type window = int
type Backtester(withRisk) =
    
    let  mergeAndFilterActives t inputList = 
         inputList 
         |> List.append t.TradeHistory
         |> Seq.filter(fun x -> x.IsStillActive)
         |> Seq.distinctBy(fun x -> x.GetPoint) 
         |> Seq.toList 

    let filterOutDeads input =
        input |> List.filter(fun (x : Order) -> not x.IsStillActive)

    let mergeBackDeads t inputList = 
        inputList @ (t.TradeHistory |> filterOutDeads)

    let seperateOutAlreadyRaisedOrders t s =
        let newInput = s |> Set.ofList
        let existingInput = t.TradeHistory |> Set.ofList
        let intersection = Set.intersect newInput existingInput
        let difference =  newInput -  existingInput
        difference |> Set.toSeq

    let alreadyExistsIn (trades : Order list) (t : Order) = trades |> List.exists(fun x -> x.GetPoint.Start = t.GetPoint.Start)

    let rec filterDuplicates initList acc (iteratedRts : Order list) =   
        match iteratedRts with
        |[] -> acc
        |head::tail ->    
            let multipledOrders = initList |> List.filter(fun (x: Order) -> head.GetPoint.Start = x.GetPoint.Start )
            match multipledOrders with
            |uniqueOrder::[] -> filterDuplicates initList (uniqueOrder::acc) tail
            |firstOrder::nextOrder::_ -> 
                if (firstOrder.IsStillActive && nextOrder.IsStillActive) then filterDuplicates initList (firstOrder::acc) tail
                else
                    let result = [firstOrder;nextOrder] |> Seq.tryFind(fun x -> not <| x.IsStillActive)
                    match result with Some(x) -> if(x |> alreadyExistsIn(acc)) then filterDuplicates initList acc tail 
                                                    else filterDuplicates initList (x::acc) tail 

    let backtest 
        (returnsCalc : Order -> decimal)
        (enter : decimal -> (decimal -> decimal ->  decimal * int) -> OHLCPoint seq -> Order list)
        (exit : Order list -> seq<OHLCPoint> -> Order list)
        (data : OHLCPoint list)
        (windowSize : window)
        balance = 
            let dataWindows = 
                data 
                |> Seq.sortBy(fun x -> x.Start)
                |> Seq.toList 
                |> List.slide(windowSize) 

            let rec runBacktestOnWindows allWindows trade enter exit = 
                match allWindows with
                |[] -> trade
                |window::nextWindows ->
                    let sortedWindow = window |> List.sortBy(fun x ->  dateSortDesc x.Start)
                    let entryOrders = enter trade.Balance (withRisk) sortedWindow |> mergeAndFilterActives trade
                    let returnsForThisWindow = exit entryOrders sortedWindow
                    let returns = returnsForThisWindow |> mergeBackDeads trade
                    let filteredReturns =  filterDuplicates returns [] returns |> List.sortBy(fun x -> dateSortDesc x.GetPoint.Start)  
                    let lastPointInWindow::_ = window |> List.rev
                    let returnsForWindow = (match filteredReturns with [] -> 0m | _ -> (filteredReturns |>  List.head) |> returnsCalc )
                    let newBalance = balance * (returnsForWindow::(trade.Returns |> List.map(fun (x,y) -> y))|> List.reduce(fun x y -> x + y))
                    let newTrade =  {
                        Balance = newBalance;
                        TradeHistory = filteredReturns |> List.toSeq |> Seq.toList
                        AssetHistory = (lastPointInWindow.Start,newBalance) ::trade.AssetHistory
                        Returns = (lastPointInWindow.Start,returnsForWindow) :: trade.Returns
                    }
                    runBacktestOnWindows nextWindows newTrade enter exit
            runBacktestOnWindows dataWindows {Balance = balance; TradeHistory = []; AssetHistory = []; Returns = []} enter exit

    member x.Backtest = backtest ((fun x -> 0m))
    member x.BacktestWithReturnsCalc returnsOn = backtest returnsOn


//    interface ExitStrategy with
//        member I.Exit raisedSignals direction data = I.Exit raisedSignals direction data