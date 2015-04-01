// Learn more about F# at http://fsharp.net. See the 'F# Tutorial' project
// for more guidance on F# programming.

#r "System.Core.dll"
#r @"D:\Code\Repos\TheVault\Projects\ShogeCapital\packages\MathNet.Numerics.3.5.0\lib\portable-net4+sl5+netcore45+wpa81+wp8+MonoAndroid1+MonoTouch1\MathNet.Numerics.dll"
#r @"D:\Code\Repos\TheVault\Projects\ShogeCapital\ShogeLabs.PatternRecognition\bin\Debug\Capital.Main.dll"
#r "D:/Code/Repos/TheVault/Projects/ShogeCapital/packages/FSharp.Charting.0.90.9/lib/net40/FSharp.Charting.dll"
#r "D:/Code/Repos/TheVault/Projects/ShogeCapital/packages/FSharp.Data.2.0.14/lib/net40/FSharp.Data.dll"
#r "D:/Code/Repos/TheVault/Projects/ShogeCapital/packages/Deedle.1.0.6/lib/net40/Deedle.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\System.Windows.Forms.DataVisualization.dll"
#r @"D:\Code\Repos\TheVault\Projects\ShogeCapital\packages\FSharp.Collections.ParallelSeq.1.0.2\lib\net40\FSharp.Collections.ParallelSeq.dll"

#load @"D:\Code\Repos\TheVault\Projects\ShogeCapital\ShogeLabs.PatternRecognition\PatternRecognition\PatternRecogniser.fs"
#load @"D:\Code\Repos\TheVault\Projects\ShogeCapital\ShogeLabs.PatternRecognition\PatternRecognition\PatternRunner.fs"
#load "../packages/FSharp.Charting.0.90.9/FSharp.Charting.fsx"
#load "../packages/Deedle.1.0.6/Deedle.fsx"

#load "SeriesExtensions.fs"
#load @"D:\Code\Repos\TheVault\Projects\ShogeCapital\Capital.Main\DataStructures.fs"
#load "Charting.fs"
#load "EventProfiler.fs"
#load "../packages/Deedle.1.0.6/Deedle.fsx"
#load "../packages/FSharp.Charting.0.90.9/FSharp.Charting.fsx"
open Deedle
open Capital.DataStructures
open System
open Capital.Charting
open Capital.EventProfiler
open Capital.Agents
open Capital.DataProviders
open System.Drawing
open Capital.Engine
open FSharp.Data
open ShogeLabs.Strategies.Patterns
open FSharp.Charting
open MathNet.Numerics.Statistics
open ShogeLabs.Strategies.PatternRecognition


let dates : Capital.DataStructures.DateRange = {Start = (DateTime(2008,01,01)); End = (DateTime(2014,01,01))}
let config =  { Lookback = 20;
                LookForward = 5;    
                Tolerance = 0.5;
                GenerateOrderOn = fun pttrn -> pttrn.PossibilityOfRise > 0.8;
                Range = {Start = (DateTime(2008,01,01)); End = (DateTime(2014,01,01))}; 
                Period = Capital.DataStructures.Period.Day;
                OrderSize = 100. }

let universe = [|"AAPL";"MSFT";"ADBE";"MMM";"AMZN";"BAC";"CSCO";"EBAY";"EA";"F";"ORCL";"PEP";"RL";"RHT";"SBUX"|]
let data = loadStocksParallelWith (fun x -> x) universe config.Range.Start config.Range.End 3 
let strategy = PatternRecognitionStrategy(YahooDataProvider().GetStockData)
let amzn = YahooDataProvider().GetStockData "pep" config.Range.Start config.Range.End
let timer = Diagnostics.Stopwatch.StartNew()
let backtest = data |> Frame.mapCols(fun x y-> strategy.WalkForward(config, y.As<Tick>().Values) )
let values = backtest.Keys |> Seq.map(fun x -> amzn |> Seq.tryFind(fun t -> t.Date = x))
let pricesAtClose = data |> Frame.mapCols(fun x y -> y.As<Tick>() |> Series.map(fun k v -> v.AC))
let cflow = BackTester().GetBacktestCash( backtest, pricesAtClose, 10000.)
let chr = Chart.Line( cflow.Values |> Seq.toArray,"Cash")
chr.ShowChart()
printf "took %A seconds" timer.Elapsed.TotalSeconds 

//let batchSize = 70
//let startDate = new DateTime(2012, 01, 01)
//let endDate = new DateTime(2013, 01, 01)
//let dates = getNYSEDates startDate endDate |> Array.map(fun x -> x.ToString()) 
//let symbols = [|"AAPL"; "MSFT"; "XOM"; "SPX"; "GLD"|]
//let stocks = loadStocks symbols startDate endDate
//
//let d = series[1 => 0.4; 2 => 0.8; 3 => 1.]
//let productive_d = d + 1.
//productive_d |> Series.scanValues (fun x y -> y * x) 1.
//(stocks |> normalized |> DisplayChart).ShowChart()
//
//printf "Event block began \n"
//let sw = System.Diagnostics.Stopwatch.StartNew()
//let snP = getSnP500Symbols()
//let snpSymbols = "SPX"::(snP |> Array.map(fun x -> x.Ticker) |> Seq.toList)
//let sdata = loadStocksParallel (snpSymbols |> List.toArray) startDate endDate batchSize
//printf "loading data took %d seconds \n" (sw.ElapsedMilliseconds / 1000L)
//let profiler = EventProfiler("SPX", fun symRet mktRet -> symRet <= -0.05 && mktRet >= 0.05)
////let events  = profiler.FindAllEvents sdata (getNYSEDates startDate endDate)
//let events2  = profiler.ProfileAll(sdata ,(getNYSEDates startDate endDate), 20)
//printf "FINISHED %d seconds" (sw.ElapsedMilliseconds / 1000L)
//
//let dailyRets = dailyReturns stocks
//let curried = correlationChart dailyRets
//(curried "GLD" "XOM").ShowChart()
//(eventsChart events2).ShowChart()