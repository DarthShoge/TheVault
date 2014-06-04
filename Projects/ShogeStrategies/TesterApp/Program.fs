open ShogeStrategies.Strategies
open DataStructures
open NewStrategyModule
open ShogeStrategies.Lab
open MoneyManagerModule
open DescriptiveStatisticsModule
open FSharp.Data
open ShogeStrategies.Data
open Microsoft.FSharp.Collections
open System

let data = FreebaseData.GetDataContext()


let riskMan = SimpleRiskManager(0.1m)
let backtester = new Backtester(riskMan.Offer);

let backtest entryDirection exitStrategy startBal data = 
    let entry = TrendFollowerEntry()
    let exit = TrendFollowerExit()
    backtester.Backtest (entry.EnterToOrder entryDirection ) (exit.ExitOrder exitStrategy)  data 5 startBal 


let getReturns r = 
    match r with 
    | [] -> 0m
    | x::_ -> let (m,y) = x; 
              y

type BacktestRunnerHarness(entry, exit,dataProvider : IStockDataProvider, riskManager: IRiskManager) = 
    let purchaseFunc startBalance window x = 
            let data = dataProvider.GetSymbolData(x) |> Seq.takeWhile(fun g -> g.Start > DateTime(2010,01,01)) |> Seq.sortBy(fun g ->g.Start)
            let backtestResult = Backtester(riskMan.Offer).Backtest entry exit (data |> Seq.toList) window startBalance
            let ticker,result = x,backtestResult
            do printf "\nfinished %A returns: %g balance: %g on Thread: %A" x (getReturns result.Returns) result.Balance System.Threading.Thread.CurrentThread.ManagedThreadId
            ticker,result

    member this.RunAllTickersParallely windowSize startBalance tickers = 
            tickers                 
            |> PSeq.map(purchaseFunc startBalance windowSize)
            |> PSeq.filter(fun x -> match x with |name,value -> value.Balance <> startBalance)
            |> PSeq.toArray

    member this.RunAllTickersAsyncParallely windowSize startBalance (tickers: string seq) = 
            tickers                 
            |> Seq.map(fun x -> async {return purchaseFunc startBalance windowSize x})
            |> Async.Parallel
            |> Async.RunSynchronously


type BacktestResultsComparer() =
    member x.Compare r1 r2 =
        (r1.Mean - r2.Mean, r1.StandardDev - r2.StandardDev)

[<EntryPoint>]
let main argv = 
//    do tester().saveRow 1
    let dataProvider = StockDataProvider() :> IStockDataProvider
    let entry = TrendFollowerEntry().EnterToOrder (up 4)
    let exit = TrendFollowerExit().ExitOrder (down 3)
    let btestStrapper = BacktestRunnerHarness( entry,exit,dataProvider,riskMan)
    
    let allTickers = 
            data.``Products and Services``.Business.``Stock exchanges``.Individuals.NASDAQ.``Companies traded`` 
            |> Seq.filter(fun x -> x.``End date`` = null )
            |> Seq.map(fun x -> x.``Ticker symbol``) 
            |> Seq.toArray

    let p = {Lookback = 30; Lag = 20}
    let btest = BackTestRunner(MovingAverageEntryStrategy(p).Enter,MovingAverageExitStrategy(p).Exit).Backtest
    let defaultTest = BackTestRunner(DefaultEntryStrategy().Enter,DefaultExitStrategy().Exit).Backtest
    let ndx = dataProvider.GetSymbolDataForDates "^NDX" (DateTime.Now.AddYears(-6)) DateTime.Now  |> Seq.toList
    let msft = dataProvider.GetSymbolDataForDates "MSFT" (DateTime.Now.AddYears(-6)) DateTime.Now  |> Seq.toList 
    let market =  defaultTest ndx |> getBacktestDescriptiveStatistics
    let values = ["MSFT";"AAPL";"YHOO";"ITG"] |> List.map(fun x -> 
        let results = btest ((dataProvider.GetSymbolDataForDates x  (DateTime(2005,02,13)) (DateTime.Now) |> Seq.toList )|> Seq.toList)
        (x,  results |> getBacktestDescriptiveStatistics),results)
    
    let msftDesc,msftRet = values.[0]
    let rComp = BacktestResultsComparer().Compare market (snd msftDesc)

    let chartData = dataProvider.GetSymbolData("AMTY")
    printf "finished getting data"
    
    0 // return an integer exit code
