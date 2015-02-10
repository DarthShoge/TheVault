// Learn more about F# at http://fsharp.net. See the 'F# Tutorial' project
// for more guidance on F# programming.

#r @"D:\Code\Repos\TheVault\Projects\ShogeCapital\ShogeLabs.PatternRecognition\bin\Debug\Capital.Main.dll"
#r "D:/Code/Repos/TheVault/Projects/ShogeCapital/packages/FSharp.Charting.0.90.7/lib/net40/FSharp.Charting.dll"
#load "PatternRecogniser.fs"
#load "../packages/FSharp.Charting.0.90.7/FSharp.Charting.fsx"

open Capital.DataProviders
open System
open System.Drawing
open ShogeLabs.PatternRecognition
open FSharp.Charting
open System.Drawing
open System.Windows.Forms.DataVisualization.Charting

let aapl = YahooDataProvider().GetStockData "EA" (DateTime(2001,01,01)) (DateTime.Now) 
let lookback = 20
let look4wrd = 10
let look4wrdCalc = 10
let patternFinder = PatternRecogniser(lookback,look4wrdCalc,TopAnchored)
let patterns = aapl |> Seq.map(fun x -> x.AC) 
                    |> Seq.windowed(lookback + look4wrd + 1) 
                    |> Seq.map(fun x -> patternFinder.ToPattern x)
                    |> Seq.toArray

let cp = patternFinder.ToPattern (aapl |> Seq.map(fun x -> x.AC) |> Seq.toArray).[(aapl.Length - (lookback + 1))..(aapl.Length - 1)] //patterns.[patterns.Length - 1]

let bestPatterns = patterns.[..patterns.Length - 1]  
                    |> Array.mapi(fun i x -> (i,cp % x))
                    |> Array.filter(fun (i,x) -> x.Outcome.IsSome && x.Outcome.Value >= 0.70)

let similars = (bestPatterns |> Array.map(fun (i,x) -> patterns.[i]))

let allValues =[|
        yield cp
        for p in similars -> p
    |]

let possibilityOfpositive = (double)(allValues |> Array.filter(fun x -> x.Outcome.IsSome && x.Outcome.Value > 0.)).Length / (double)allValues.Length

let outcomeXPoint = (lookback + 1)
let chart = Chart.Combine[
                        yield Chart.Line(cp.PatternArray,Name="main")
                              |> Chart.WithSeries.Style(Color= Color.Black,BorderWidth = 5)
//                        yield   Chart.Point([(outcomeXPoint + 1,cp.Outcome.Value)])
//                                |> Chart.WithSeries.Marker(Style= MarkerStyle.Circle,Size=8,Color = if cp.Outcome.Value < 0. then Color.BurlyWood else Color.Blue)
                        for s in similars do
                            yield Chart.Line(s.PatternArray)
                            yield Chart.Point([(outcomeXPoint,s.Outcome.Value)])
                                    |> Chart.WithSeries.Marker(Style= MarkerStyle.Circle,Size = 8,Color = if s.Outcome.Value < 0. then Color.FromArgb(100, 255, 0, 0) else Color.FromArgb(100, 0, 255, 0)
                                    ,BorderWidth = 3 )
                    ]
                    |> Chart.WithTitle(sprintf "Probability of positive movement = %f" possibilityOfpositive)

// Define your library scripting code here

