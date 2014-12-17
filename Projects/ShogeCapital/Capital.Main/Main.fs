module main
open Deedle
open System
open FSharp.Data
open FSharp.Charting
open MathNet.Numerics.Statistics
open Capital.Extensions

type Tick = {Date:DateTime ; O:double;H:double;L:double;C:double;AC:double; V:double}
type Symbol = {Ticker:string; CompanyName:string; }
type StockData = CsvProvider<"D:\\Code\Repos\\TheVault\\Projects\\ShogeCapital\\Capital.Main\\table.csv">
type SnPData = JsonProvider<"http://data.okfn.org/data/core/s-and-p-500-companies/r/constituents.json">

let private worldBank = WorldBankData.GetDataContext()
let private freeBase = FreebaseData.GetDataContext()

let getSnP500Symbols() = 
    SnPData.GetSamples()
    |> Seq.map(fun x -> {Ticker= x.Symbol; CompanyName =x.Name})
    |> Seq.toArray

let urlFor symbol (startDate:System.DateTime) (endDate:System.DateTime) = 
    sprintf "http://ichart.finance.yahoo.com/table.csv?s=%s&a=%i&b=%i&c=%i&d=%i&e=%i&f=%i" 
        symbol
        (startDate.Month - 1) startDate.Day startDate.Year 
        (endDate.Month - 1) endDate.Day endDate.Year

let getAllSymbols() =
    let parse_d (d : string) = 
        match d with
        |null -> None
        |x when x.ToCharArray() |> Array.length <= 4 -> Some(DateTime(x.AsInteger(),1,1))
        |x -> Some(DateTime.Parse(x))

    freeBase.``Products and Services``.Business.``Stock exchanges``.Individuals100.NYSE.``Companies traded``
    |> Seq.map(fun x -> {Ticker = x.``Ticker symbol``; CompanyName = x.Name; })

let getStockData sym startdate (enddate) : Tick array =
    StockData.Load(urlFor sym startdate enddate).Rows 
    |> Seq.map(fun x -> {Date=x.Date ; O=(double)x.Open; H=(double)x.High; L= (double)x.Low; C = (double)x.Close;AC = (double)x.``Adj Close``;V=(double)x.Volume})
    |> Seq.toArray

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
 
//// Display normalized values of all the indexes, looks like bullyish equities market is back :-)

let DisplayChart (vals: Frame<'a,string>) =
    Chart.Combine
        [ for s in vals.GetAllColumns() 
            -> Chart.Line(s.Value |> Series.observations, Name = s.Key) ]
    |> Chart.WithLegend (Docking = ChartTypes.Docking.Top)
    
// Calculate daily returns
let mapStocks f stocks =
    let s = Frame.sortRowsByKey stocks
    s |> Frame.mapColValues (fun os -> 
        os.As<decimal>() 
        |> Series.pairwiseWith (fun k (v1, v2) -> f v1 v2))

let dailyReturns stocks =
    mapStocks (fun v1 v2 -> v2/v1 - 1.0m) stocks


 
// Visualise and calculate correlation between two indexes
let correlationChart (dailyReturns :Frame<'a,'b>) index1 index2 =

    let values1 = dailyReturns.GetColumn<float>(index1) |> Series.values
    let values2 = dailyReturns.GetColumn<float>(index2) |> Series.values
    let correlationValue = Correlation.Pearson(values1, values2)    
    Chart.Point (Seq.zip values1 values2, 
        Name = sprintf "Correlation coefficient between %s and %s is %f" 
                       index1 index2 correlationValue)
    |> Chart.WithLegend (Docking = ChartTypes.Docking.Top)    
    |> Chart.WithXAxis (Enabled = false) 
    |> Chart.WithYAxis (Enabled = false)



type EventProfiler(marketSymbol, eventPredicate) =
    let findEvents symbol (stockFrame : Frame<DateTime,string>) rawDates =
        let marketSeries = stockFrame.[marketSymbol]
        let nonMarketSeries = stockFrame |>  Frame.filterCols(fun x y -> x <> marketSymbol)
        let dates = rawDates
                    |> Array.sortBy(fun (x: DateTime) -> -x.Ticks)
                    |> Array.toList
        let rec innerFindEvents  (ser : Series<DateTime,double>) (dates: DateTime list) accu = 
            match dates with
            |[] -> accu
            | c::[] -> accu
            |today::nextDays ->
                let result = match ser with
                    |s when (s |> Series.countValues) < 2 -> (today => 0)
                    |s when not <| s.ContainsKey(today) -> (today => 0)
                    |symSer -> 
                        let getDailyReturnForToday (series : Series<DateTime,double>) = 
                            let seriesToday, seriesYester = series.[today], series.[nextDays.Head]
                            (seriesToday/seriesYester) - 1.
                        let mktSer = marketSeries
                        let symReturn = getDailyReturnForToday symSer 
                        let mktReturn = getDailyReturnForToday mktSer
                        if eventPredicate symReturn mktReturn then (today => 1) else (today => 0)
                innerFindEvents ser nextDays (result::accu)
        let intermediateResult =  Series.ofObservations (innerFindEvents stockFrame.[symbol] dates [])
        intermediateResult
    new(marketSymbol) = EventProfiler(marketSymbol, fun symRet mktRet -> symRet <= -0.03 && mktRet >= 0.02)
    member x.FindEvents stock dates =  findEvents stock dates
    member x.FindAllEvents (stocks : Frame<DateTime,string>) dates = 
        stocks |> Frame.map(fun col sers -> findEvents sers dates)
//        let results = nonMarketSeries |> Frame.mapCols(fun c rows ->
//                RowSeries
//            )
