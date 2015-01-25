module Capital.Charting
open Deedle
open FSharp.Charting
open Capital.Engine
open MathNet.Numerics.Statistics

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


//// Display normalized values of all the indexes, looks like bullyish equities market is back :-)

let DisplayChart (vals: Frame<'a,string>) =
    Chart.Combine
        [ for s in vals.GetAllColumns() 
            -> Chart.Line(s.Value |> Series.observations, Name = s.Key) ]
    |> Chart.WithLegend (Docking = ChartTypes.Docking.Top)
