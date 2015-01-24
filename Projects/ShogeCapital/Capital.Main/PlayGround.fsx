// Learn more about F# at http://fsharp.net. See the 'F# Tutorial' project
// for more guidance on F# programming.

#r "System.Core.dll"
#r "D:/Code/Repos/TheVault/Projects/ShogeCapital/packages/Deedle.1.0.6/lib/net40/Deedle.dll"
#r "D:/Code/Repos/TheVault/Projects/ShogeCapital/packages/MathNet.Numerics.3.2.3/lib/net40/MathNet.Numerics.dll"
#r "D:/Code/Repos/TheVault/Projects/ShogeCapital/packages/FSharp.Data.2.0.14/lib/net40/FSharp.Data.dll"
#r "D:/Code/Repos/TheVault/Projects/ShogeCapital/packages/FSharp.Charting.0.90.7/lib/net40/FSharp.Charting.dll"
#load "SeriesExtensions.fs"
#load "Main.fs"
#load "Agent.fs"
#load "../packages/Deedle.1.0.6/Deedle.fsx"
#load "../packages/FSharp.Charting.0.90.7/FSharp.Charting.fsx"
open Deedle
open System
open Agents
open main
open FSharp.Data
open FSharp.Charting
open MathNet.Numerics.Statistics

let batchSize = 70
let startDate = new DateTime(2011, 01, 01)
let endDate = new DateTime(2012, 01, 01)
let dates = getNYSEDates startDate endDate |> Array.map(fun x -> x.ToString()) 
let symbols = [|"AAPL"; "MSFT"; "XOM"; "SPX"; "GLD"|]
let stocks = loadStocks symbols startDate endDate

let d = series[1 => 0.4; 2 => 0.8; 3 => 1.]
let productive_d = d + 1.
productive_d |> Series.scanValues (fun x y -> y * x) 1.
(stocks |> normalized |> DisplayChart).ShowChart()

printf "Event block began \n"
let sw = System.Diagnostics.Stopwatch.StartNew()
let snP = getSnP500Symbols()
let snpSymbols = "SPX"::(snP |> Array.map(fun x -> x.Ticker) |> Seq.toList)
let sdata = loadStocksParallel (snpSymbols |> List.toArray) startDate endDate batchSize
printf "loading data took %d seconds \n" (sw.ElapsedMilliseconds / 1000L)
let profiler = EventProfiler("SPX", fun symRet mktRet -> mktRet <= -0.05 && symRet >= 0.05)
//let events  = profiler.FindAllEvents sdata (getNYSEDates startDate endDate)
let events2  = profiler.ProfileAll sdata (getNYSEDates startDate endDate) 20
printf "FINISHED %d seconds" (sw.ElapsedMilliseconds / 1000L)

let dailyRets = dailyReturns stocks
let curried = correlationChart dailyRets
(curried "GLD" "XOM").ShowChart()
(eventsChart events2).ShowChart()