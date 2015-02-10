module Capital.EventProfiler
open Deedle
open System
open System.Drawing
open FSharp.Data
open FSharp.Charting
open MathNet.Numerics.Statistics
open Capital.Extensions
open Capital.DataStructures

type stats = MathNet.Numerics.Statistics.Statistics


let getDailyReturnForToday (today : DateTime) (nextDays : DateTime list) (ser : Series<DateTime,double>) = 
    let seriesToday, seriesYester = ser.[today], ser.[nextDays.Head]
    (seriesToday - seriesYester)/seriesYester

let defaultStrategy ep today nextDays mktSer symSer  =                         
    let symReturn = getDailyReturnForToday today nextDays symSer 
    let mktReturn = getDailyReturnForToday today nextDays mktSer
    ep symReturn mktReturn

type EventProfiler(marketSymbol, strategyPredicate) =
    let findEvents symbol (stockFrame : Frame<DateTime,string>) rawDates =
        let marketSeries = stockFrame.[marketSymbol]

        let nonMarketSeries = stockFrame |>  Frame.filterCols(fun x y -> x <> marketSymbol)
        let dates = rawDates
                    |> Array.sortBy(fun (x: DateTime) -> -x.Ticks)
                    |> Array.toList
        let rec innerFindEvents  (ser : Series<DateTime,double>) (dates: DateTime list) accu = 
            match dates with
            |[] -> accu
            | c::[] -> accu
            |today::nextDays ->
                let result = match ser with
                    |s when (s |> Series.countValues) < 2 -> (today => 0)
                    |s when not <| s.ContainsKey(today) -> (today => 0)
                    |symSer -> 
                        if strategyPredicate today nextDays marketSeries symSer  then (today => 1) else (today => 0)
                innerFindEvents ser nextDays (result::accu)
        let intermediateResult =  Series.ofObservations (innerFindEvents stockFrame.[symbol] dates [])
        intermediateResult

    let processEvents (stock : Series<DateTime,double>) dates lookbck = 
        let rec innerProcess dts vals = 
            match dts with
            |[] -> vals
            |evntDate::t ->
                let filtered = stock |> Series.filterValues(fun y -> y <> 0.0)
                let after, before = (filtered.After evntDate) , filtered.Before evntDate
                let result = 
                    match (after, before) with
                    |(a, b) when a |> Series.countKeys < lookbck -> None
                    |(a, b) when b |> Series.countKeys < lookbck -> None
                    |(a,b) ->
                        let upper = after |> Series.take lookbck |> Series.lastKey
                        let lower = before |> Series.rev |> Series.take lookbck  |> Series.lastKey
                        let captures = filtered.Between( lower, upper)
                        let obs = captures.Values |> Seq.mapi(fun i x -> (i - lookbck) => x)
                        Some(Series.ofObservations obs)
                innerProcess t (result::vals)
        innerProcess dates [] |> List.filter(fun x -> x.IsSome) |> List.map(fun x -> x.Value)


    let profileEvents (stock : Series<DateTime,double>) (events : Series<DateTime,int>) lookback = 
        let triggeredEvents = events |> Series.filterValues(fun x -> x = 1) 
        let eventDates = triggeredEvents.Keys |> Seq.toList
        let results = processEvents stock eventDates lookback
        results

    new(marketSymbol, ep : double -> double -> bool) = 
        let curriedDefault = defaultStrategy ep
        EventProfiler(marketSymbol,curriedDefault)
    new(marketSymbol) = EventProfiler(marketSymbol,  fun symRet mktRet -> symRet <= -0.03 && mktRet >= 0.02)

    member x.FindEvents stock dates =  findEvents stock dates
    member x.FindAllEvents (stocks : Frame<DateTime,string>) (dates : DateTime[]) = 
        stocks |> Frame.mapCols(fun col ticker -> x.FindEvents col stocks dates)
    member x.ProfileEvents rets evs lookbck = profileEvents rets evs lookbck
    member x.ProfileAll (stocks : Frame<DateTime,string>, dates : DateTime[], lookback, marketNuetral) =
        let r = (stocks).Columns.Observations |> Seq.map(fun col -> 
                        if marketNuetral && col.Key = marketSymbol then 
                            Array.empty
                        else
                            let evnts = x.FindEvents col.Key stocks dates
                            let prcs = col.Value.As<double>() 
                            let returns  = prcs |> Series.pairwiseWith (fun k (v1, v2) ->if v1 = 0. then 1. else (v2-v1) / v1)
                            //DEBUGGING 
                            if returns.Values |> Seq.exists(fun xx -> xx > 10.) then
                                let x = 1 * 2 
                                () else ()//DEBUGGING
                            let results = x.ProfileEvents returns evnts lookback |> Seq.toArray
                            results
                        ) |>  Seq.filter(fun x -> x.Length > 0) |> Seq.collect(fun x -> x) |> Seq.toList
        let iterator = ref 0
        frame [for v in r do 
                    let x = iterator.Value =>  v
                    iterator := iterator.Value + 1
                    yield x
                     ]

    member x.ProfileAll (stocks : Frame<DateTime,string>, dates : DateTime[],lookback  )= x.ProfileAll(stocks ,dates ,lookback ,true)

let eventsChart (eventsReturns : Frame<int,int>) =
    let rowCount = (eventsReturns.RowCount - 1) / 2
    let eventCount = eventsReturns.Columns.ValueCount
    let cumprodReturns = eventsReturns |> Frame.mapCols(fun c x -> x.As<double>() + 1. |> Series.scanValues(fun i j -> i * j) 1. )
    let rows = cumprodReturns.Rows.Values
    let avgSer = rows |> Seq.toArray |> Array.mapi(fun i x -> i - rowCount => stats.Mean( x.As<double>().Values))
    rows |> Seq.mapi(fun i x -> 
                                let ser = x.As<double>().Values |> Seq.toArray
                                let stDev = stats.StandardDeviation(ser)
                                let mean = stats.Mean(ser)
                                let currentRow = i - rowCount
                                if currentRow >= 0 then (mean+stDev,mean - stDev) else (mean,mean)
                            )
    |> Chart.Range 
    |> Chart.WithSeries.Style( Color = Color.FromArgb(32, 135, 206, 250), 
       BorderColor = Color.Blue, BorderWidth = 1)
    |> Chart.WithTitle(eventCount.ToString() + " Events Processed")
    |> Chart.WithYAxis(Title = "mean", Enabled = true,Max = 1.4, Min = 0.6)    
    |> Chart.WithXAxis(Title = "timeStep", Enabled = true)
