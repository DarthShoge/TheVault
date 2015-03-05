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
open Capital.Agents
open Capital.Engine
open Capital.Charting
open ShogeLabs.PatternRecognition
open Capital.DataProviders
open Deedle
open System.Windows.Forms.DataVisualization.Charting
open System
open System.Diagnostics
open ShogeLabs.Patterns


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
    let snP = getSnP500Symbols()
    let batchSize = 70
    let snpSymbols = "SPX"::(snP |> Array.map(fun x -> x.Ticker) |> Seq.toList)
    let dates = {Start = (DateTime(2001,01,01)); End = (DateTime(2014,01,01))}
    //let sdata = loadStocksParallel (snpSymbols |> List.toArray) dates.Start dates.End batchSize

    let run = PatternRecognitionStrategy(YahooDataProvider().GetStockData)
//    let allValues = snpSymbols 
//                        |> Seq.map(fun sym -> 
//                            let symdata : Tick array = YahooDataProvider().GetStockData sym dates.Start dates.End
//                            run.Run(20,15,0.5,symdata |> Seq.toList) )
//                        |> Seq.filter(fun x -> x.PossibilityOfRise > 0.6)
//                        |> Seq.toArray
    let chart = run.ChartPatterns(20,15,0.5,dates,"AMZN")
    let config =  { Lookback = 20;
                    LookForward =10;    
                    Tolerance = 0.5;
                    GenerateOrderOn = fun pttrn -> pttrn.PossibilityOfRise > 0.7;
                    Range = dates; 
                    Period = Day;
                    OrderSize = 100. }
    let strategy = PatternRecognitionStrategy(YahooDataProvider().GetStockData)
    let timer = Diagnostics.Stopwatch.StartNew()
    let backtest = strategy.WalkForward(config,"AMZN") |> Series.filter(fun k v -> v.IsActiveOrder)
    printf "took %A seconds" timer.Elapsed.TotalSeconds 
    0 // return an integer exit code

