// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open main
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
    let curried = correlationChart dailyRets
    (curried "GLD" "XOM").SaveChartAs("cor.jpg",FSharp.Charting.ChartTypes.ChartImageFormat.Jpeg)
    printfn "%A" argv
    0 // return an integer exit code
