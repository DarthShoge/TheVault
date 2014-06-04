namespace ShogeStrategies
open System
open MoneyManagerModule
open DataStructures
open Utilities
open DescriptiveStatisticsModule

module Strategies =

type DebugEntryStrategy() = 
    member x.Enter balance (riskFunc : decimal -> decimal -> decimal*int) window =
        let riskToApply = riskFunc balance
        match window |> Seq.toList with 
        |[] -> []
        |h::_ -> let cost,contract = riskToApply h.Value.Open 
                 [Long(h,contract)]

type DebugExitStrategy() = 
    member x.Exit orders window  =
        match window |> Seq.toList with
        |[] -> orders
        |h::[] -> orders
        |h::j::t -> 
            match orders with
            |[] -> orders
            |order::_ -> 
                match order with
                |Long(p,y)-> DeadOrder(order,j)::orders  
                |Short(p,y)-> DeadOrder(order,j)::orders 
                |DeadOrder(_)-> orders 
                |UnFilledOrder(_)-> orders 


type DefaultEntryStrategy() =
    member x.Enter data =
        let current = data  |> List.head
        Long(current,0)

type DefaultExitStrategy() = 
    member x.Exit (data: OHLCPoint list) (order : Order) =
        match data with
        |[] -> order
        |h::t -> 
            if(order.GetPoint.Start < h.Start && order.IsStillActive) then
                DeadOrder(order,h)
            else
                order
 
 type BackTestRunner(enter, exit) =
    member x.Backtest (data : OHLCPoint list) =
        let sortedList = data |> List.sortBy(fun s -> s.Start)
        let tradeData = sortedList  |> List.flow(fun data orders -> 
                                                        let orderedData = data |> List.rev
                                                        let ordersPlusNew = (enter orderedData)::orders
                                                        let allOrders =  ordersPlusNew |> List.map(fun x -> exit orderedData x)
                                                        allOrders )
                                    |> List.filter(fun (t : Order) -> t.HasBeenFilled)

        let returns = tradeData |> List.map(fun t -> (t.GetPoint.Start,t.GetReturnsBy(ReturnsCalculator().CalculateSingleCCReturnByClose)))
        {Trades = tradeData;Returns = returns; Assets = []}

let orderByMovingAvg( parameter, orderPredicate, data) = 
      match data with
      |h::t -> 
            let values = t |> Seq.map(fun v -> float v.Value.Open) |> Seq.tryTake (parameter.Lag + parameter.Lookback) |> Seq.toArray 
            let mA = AverageCalculator().GetMovingAverages parameter.Lookback values ByMean 
                            |> Seq.tryTake parameter.Lag 
                            |> Seq.toList
            let xs = values |> Seq.take parameter.Lag
            let isTrending = (mA |> Seq.forall2( fun point avg -> orderPredicate point avg) xs)
            let hasAllPoints =  mA.Length = parameter.Lag
            hasAllPoints && isTrending

type MovingAverageEntryStrategy(p) =
    let enter (parameter : MovingAvgStrategyConfig ) (x : OHLCPoint list) = 
            match x with
            |g when (g |> List.length <= parameter.Lookback) -> UnFilledOrder
            |[] -> UnFilledOrder
            |h::t -> 
                if(orderByMovingAvg( parameter, (fun point avg -> avg < point), x)) then
                    Long(h,0) 
                else 
                    UnFilledOrder

    member me.Enter x = enter p x

type MovingAverageExitStrategy(p) = 
    let exit (parameter : MovingAvgStrategyConfig) points order =
            match order with
            |UnFilledOrder |DeadOrder(_,_) -> order
            |_ -> 
                match points with
                |[] -> order
                |h when parameter.Lookback >= points.Length -> order
                |h::t -> 
                        if(orderByMovingAvg( parameter, ( fun point avg -> avg > point ), points)) then
                            DeadOrder(order,h) 
                        else 
                            order
    member me.Exit x = exit p x
                

        
