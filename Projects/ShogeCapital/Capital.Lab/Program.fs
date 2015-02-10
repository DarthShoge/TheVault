// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open Capital.EventProfiler
open System
open FSharp.Data
open FSharp.Charting
open MathNet.Numerics.Statistics
open Capital.Extensions
open Capital.DataStructures
open System.Drawing
open Agents
open Capital.Engine
open Capital.Charting
open ShogeLabs.PatternRecognition
open Capital.DataProviders
open System.Windows.Forms.DataVisualization.Charting
open System


let runEventStudy() = 
    let startDate = new DateTime(2012, 01, 01)
    let endDate = new DateTime(2013, 01, 01)
    let symbols = [|"AAPL"; "MSFT"; "XOM"; "SPX"; "GLD"|]
    
    let stocks = loadStocks symbols startDate endDate
    sprintf "%A" stocks |> ignore
    let chrt = (stocks |> normalized |> DisplayChart)
    chrt.SaveChartAs("normalised.jpg",FSharp.Charting.ChartTypes.ChartImageFormat.Jpeg)
    let dailyRets = dailyReturns stocks
    let allStcks = getAllSymbols() |> Seq.toArray
    let curried = correlationChart dailyRets
    let dates = getNYSEDates startDate endDate

    System.Threading.Thread.Sleep(10000)
    printf "Event block began \n"
    let sw = System.Diagnostics.Stopwatch.StartNew()
    let snP = getSnP500Symbols()
    let snpSymbols = "SPX"::(snP |> Array.map(fun x -> x.Ticker) |> Seq.toList)
    let sdata = loadStocksParallel (snpSymbols |> List.toArray) startDate endDate 70
    printf "loading data took %d seconds \n" (sw.ElapsedMilliseconds / 1000L)
//    let profiler = EventProfiler("SPX", fun symRet mktRet -> symRet <= -0.1 && mktRet >= 0.05)
//    let events  = profiler.FindAllEvents sdata (getNYSEDates startDate endDate)
    let profiler = EventProfiler("SPX", fun symRet mktRet -> mktRet >= 0.1)
    let events2  = profiler.ProfileAll (sdata ,(getNYSEDates startDate endDate), 20)
    let evchart = (eventsChart events2)
    evchart.ShowChart();
    evchart.SaveChartAs("ev.jpg",FSharp.Charting.ChartTypes.ChartImageFormat.Jpeg)
    (curried "GLD" "XOM").SaveChartAs("cor.jpg",FSharp.Charting.ChartTypes.ChartImageFormat.Jpeg)
    printf "FINISHED %d seconds" (sw.ElapsedMilliseconds / 1000L)
    Console.ReadLine()

[<EntryPoint>]
let main argv = 
    let aapl = YahooDataProvider().GetStockData "EA" (DateTime(2001,01,01)) (DateTime(2014,01,01)) 
    let lookback = 20
    let look4wrd = 10
    let look4wrdCalc = 10
    let patternFinder = PatternRecogniser(lookback,look4wrdCalc,TopAnchored)
    let patterns = aapl |> Seq.map(fun x -> x.AC) 
                        |> Seq.windowed(lookback + look4wrd + 1) 
                        |> Seq.map(fun x -> patternFinder.ToPattern x)
                        |> Seq.toArray

    let cp = patterns.[patterns.Length - 1]

    let bestPatterns = patterns.[..patterns.Length - 1]  
                        |> Array.mapi(fun i x -> (i,cp % x))
                        |> Array.filter(fun (i,x) -> x.Outcome.IsSome && x.Outcome.Value >= 0.60)

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
                            yield   Chart.Point([(outcomeXPoint + 1,cp.Outcome.Value)])
                                    |> Chart.WithSeries.Marker(Style= MarkerStyle.Circle,Size=8,Color = if cp.Outcome.Value < 0. then Color.BurlyWood else Color.Blue)
                            for s in similars do
                                yield Chart.Line(s.PatternArray)
                                yield Chart.Point([(outcomeXPoint,s.Outcome.Value)])
                                        |> Chart.WithSeries.Marker(Style= MarkerStyle.Circle,Size = 8,Color = if s.Outcome.Value < 0. then Color.FromArgb(100, 255, 0, 0) else Color.FromArgb(100, 0, 255, 0)
                                        ,BorderWidth = 3 )
                        ]
                        |> Chart.WithTitle(sprintf "Probability of positive movement = %f" possibilityOfpositive)


// Define your library scripting code here



    0 // return an integer exit code

