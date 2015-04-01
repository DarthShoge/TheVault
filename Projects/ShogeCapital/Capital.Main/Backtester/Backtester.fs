module Capital.Main.MarketSimulation
open Deedle 
open System
open Capital.DataStructures

type BackTester() =

    static member ticksToDataFrame (ticks : (string*Tick seq) seq) =
        frame [
                for t in ticks do
                    let sym = (fst t)
                    let data = (snd t)
                    yield sym => (data |> Seq.map(fun x -> (x.Date,x.AC)) |> Series.ofObservations)
                
                ]

    member x.GetOwnedOrders (orders : Series<DateTime,Order>) =   
        orders |> Series.mapValues(fun order ->
                       order.AbsoluteValue
                    )
                |> Series.scanValues(fun x y -> x + y) 0.00

    member x.GetBacktestCash (orders : Series<DateTime,Order>, data : Tick seq, startCash) =
        orders |> Series.map(fun k ordr -> 
                                    let valueToday = data |> Seq.tryFind(fun x -> x.Date = k)
                                    if(valueToday.IsSome) then
                                        -(ordr.AbsoluteValue * valueToday.Value.AC)
                                    else
                                        0.
                                    )
               |> Series.scanValues(fun x y -> x + y ) startCash

    member x.GetBacktestCash (orders : Frame<DateTime,string>, data :Frame<DateTime,string>, startCash) =
        let periodsOfTrade = orders.RowKeys
        series [ for tstep in periodsOfTrade do 
                    let ordersToday  : Series<string,Order>= orders |> Frame.getRow tstep 
                    let dailycash = ordersToday.Observations 
                                    |> Seq.map(fun symbordrs ->
                                                let sym,ordr = symbordrs.Key,symbordrs.Value
                                                let valueAtStep = data.[sym] |> Series.tryGet tstep
                                                if(valueAtStep.IsSome) then
                                                    -(ordr.AbsoluteValue * valueAtStep.Value)
                                                else
                                                    0.)
                                    |> Seq.sum
                    yield tstep => dailycash 
               ]
               |> Series.scanValues(fun x y -> x + y ) startCash

    member x.GetOwnedCash (owned : Frame<DateTime,string>, data :Frame<DateTime,string>) =
        let periodsOfTrade = owned.RowKeys
        series [ for tstep in periodsOfTrade do 
                    let ownedToday  : Series<string,double>= owned |> Frame.getRow tstep 
                    let dailycash = ownedToday.Observations 
                                    |> Seq.map(fun symbordrs ->
                                                let sym,ownd = symbordrs.Key,symbordrs.Value
                                                let valueAtStep = data.[sym] |> Series.tryGet tstep
                                                if(valueAtStep.IsSome) then
                                                    (ownd * valueAtStep.Value)
                                                else
                                                    0.)
                                    |> Seq.sum
                    yield tstep => dailycash 
               ]
//               |> Series.map(fun x y -> x + y ) 0.0
                