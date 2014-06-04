module DescriptiveStatisticsModule
open Utilities
open DataStructures
open MathNet
open System

let cube x = x*x*x
let sqr x = x*x
let (^) x s = Math.Pow(x,s)
    
let generateHistogram n (values : 'a list) = 
    match values with
    |[] -> [] 
    |values -> values |> List.sort 
            |> List.cut n
            |> List.map(fun x -> HistogramBucket(x |> Seq.toArray))



let sampleVariance (xt: float seq) = 
    let mBar = MathNet.Numerics.Statistics.Statistics.Mean(xt)
    let n = xt |> Seq.length |> float
    (xt |> Seq.map(fun x -> Math.Pow(x - mBar,2.)) |> Seq.sum)
    / (n - 1.)

let populationVariance (xt: float seq) = 
    let mu = MathNet.Numerics.Statistics.Statistics.Mean(xt)
    let n = xt |> Seq.length |> float
    (xt |> Seq.map(fun x -> Math.Pow(x - mu,2.)) |> Seq.sum) / n


let sampleStandardDeviation xt = sqrt (sampleVariance xt)
let populationStandardDeviation xt = sqrt (populationVariance xt)
let standardDeviation xt f = sqrt (f xt)

let private calculateByMoments k (xt: float seq) = 
    let mBar = MathNet.Numerics.Statistics.Statistics.Mean(xt)
    let sigma = sampleStandardDeviation xt
    let kMeans = xt |> Seq.map(fun x -> (x - mBar)^k) |> Seq.sum
    let n = xt |> Seq.length |> float 
    let sampleAdjust =  n / ((n - 1.) * (n - 2.))
    (kMeans * sampleAdjust) / (sigma^k)

let kurtosis xt = calculateByMoments 4. xt 

let skewness xt  = calculateByMoments 3. xt 

let getBacktestDescriptiveStatistics backtest =
    let returns = backtest.Returns 
                    |> Seq.map(fun (x,y) -> y) 
                    |> Seq.toArray
                    |> Array.rev
    {   
        Mean =  MathNet.Numerics.Statistics.Statistics.Mean(returns)
        Variance = sampleVariance returns;
        StandardDev = sampleVariance |> standardDeviation returns;
        Kurtosis = kurtosis returns; 
        Skew = skewness returns;  
        Histogram = [||] 
    }

type AverageType = 
    |ByMean
    |ByMedian

type AverageCalculator() =  
    member avg.GetAverage (values : float seq) = function
    |ByMean -> values |> MathNet.Numerics.Statistics.Statistics.Mean
    |ByMedian -> values |> MathNet.Numerics.Statistics.Statistics.Median
        
    member avg.GetMovingAverages size values byType =
        if size > (values |> Seq.length) then
            [avg.GetAverage values byType ]
        else
            values |> Seq.windowed size |> Seq.map(fun x -> avg.GetAverage x byType) |> Seq.toList
    