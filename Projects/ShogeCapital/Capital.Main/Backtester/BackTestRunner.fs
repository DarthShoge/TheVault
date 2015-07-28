module Capital.Main.BackTesting
open Capital.DataStructures
open Capital.Agents
open Deedle
open Capital.Main.MarketSimulation
open System

type Runner<'a>(strategy : 'a*Tick seq -> Series<DateTime,Order> ,config : 'a,resultsFolder) =

    let runBacktest(data : Frame<DateTime,string>) cash =
//        let range = {Start = date; End = date.AddYears(1).AddDays(-1.)}
//        let rawSymbols = symbols |> Seq.map(fun x -> x.Ticker)
//        let data = loadStocksParallelWith (fun x -> x) rawSymbols range.Start range.End 7 
        let backtster = BackTester()
        let backtest = data |> Frame.mapCols(fun _ y-> strategy(config, y.As<Tick>().Values) )
        let backtestOwned = backtest |> Frame.mapCols(fun _ y-> backtster.GetOwnedOrders(y.As<Order>() ))
        let pricesAtClose = data |> Frame.mapCols(fun _ y -> y.As<Tick>() |> Series.map(fun k v -> v.AC))
        let ownedcflow = backtster.GetOwnedCash(backtestOwned,pricesAtClose)
        let cflow = backtster.GetBacktestCash( backtest, pricesAtClose,cash )
        let fullcflow = cflow + ownedcflow
        let cashflows = frame ["Raw Cashflow" => cflow;"Owner Cashflow" => ownedcflow;"Full Cashflow" => fullcflow]
        {Cashflows = cashflows; ClosingPrices =pricesAtClose; Orders = backtest; Holdings = backtestOwned }

    let runAndSave (data : Frame<DateTime,string>) cash =
        let results = runBacktest data cash
        let createFilePath s = sprintf @"%s\%s\%s" resultsFolder (DateTime.Now.ToString()) s 
        results.Orders.SaveCsv(createFilePath "backtest.csv")
        results.Holdings.SaveCsv(createFilePath "backtestOwned.csv")
        results.ClosingPrices.SaveCsv(createFilePath "pricesAtClose.csv")
        results.Cashflows.SaveCsv(createFilePath "cashflows.csv")

    new(strategy : 'a*Tick seq -> Series<DateTime,Order> ,config : 'a) = 
        Runner<'a>(strategy,config,sprintf @"D:\temp%s" (DateTime.Now.ToShortTimeString()))

    member my.RunBacktest(data : Frame<DateTime,string>) cash=  runBacktest data cash
    member my.RunAndSave(data : Frame<DateTime,string>) cash =  runAndSave data cash
     
