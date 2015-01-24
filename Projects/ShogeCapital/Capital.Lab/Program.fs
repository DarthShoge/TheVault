// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open main
open Agents
open System

[<EntryPoint>]
let main argv = 
    let startDate = new DateTime(2011, 01, 01)
    let endDate = new DateTime(2012, 01, 01)
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
    let profiler = EventProfiler("SPX", fun symRet mktRet -> mktRet <= -0.05 && symRet >= 0.05)
    let events  = profiler.FindAllEvents sdata (getNYSEDates startDate endDate)
    let events2  = profiler.ProfileAll sdata (getNYSEDates startDate endDate) 20

    (eventsChart events2).SaveChartAs("cor.jpg",FSharp.Charting.ChartTypes.ChartImageFormat.Jpeg)
    (curried "GLD" "XOM").SaveChartAs("cor.jpg",FSharp.Charting.ChartTypes.ChartImageFormat.Jpeg)
    printfn "%A" argv
    printf "FINISHED %d seconds" (sw.ElapsedMilliseconds / 1000L)
    Console.ReadLine()
    0 // return an integer exit code
