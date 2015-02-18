module ShogeLabs.Patterns
open Capital.DataProviders
open System
open System.Drawing
open ShogeLabs.PatternRecognition
open FSharp.Charting
open System.Drawing
open System.Windows.Forms.DataVisualization.Charting
open Capital.DataStructures

type PatternRunner(dataProvider) =

    member x.Run(lookback,look4wrd,tolerance,dateRange,symbol : string) =
        let aapl : Tick array = dataProvider symbol dateRange.Start dateRange.End
        let patternFinder = PatternRecogniser(lookback,look4wrd,TopAnchored)
        let patterns = aapl |> Seq.map(fun x -> x.AC) 
                            |> Seq.windowed(lookback + look4wrd + 1) 
                            |> Seq.map(fun x -> patternFinder.ToPattern x)
                            |> Seq.toArray

        let cp = patternFinder.ToPattern (aapl |> Seq.map(fun x -> x.AC) |> Seq.toArray).[(aapl.Length - (lookback + 1))..(aapl.Length - 1)] 

        let bestPatterns = patterns.[..patterns.Length - 1]  
                            |> Array.mapi(fun i x -> (i,cp % x))
                            |> Array.filter(fun (i,x) -> x.Outcome.IsSome && x.Outcome.Value >= tolerance)

        let similars = (bestPatterns |> Array.map(fun (i,x) -> patterns.[i]))
        {Patterns = similars; MainPattern =cp}

    member x.ChartPatterns(lookback,look4wrd,tolerance,dateRange,symbol) =
        let result = x.Run(lookback,look4wrd,tolerance,dateRange,symbol)
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