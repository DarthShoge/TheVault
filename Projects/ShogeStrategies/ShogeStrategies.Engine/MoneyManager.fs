namespace MoneyManagerModule
open DataStructures



type IBankAccount = 
    abstract member Balance : unit -> decimal

type IRiskManager =
    abstract member GetPortfolioBalance : ( unit -> cash) -> cash
    abstract member Offer :  cash -> stockPrice -> cash*units
    
type SimpleRiskManager(prcToUse) =
    new () = new SimpleRiskManager(0.01M)
    member me.GetPortfolioBalance (bankBalFunc : unit -> cash) =  bankBalFunc()
    member me.Offer balance prc = 
        if prc = 0m then
            0m,0
        else
            let usableCredit = balance * prcToUse
            let unitsToPurchase = floor usableCredit / prc
            (usableCredit, int unitsToPurchase)
    member my.PercentageOfRiskPerTrade with get() = prcToUse
    interface IRiskManager with
        member I.GetPortfolioBalance f = I.GetPortfolioBalance f
        member I.Offer f p = I.Offer f p


            