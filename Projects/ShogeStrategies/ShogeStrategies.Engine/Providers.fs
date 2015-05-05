namespace ShogeStrategies.Data
open System
open System.IO
open System.Net
open DataStructures



type IStockDataProvider =  
    abstract GetSymbolData : string -> OHLCPoint seq
    abstract GetSymbolDataForDates : string -> DateTime -> DateTime -> OHLCPoint seq

type FakeStockDataProvider() =
    let rand = new Random()

    let randomBetween v x =  rand.Next(v,x) |> decimal
    let getDataPointFromDelta seed date =    
        let growValByPrcntg value l h =
            let growthPrc = (randomBetween l h) /  100.0M
            let growth = growthPrc * value
            growth + value
        let getHLBy value s (predicate : decimal -> decimal ->bool) = 
            if(predicate value s) then
                    value
                else
                    s
        let seedclose = randomBetween 25 50
        let hiValue = getHLBy (growValByPrcntg seedclose 0 50) seed (fun x y -> x > y)
        let lowValue = getHLBy (growValByPrcntg seedclose -50 0) seed (fun x y -> x < y)
        {Start= date ;Value = {Hi=hiValue;
                                Low=lowValue;
                                Open=seed;
                                Close=seedclose;
                                AdjClose = seedclose;
                                Volume=randomBetween 1000 10000}}



    let rec createPriceSet (from:DateTime, toDate:DateTime, acc) =
        match from with
        |x when from.DayOfWeek = DayOfWeek.Saturday  || from.DayOfWeek = DayOfWeek.Sunday -> createPriceSet(from.AddDays(1.0),toDate,acc)
        |x when from >= toDate -> acc
        |_ ->   match acc with
                |[] -> 
                        let seed = randomBetween 20 75
                        let seedOHLC = getDataPointFromDelta seed from
                        let newTo = from.AddDays(1.0)
                        createPriceSet(newTo, toDate, [seedOHLC])
                |h::t-> 
                        let OHLC = getDataPointFromDelta h.Value.Close from
                        createPriceSet(from.AddDays(1.0), toDate, OHLC::h::t)

    let getMockSymbols(symbol: string) =  createPriceSet(new DateTime(1990,01,01),new DateTime(2013,01,01),[]) |> List.toSeq
    member x.GetStockSymbols from until = createPriceSet (from, until, [])
    interface IStockDataProvider with
        member x.GetSymbolData(symbol) = getMockSymbols(symbol)
        member x.GetSymbolDataForDates symbol from toDate = getMockSymbols(symbol)


type StockDataProvider() =

    let getSymbolFor(symbol: string) (fromDate: DateTime) (toDate: DateTime) = 
        let pageToRequest = sprintf "https://ichart.finance.yahoo.com/table.csv?s=%s&a=%i&b=%i&c=%i&d=%i&e=%i&f=%i&g=d&ignore=.csv" symbol (fromDate.Month - 1) fromDate.Day fromDate.Year (toDate.Month - 1) toDate.Day toDate.Year
        let req = HttpWebRequest.Create(pageToRequest)
        []

    interface IStockDataProvider with
        member x.GetSymbolData(sym) = getSymbolFor sym (DateTime(1986,02,13)) DateTime.Now |> List.toSeq
        member x.GetSymbolDataForDates(sym) frm too = getSymbolFor sym frm too |> List.toSeq

    
