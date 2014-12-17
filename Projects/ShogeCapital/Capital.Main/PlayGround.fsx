// Learn more about F# at http://fsharp.net. See the 'F# Tutorial' project
// for more guidance on F# programming.

#r "System.Core.dll"
#r "D:/Code/Repos/TheVault/Projects/ShogeCapital/packages/Deedle.1.0.6/lib/net40/Deedle.dll"
#r "D:/Code/Repos/TheVault/Projects/ShogeCapital/packages/MathNet.Numerics.3.2.3/lib/net40/MathNet.Numerics.dll"
#r "D:/Code/Repos/TheVault/Projects/ShogeCapital/packages/FSharp.Data.2.0.14/lib/net40/FSharp.Data.dll"
#r "D:/Code/Repos/TheVault/Projects/ShogeCapital/packages/FSharp.Charting.0.90.7/lib/net40/FSharp.Charting.dll"
#load "SeriesExtensions.fs"
#load "Main.fs"
#load "../packages/Deedle.1.0.6/Deedle.fsx"
#load "../packages/FSharp.Charting.0.90.7/FSharp.Charting.fsx"
open Deedle
open System
open main
open FSharp.Data
open FSharp.Charting
open MathNet.Numerics.Statistics
// Define your library scripting code here

let startDate = new DateTime(2011, 01, 01)
let endDate = new DateTime(2012, 01, 01)
let dates = getNYSEDates startDate endDate |> Array.map(fun x -> x.ToString()) 
let snP = getSnP500Symbols()
let symbols = [|"AAPL"; "MSFT"; "XOM"; "SPX"; "GLD"|]
let stocks = loadStocks symbols startDate endDate

(stocks |> normalized |> DisplayChart).ShowChart()
let dailyRets = dailyReturns stocks
let curried = correlationChart dailyRets
(curried "GLD" "XOM").ShowChart()