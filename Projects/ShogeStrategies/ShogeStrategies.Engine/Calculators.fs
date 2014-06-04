module Calculators
open MathNet
open System
open DataStructures

//type ReturnsCalculator() = 
//    let round (d: int) (x: float) =
//        let rounded = Math.Round(x,d)
//        if rounded < x then x + Math.Pow(10.0,float -d) else rounded
//
//    let calculateSimpleReturns = function
//        |Long(_) -> 0m
//        |Short(_) -> 0m
//        |UnFilledOrder(_) -> 0m
//        |DeadOrder(opened,closed) ->  
//                            let calculate pt p0 = if p0 = 0m then 0m else (pt - p0)/p0
//                            match opened with
//                            |Long(_) -> calculate closed.Value.Open opened.GetPoint.Value.Open
//                            |Short(_) -> calculate opened.GetPoint.Value.Open closed.Value.Open
//
//    let calculateCCReturns pt =
//        match pt with 
//        |Long(_) -> 0m
//        |Short(_) -> 0m
//        |UnFilledOrder(_) -> 0m
//        |DeadOrder(_)  ->   let RT = calculateSimpleReturns pt;
//                            log(1.0 +  float RT) |> decimal
//
//    member x.CalculateSimpleReturns pts =
//            (pts |> Seq.map( fun (pi) -> 1.0m + (calculateSimpleReturns pi))|> Seq.reduce( fun Ri Ry -> Ri * Ry )) - 1.0m
//
//    member x.CalculateCompoundedReturns pts = (pts|> Seq.map(fun pi -> calculateCCReturns pi)|> Seq.sumBy(fun x -> x) )