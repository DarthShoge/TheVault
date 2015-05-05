module Capital.DataStructures
open System
[<StructuredFormatDisplay("{AsString}")>]
type IndexConstituents = {Id : int ; Date:DateTime; Constituents: string seq; Index : string}
type Tick = {Date:DateTime ; O:double;H:double;L:double;C:double;AC:double; V:double}
    with 
    member x.AsString = sprintf "Date : %A AC : %f" x.Date x.AC

type Symbol = {Ticker:string; CompanyName:string; }
type DateRange = {Start:DateTime; End:DateTime}
type Period = |Month|Week|Day|Hour|Minute
type Order = Buy of double | Sell of double | AggregateOrder of Order * Order | NoOrder
                with
                member this.AbsoluteValue =    
                    let rec addBranch c =
                        match c with 
                        |NoOrder -> 0.00
                        |Buy(x) -> x
                        |Sell(x) -> -x
                        |AggregateOrder(x,y) -> (addBranch x) + (addBranch y)
                    addBranch this

                member x.IsActiveOrder = 
                    match x with
                    |Buy(_) -> true 
                    | Sell(_) -> true
                    | AggregateOrder(_,_) -> true
                    | _ -> false

                override x.ToString() =
                    if x.AbsoluteValue >= 1.0000 then
                        sprintf "Buy %f" x.AbsoluteValue
                    else if x.AbsoluteValue <= -1.0000 then
                        sprintf "Sell %f" x.AbsoluteValue
                    else
                        "No Order"


                                        