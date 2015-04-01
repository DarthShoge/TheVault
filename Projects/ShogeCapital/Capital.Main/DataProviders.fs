module Capital.DataProviders
open FSharp.Data
open Capital.DataStructures
open System
open Deedle

type StockData = CsvProvider<"D:\\Code\Repos\\TheVault\\Projects\\ShogeCapital\\Capital.Main\\table.csv">
type BlackListedException(sym,why) =
    inherit Exception(sprintf "Symbol %s is blacklisted : %s" sym why)

type YahooDataProvider() =
    let blackList = 
        [("PLL","Has malformed data and missing values represented as 0's")        ]
    let urlFor symbol (startDate:System.DateTime) (endDate:System.DateTime) = 
        sprintf "http://ichart.finance.yahoo.com/table.csv?s=%s&a=%i&b=%i&c=%i&d=%i&e=%i&f=%i" 
        // sprintf "http://real-chart.finace.yahoo.com/table.csv?s=%s&d=%i&e=%i&f=%i&g=d&a=%i&b=%i&c=%i&ignore=.csv"
    
            symbol
            (startDate.Month - 1) startDate.Day startDate.Year 
            (endDate.Month - 1) endDate.Day endDate.Year
    let load sym startd endd =
        if(blackList |> List.exists(fun (blacklistedSym ,_) -> sym = blacklistedSym)) then
            let (blacklistedSym ,message) = blackList |> List.find(fun (bl ,_) -> sym = bl )
            raise  (BlackListedException(blacklistedSym,message))

        let urlToLoad = urlFor sym startd endd
        StockData.Load(urlToLoad).Rows 
        |> Seq.map(fun x -> {Date=x.Date ; O=(double)x.Open; H=(double)x.High; L= (double)x.Low; C = (double)x.Close;AC = (double)x.``Adj Close``;V=(double)x.Volume})
        |> Seq.toArray

    let getStockData sym startdate (enddate) : Tick array =
        let rec createEmptyTicks date arr = 
                        match date with
                        |e when date = enddate -> arr 
                        |(d: DateTime) -> createEmptyTicks (d.AddDays(1.)) (({Date = d;O = 0.;H = 0.;L = 0.; C = 0.; AC = 0.;V = 0.})::arr)
        try 
            load sym startdate enddate
        with
            | :? BlackListedException as b ->
                printf "%A \n" b.Message
                (createEmptyTicks startdate []) |> List.toArray
            | :? System.Net.WebException -> 
                printf "%A did not have data \n" sym
                (createEmptyTicks startdate []) |> List.toArray

    member x.GetStockData sym startdate enddate = 
            getStockData sym startdate enddate


//
//type GoogleTickDataProvider() =
    
