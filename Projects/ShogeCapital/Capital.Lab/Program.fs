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
open System.Windows.Forms
open ShogeLabs.Strategies.PatternRecognition
open Capital.DataProviders
open Deedle
open System.Windows.Forms.DataVisualization.Charting
open System
open System.Diagnostics
open Capital.Main.MarketSimulation
open ShogeLabs.Strategies.Patterns
open Capital.Data

let getSnP500SymbolsByDate date = 
    Capital.Data.Query.IndexConstituentRepository().GetIndexConstituent("S&P500",date).Constituents
    |> Seq.map(fun x -> {Ticker= x; CompanyName =x})
    |> Seq.toArray

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
//    evchart.SaveChartAs("ev.jpg",FSharp.Charting.ChartTypes.ChartImageFormat.Jpeg)
    (curried "GLD" "XOM").SaveChartAs("cor.jpg",FSharp.Charting.ChartTypes.ChartImageFormat.Jpeg)
    printf "FINISHED %d seconds" (sw.ElapsedMilliseconds / 1000L)
    Console.ReadLine()  


let patts() = 
    let snP = getSnP500Symbols()
    let batchSize = 70
    let snpSymbols = "SPX"::(snP |> Array.map(fun x -> x.Ticker) |> Seq.toList)
    let dates = {Start = (DateTime(2001,01,01)); End = (DateTime(2014,01,01))}
    ////let sdata = loadStocksParallel (snpSymbols |> List.toArray) dates.Start dates.End batchSize
    //
    //let run = PatternRunner(YahooDataProvider().GetStockData)
    //let chart = run.ChartPatterns(20,15,0.5,dates,"AMZN")


    let aapl = YahooDataProvider().GetStockData "zeus" (DateTime(2001,01,01)) (DateTime.Now) 
    let lookback = 20
    let look4wrd = 10
    let look4wrdCalc = 10
    let patternFinder = PatternRecogniser(lookback,look4wrdCalc,TopAnchored)
    let patterns = aapl |> Seq.map(fun x -> x.AC) 
                        |> Seq.windowed(lookback + look4wrd + 1) 
                        |> Seq.map(fun x -> patternFinder.ToPattern x)
                        |> Seq.toArray

    let cp = patternFinder.ToPattern (aapl |> Seq.map(fun x -> x.AC) |> Seq.toArray).[(aapl.Length - (lookback + 1))..(aapl.Length - 1)] //patterns.[patterns.Length - 1]

    let bestPatterns = patterns.[..patterns.Length - 1]  
                        |> Array.mapi(fun i x -> (i,cp.Correlate x,cp % x))
                        |> Array.filter(fun (i,corr,x) ->  corr >= 0.90)
    //                        x.Outcome.IsSome && x.Outcome.Value >= 0.70)

    let similars = (bestPatterns |> Array.map(fun (i,corr,x) -> patterns.[i]))

    let allValues =[|
            yield cp
            for p in similars -> p
        |]

    let possibilityOfpositive = (double)(allValues |> Array.filter(fun x -> x.Outcome.IsSome && x.Outcome.Value > 0.)).Length / (double)similars.Length

    let outcomeXPoint = (lookback + 1)
    let chart = Chart.Combine[
                            yield Chart.Line(cp.PatternArray,Name="main")
                                  |> Chart.WithSeries.Style(Color= Color.Black,BorderWidth = 5)
    //                        yield   Chart.Point([(outcomeXPoint + 1,cp.Outcome.Value)])
    //                                |> Chart.WithSeries.Marker(Style= MarkerStyle.Circle,Size=8,Color = if cp.Outcome.Value < 0. then Color.BurlyWood else Color.Blue)
                            for s in similars do
                                yield Chart.Line(s.PatternArray)
                                yield Chart.Point([(outcomeXPoint,s.Outcome.Value)])
                                        |> Chart.WithSeries.Marker(Style= MarkerStyle.Circle,Size = 8,Color = if s.Outcome.Value < 0. then Color.FromArgb(100, 255, 0, 0) else Color.FromArgb(100, 0, 255, 0)
                                        ,BorderWidth = 3 )
                        ]
                        |> Chart.WithTitle(sprintf "Probability of positive movement = %f" possibilityOfpositive)

    chart


[<EntryPoint>]
let main argv = 
    let dates = {Start = (DateTime(2012,01,01)); End = (DateTime(2014,01,01))}
    let snP = getSnP500SymbolsByDate dates.Start
    let batchSize = 70
    let snpSymbols = "SPX"::(snP |> Array.map(fun x -> x.Ticker) |> Seq.toList)
    //let sdata = loadStocksParallel (snpSymbols |> List.toArray) dates.Start dates.End batchSize
    let random = Random()
    let run = PatternRecognitionStrategy(YahooDataProvider().GetStockData)
//    let allValues = snpSymbols 
//                        |> Seq.map(fun sym -> 
//                            let symdata : Tick array = YahooDataProvider().GetStockData sym dates.Start dates.End
//                            run.Run(20,15,0.5,symdata |> Seq.toList) )
//                        |> Seq.filter(fun x -> x.PossibilityOfRise > 0.6) 
//                        |> Seq.toArray
//    let chart = run.ChartPatterns(20,15,0.5,dates,"AMZN")
//    patts().ShowChart() |> ignore
    let config =  { Lookback = 20;
                    LookForward = 5;    
                    Tolerance = 0.7;
                    GenerateOrderOn = fun pttrn -> pttrn.AverageOutcomes > 0.00000005;
                    Range = dates; 
                    Period = Capital.DataStructures.Day;
                    OrderSize = 100. }
    let backtster = BackTester()
    let randomUniverse = snpSymbols |> Seq.sortBy(fun x -> random.Next()) |> Seq.take 500 |> Seq.toArray
    let universe = [|"AAPL";"MSFT";"ADBE";"MMM";"AMZN";"BAC";"CSCO";"EBAY";"EA";"F";"ORCL";"PEP";"RL";"RHT";"SBUX";"COH";"DAL"|]
    let data = loadStocksParallelWith (fun x -> x) randomUniverse config.Range.Start config.Range.End 7 
    printf "all data loaded!!!"
    let strategy = PatternRecognitionStrategy(YahooDataProvider().GetStockData)
//    let amzn = YahooDataProvider().GetStockData "pep" config.Range.Start config.Range.End
    let timer = Diagnostics.Stopwatch.StartNew()
    let backtest = data |> Frame.mapCols(fun x y-> strategy.WalkForward(config, y.As<Tick>().Values) )
    let backtestOwned = backtest |> Frame.mapCols(fun x y-> backtster.GetOwnedOrders(y.As<Order>() ))
//    let values = backtest.Keys |> Seq.map(fun x -> amzn |> Seq.tryFind(fun t -> t.Date = x))
    let pricesAtClose = data |> Frame.mapCols(fun x y -> y.As<Tick>() |> Series.map(fun k v -> v.AC))
    let cash = 10000.
    let cflow = backtster.GetBacktestCash( backtest, pricesAtClose,cash )
    let ownedcflow = backtster.GetOwnedCash(backtestOwned,pricesAtClose)
    let fullcflow = cflow + ownedcflow
    let cashflows = frame ["Raw Cashflow" => cflow;"Owner Cashflow" => ownedcflow;"Full Cashflow" => fullcflow]

    let chr = Chart.Line( fullcflow.Observations |>Seq.map(fun x -> x.Key => x.Value),Name= sprintf "returns %f" (ShogeLabs.Strategies.PatternRecognition.calculateChange cash (fullcflow.LastValue()))) 
    use form = new Form(Width=400, Height=300, Visible=true, Text="Hello charting")
    chr.ShowChart()
    System.Windows.Forms.Application.Run(form)

    backtest.SaveCsv(@"D:\temp\backtest.csv")
    backtestOwned.SaveCsv(@"D:\temp\backtestOwned.csv")
    pricesAtClose.SaveCsv(@"D:\temp\pricesAtClose.csv")
    cashflows.SaveCsv(@"D:\temp\cashflows.csv")

    0 // return an integer exit code
