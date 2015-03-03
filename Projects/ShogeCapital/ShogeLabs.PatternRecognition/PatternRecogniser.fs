module ShogeLabs.PatternRecognition
open Capital.Extensions
open MathNet.Numerics.Statistics
open System

let calculateChange strt curnt =
    (curnt - strt) / abs(strt)

type PatternStrategy =
    |PointByPoint
    |TopAnchored
    |BottomAnchored


type Pattern = {PatternArray : double array; Outcome : double option ; PredictedOutcome : double option}
                with
                    static member (%) (p1 : Pattern ,p2 : Pattern) =
                        match p1.PatternArray.Length , p2.PatternArray.Length with 
                        |(0,0) -> {PatternArray  = [||]; Outcome = None; PredictedOutcome = None}
                        |(x,y) when x <> y ->  raise(System.ArgumentException())
                        |(_,_) ->  
                            let diffs = p1.PatternArray 
                                        |> Array.zip p2.PatternArray 
                                        |> Array.map (fun (x,y) -> 1.0 - abs(calculateChange x y))
                            {PatternArray  = diffs; Outcome = Some(diffs |> Array.average); PredictedOutcome = None}
                    member x.Correlate p2 = MathNet.Numerics.Statistics.Correlation.Pearson(x.PatternArray,p2.PatternArray)
                        

type PatternSummary = {Patterns: Pattern array; MainPattern : Pattern; }    
                        with
                            member x.PossibilityOfRise = 
                                let allValues =[|yield x.MainPattern; for p in x.Patterns -> p |]
                                if allValues.Length < 2 then
                                    0.0
                                else
                                    match x.MainPattern.Outcome with
                                    | Some(_) ->(double)(allValues |> Array.filter(fun x -> x.Outcome.IsSome && x.Outcome.Value > 0.)).Length / (double)allValues.Length
                                    | None ->(double)(x.Patterns |> Array.filter(fun x -> x.Outcome.IsSome && x.Outcome.Value > 0.)).Length / (double)allValues.Length
                            member x.AverageOutcomes =
                                if x.Patterns.Length = 0 then
                                    0.0
                                else
                                    x.Patterns |> Seq.averageBy(fun x -> if x.Outcome.IsSome then x.Outcome.Value else 0.0)

type PatternRecogniser(lookback, lookforward,calcStrategy ) =

    let calcPointByPoint v =
        v |> Seq.windowed(2)
          |> Seq.map(fun x -> 
                    let v1 = x.[0]
                    let v2 = x.[1]
                    calculateChange v1 v2
                    )   
         |> Seq.toArray

    let calcTopAnchored v  =
        let top = v |> Seq.nth 0
        v |> Seq.skip 1
        |> Seq.map(fun x -> calculateChange top x)
        |> Seq.toArray

    let calcBottomAnchored v  =
        let valLength = (v |> Seq.length) - 1
        let bottom = v |> Seq.nth valLength 
        v |> Seq.take valLength 
        |> Seq.map(fun x -> calculateChange bottom x)
        |> Seq.toArray

    let (|Safe|Unsafe|) (x: double array) = 
        let endIndex = (lookback + lookforward) - 1
        if lookforward > 0 && x.Length > endIndex then
            Safe
        else
            Unsafe
         
    new(lookback,lookforward) = PatternRecogniser(lookback,lookforward ,PointByPoint)
    member this.ToPattern (v : double seq) = 
        let ar = v |> Seq.toArray
        if ar.Length = 0 then
            {PatternArray = [||]; Outcome = None; PredictedOutcome = None}
        else
            let observations = v |> Seq.tryTake lookback |> Seq.toList
            let pat = match calcStrategy with
                |TopAnchored -> calcTopAnchored observations
                |BottomAnchored -> calcBottomAnchored observations
                |_ -> calcPointByPoint observations
            let outcome = match ar with 
                          | Safe -> Some(calculateChange ar.[lookback - 1] ar.[lookforward + lookback - 1])
                          | Unsafe -> None
            let predictedOc = match ar with 
                          |Safe -> Some(calcPointByPoint ar.[(lookback - 1)..(lookback + lookforward - 1)] |> Array.average) 
                          | Unsafe -> None
            {PatternArray = pat; Outcome = outcome; PredictedOutcome = predictedOc}