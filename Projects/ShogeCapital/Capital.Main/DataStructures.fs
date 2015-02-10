module Capital.DataStructures
open System
type Tick = {Date:DateTime ; O:double;H:double;L:double;C:double;AC:double; V:double}
type Symbol = {Ticker:string; CompanyName:string; }
type DateRange = {Start:DateTime; End:DateTime}

