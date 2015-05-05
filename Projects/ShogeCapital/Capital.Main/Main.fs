module Capital.Engine
module dp = Capital.DataProviders
open Deedle
open System
open FSharp.Data
open MathNet.Numerics.Statistics
open Capital.Extensions
open Capital.DataStructures

type SnPData = JsonProvider<"http://data.okfn.org/data/core/s-and-p-500-companies/r/constituents.json">

let private freeBase = FreebaseData.GetDataContext()
let dt(y,m,d) = DateTime(y,m,d)
let globalDataProvider = dp.YahooDataProvider()
let getStockData = globalDataProvider.GetStockData

let getSnP500Symbols() = 
    SnPData.GetSamples()
    |> Seq.map(fun x -> {Ticker= x.Symbol; CompanyName =x.Name})
    |> Seq.toArray


let getAllSymbols() =
    freeBase.``Products and Services``.Business.``Stock exchanges``.Individuals100.NYSE.``Companies traded``
    |> Seq.map(fun x -> {Ticker = x.``Ticker symbol``; CompanyName = x.Name; })



let backFill (ser : double seq) = 
    let backfiller (l : double list) =
        match l with 
        |[v] -> v
        |[v;u] -> if Double.IsNaN(u) then v else u

    ser |> Seq.toList |> List.slide(2) |> List.map(backfiller )


let toSeries selector ticks : Series<DateTime,'a> =
    let values = ticks |> Seq.map(selector) 
    let dates : DateTime seq = ticks |> Seq.map( fun x -> x.Date)
    Series(dates,values)

let getNYSEDates fromDate toDate = 
      (getStockData "AAPL" fromDate toDate) 
      |> Array.filter(fun x -> not <| Double.IsNaN(x.AC))
      |> Array.map(fun x -> x.Date)
      |> Array.sort
 
let loadStocks symbols startDate endDate =
    [for symbol in symbols ->
        symbol =>
            let data = getStockData symbol startDate endDate
            (data |> toSeries (fun x -> x.AC)) |> Series.sortByKey
    ]
    |> Frame.ofColumns
    |> Frame.fillMissing(Direction.Backward)


//// Normalize returned values so they can be easily compared
let normalized stocks = 
    stocks 
    |> Frame.mapColValues (fun os -> 
        let osAsFloat = os.As<float>()
        let firstItem = osAsFloat.GetAt(0)
        osAsFloat / firstItem)
 

    
// Calculate daily returns
let mapStocks f stocks =
    let s = Frame.sortRowsByKey stocks
    s |> Frame.mapColValues (fun os -> 
        os.As<decimal>() 
        |> Series.pairwiseWith (fun k (v1, v2) -> f v1 v2))

let dailyReturns stocks =
    mapStocks (fun v1 v2 -> v2 - v1/v1) stocks


 
