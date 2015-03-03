module ShogeLabs.Patterns
open Capital.DataProviders
open System
open System.Drawing
open ShogeLabs.PatternRecognition
open FSharp.Charting
open System.Drawing
open Deedle
open System.Windows.Forms.DataVisualization.Charting
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
        //                        yield   Chart.Point([(outcomeXPoint + 1,cp.Outcome.Value)])
        //                                |> Chart.WithSeries.Marker(Style= MarkerStyle.Circle,Size=8,Color = if cp.Outcome.Value < 0. then Color.BurlyWood else Color.Blue)
                                for s in similarPatterns do
                                    yield Chart.Line(s.PatternArray)
                                    yield Chart.Point([(outcomeXPoint,s.Outcome.Value)])
                                            |> Chart.WithSeries.Marker(Style= MarkerStyle.Circle,Size = 8,Color = if s.Outcome.Value < 0. then Color.FromArgb(100, 255, 0, 0) else Color.FromArgb(100, 0, 255, 0)
                                            ,BorderWidth = 3 )
                            ]
                            |> Chart.WithTitle(sprintf "Probability of positive movement = %f" result.PossibilityOfRise)
        chart

    member my.WalkForward(config,symbol : string) =
        let dateRange = config.Range
        let data = dataProvider symbol dateRange.Start dateRange.End 
        let dates = data |> Seq.map(fun x -> x.Date) |> Seq.sort

        let walkthrough d =
            let vals = data |> Seq.filter(fun x -> x.Date <= d) |> Seq.sortBy(fun x -> x.Date) |> Seq.toList
            if(vals.Length > config.Lookback) then
                let oc = my.Run(config.Lookback,config.LookForward,config.Tolerance,vals)
                let future,order = match oc with
                                | v when config.GenerateOrderOn v -> 
                                                    let orda = Buy(config.OrderSize)
                                                    let ftr = match config.Period with
                                                        | Day -> (d.AddDays(float config.LookForward),Sell(config.OrderSize))
                                                        | Hour -> (d.AddHours(float config.LookForward),Sell(config.OrderSize))
                                                        | Minute -> (d.AddMinutes(float config.LookForward),Sell(config.OrderSize))
                                                    (ftr,orda)
                                | _ -> ((d,NoOrder),NoOrder)
                

                (future,order)
            else
                ((d,NoOrder),NoOrder)

        let rawTrades = seq [
                            for period in dates do 
                                let futureTrade,order = walkthrough period
                                if (snd futureTrade) <> NoOrder && (fst futureTrade) <= dateRange.End then
                                    yield futureTrade
                                yield (period,order)
                            ] 

        let conditionedTrades = 
            rawTrades |> Seq.groupBy(fun (date,trade) -> date)
            |> Seq.map(fun (date ,allTrades) -> 
                            let sumOfTrades = allTrades |> Seq.sumBy(fun (d,ord) ->  
                                            match ord with |NoOrder -> 0. |Buy(v) -> v |Sell(v) -> -v )
                            let result = match sumOfTrades with
                                        | v when v > 1. -> Buy(v)| v when v < -1. -> Sell(abs(v)) 
                                        | v when (allTrades |> Seq.exists(fun (x, y) -> y.IsActiveOrder)) -> Buy(0.0)
                                        | _ -> NoOrder
                            (date,result))

        series [for v in conditionedTrades -> v] |> Series.sortByKey

           
