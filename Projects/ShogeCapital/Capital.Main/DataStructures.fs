module Capital.DataStructures
open System
[<StructuredFormatDisplay("{AsString}")>]
type Tick = {Date:DateTime ; O:double;H:double;L:double;C:double;AC:double; V:double}
    with 
    member x.AsString = sprintf "Date : %A AC : %f" x.Date x.AC

type Symbol = {Ticker:string; CompanyName:string; }
type DateRange = {Start:DateTime; End:DateTime}
type Period = |Month|Week|Day|Hour|Minute
type Order = Buy of double | Sell of double | AggregateOrder of Order * Order | NoOrder
                with
                member x.IsActiveOrder = 
                    match x with
                    |Buy(_) -> true 
                    | Sell(_) -> true
                    | AggregateOrder(_,_) -> true
                    | _ -> false
                                        