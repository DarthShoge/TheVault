module ShogeLabs.Strategies.Patterns
open Capital.DataProviders
open System
open System.Drawing
open ShogeLabs.Strategies.PatternRecognition
open FSharp.Charting
open System.Drawing
open Deedle
open Capital.Extensions.List
open System.Windows.Forms.DataVisualization.Charting
open FSharp.Collections.ParallelSeq
open Capital.DataStructures


type PatternStrategyConfig =
    {
        Lookback : int;
        LookForward : int;
        Tolerance : double;
        Period : Period;    
        Range : DateRange;
        OrderSize : double
        GenerateOrderOn : PatternSummary -> bool
    }
    
type PatternRecognitionStrategy(dataProvider) =

    let getNextDate (alldates : DateTime list) date=
        alldates |> List.filter(fun d -> d >= date) |> Capital.Extensions.List.tryHead

    let rec aggregateOrders orders =
        match orders with
        |h::[] -> h
        |h::t -> AggregateOrder(h,aggregateOrders t)
        |[] -> NoOrder

    member x.Run(lookback,look4wrd,tolerance,symdata :Tick list) =
        let patternFinder = PatternRecogniser(lookback,look4wrd,TopAnchored)
        let windows = symdata |> Seq.map(fun x -> x.AC) 
                            |> Seq.windowed(lookback + look4wrd + 1) 
        let patterns =      windows
                            |> Seq.map(fun x -> patternFinder.ToPattern x)
                            |> Seq.toArray

        let cp = patternFinder.ToPattern (symdata |> Seq.map(fun x -> x.AC) |> Seq.toArray).[(symdata.Length - (lookback + 1))..(symdata.Length - 1)] 

        let bestPatterns = patterns.[..patterns.Length - 1]  
                            |> Array.mapi(fun i x -> (i,cp % x))
                            |> Array.filter(fun (i,x) -> x.Outcome.IsSome && x.Outcome.Value >= tolerance)

        let similars = (bestPatterns |> Array.map(fun (i,x) -> patterns.[i]))
        {Patterns = similars; MainPattern =cp}



    member x.ChartPatterns(lookback,look4wrd,tolerance,dateRange,symbol) =
        let symdata : Tick array = dataProvider symbol dateRange.Start dateRange.End
        let result = x.Run(lookback,look4wrd,tolerance,symdata |> Array.toList)
        let cp,similarPatterns = result.MainPattern,result.Patterns 
        let outcomeXPoint = (lookback + 1)
        let chart = Chart.Combine[
                                yield Chart.Line(cp.PatternArray,Name="main")
                                      |> Chart.WithSeries.Style(Color= Color.Black,BorderWidth = 5)
                                for s in similarPatterns do
                                    yield Chart.Line(s.PatternArray)
                                    yield Chart.Point([(outcomeXPoint,s.Outcome.Value)])
                                            |> Chart.WithSeries.Marker(Style= MarkerStyle.Circle,Size = 8,Color = if s.Outcome.Value < 0. then Color.FromArgb(100, 255, 0, 0) else Color.FromArgb(100, 0, 255, 0)
                                            ,BorderWidth = 3 )
                            ]
                            |> Chart.WithTitle(sprintf "Probability of positive movement = %f" result.PossibilityOfRise)
        chart
    member my.WalkForward(config,data) =
        let dates = data |> Seq.map(fun x -> x.Date) |> Seq.sort |> Seq.toList
        let getNextAvailable = getNextDate dates
        let walkthrough d =
            let vals = data |> Seq.filter(fun x -> x.Date <= d) |> Seq.sortBy(fun x -> x.Date) |> Seq.toList
            if(vals.Length > config.Lookback) then
                let oc = my.Run(config.Lookback,config.LookForward,config.Tolerance,vals)
                let future,order = match oc with
                                | v when config.GenerateOrderOn v -> 
                                                    let orda = Buy(config.OrderSize)
                                                    let ftr = match config.Period with
                                                        | Day -> (getNextAvailable (d.AddDays(float config.LookForward)),Sell(config.OrderSize))
                                                        | Hour -> (getNextAvailable (d.AddHours(float config.LookForward)),Sell(config.OrderSize))
                                                        | Minute -> (getNextAvailable (d.AddMinutes(float config.LookForward)),Sell(config.OrderSize))
                                                    (ftr,orda)
                                | _ -> ((None,NoOrder),NoOrder)
                

                (future,order)
            else
                ((None,NoOrder),NoOrder)
        
        let rawTrades = seq {
                            for period in dates do 
                                let futureTrade,order = walkthrough period
                                if (snd futureTrade) <> NoOrder && (fst futureTrade).IsSome then
                                    yield ((fst futureTrade).Value , snd futureTrade)
                                yield (period,order)
                            } 
                                
        let conditionedTrades = 
            rawTrades |> Seq.groupBy(fun (date,trade) -> date)
            |> Seq.map(fun (date ,allTrades) -> 
                            let ordrsOnly = allTrades|> Seq.map snd |> Seq.filter(fun x -> x.IsActiveOrder)
                            let result = 
                                if(allTrades |> Seq.length = 1) then
                                    snd (allTrades |> Seq.nth 0)
                                else
                                    aggregateOrders (ordrsOnly |> Seq.toList)
                            (date,result))

        series [for v in conditionedTrades -> v] |> Series.sortByKey

    member my.WalkForward(config,symbol : string) =
        let dateRange = config.Range
        let data = dataProvider symbol dateRange.Start dateRange.End 
        my.WalkForward(config,data)
           
