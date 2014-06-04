module StatisticsCalculatorTests
open NUnit.Framework
open FsUnit
open MathNet
open System
open DataStructures
open TestHelpers
open Calculators
open Utilities
open DescriptiveStatisticsModule

let shouldBeAbout f g =
    let epsilon = 0.0001
    let diff = f - g
    let result = (diff < epsilon) && (-diff < epsilon); 
    result |> should be True

        

[<TestFixture>]
type ``should be able to get statistics after a run of run performance``() =
    
    let tradeIn inAt outAt = DeadOrder(Long({Start = DateTime() ;Value = (oc inAt (inAt + 1m))},1)
                    ,{Start = DateTime() ;Value =(oc outAt (outAt + 1m))}) 

    [<Test>]
    member x.``should be able to calculate simple returns of single long trade``() =
        //arrange
        let trade = DeadOrder(Long({Start = DateTime() ;Value = (oc 90m 80m)},1)
                    ,{Start = DateTime() ;Value =(oc 100m 110m)}) 
        //act
        let results = ReturnsCalculator().CalculateSimpleReturns [trade]
        //assert
        results |> shouldBeAbout 0.111111


    [<Test>]
    member x.``should be able to calculate simple returns of single short trade``() =
        //arrange
        let trade = DeadOrder(Short({Start = DateTime() ;Value = (oc 100m 80m)},1)
                    ,{Start = DateTime() ;Value =(oc 90m 100m)}) 
        //act
        let results = ReturnsCalculator().CalculateSimpleReturns [trade]
        //assert
        results |> shouldBeAbout 0.1111

    [<Test>]
    member x.``should return -1 on a bankruptcy trade``() =
        //arrange
        let trade = DeadOrder(Long({Start = DateTime() ;Value = (oc 0.01m 0m)},1)
                    ,{Start = DateTime() ;Value =(oc 0m 0m)}) 
        //act
        let results = ReturnsCalculator().CalculateCompoundedReturns [trade]
        //assert
        results |> should equal -1.

    [<Test>]
    member x.``should be able to calculate continously compounded returns of single Long trade``() =
        //arrange
        let trade = DeadOrder(Long({Start = DateTime() ;Value = (oc 85m 95m)},1)
                    ,{Start = DateTime() ;Value =(oc 90m 100m)}) 
        //act
        let results = ReturnsCalculator().CalculateCompoundedReturns [trade]
        //assert
        results |> shouldBeAbout 0.0248


    [<Test>]
    member x.``should be able to calculate continuously compounded returns of single short trade``() =
        //arrange
        let trade = DeadOrder(Short({Start = DateTime() ;Value = (oc 90m 100m)},1)
                    ,{Start = DateTime() ;Value =(oc 85m 95m)}) 
        //act
        let results = ReturnsCalculator().CalculateCompoundedReturns [trade]
        //assert
        results |> shouldBeAbout 0.024823


    [<Test>]
    member x.``should be able to calculate overall returns over multiple simple returns``() =
        //arrange
        let (P1,P2,P3) = tradeIn 80m 90m,tradeIn 70m 90m,tradeIn 100m 90m
        //act
        let results = ReturnsCalculator().CalculateSimpleReturns [P1;P2;P3]
        //assert
        results |> shouldBeAbout 0.301785

    [<Test>]
    member x.``should be able to calculate overall returns over multiple compounded returns``() =
        //arrange
        let (P1,P2,P3) = tradeIn 80m 90m,tradeIn 70m 90m,tradeIn 100m 90m
        //act
        let results = ReturnsCalculator().CalculateCompoundedReturns [P1;P2;P3]
        //assert
        results |> shouldBeAbout 0.114539
    [<Test>]
    member x.``should be ignore any ordrs that are not dead when calculating simple returns``() =
        //arrange
        let (P1,P2,P3) = tradeIn 80m 90m,tradeIn 70m 90m,tradeIn 100m 90m
        let longPx = Long({Value =  (oc 1m 2m); Start = DateTime()},50)
        let shortPx = Short({Value =  (oc 1m 2m); Start = DateTime()},50)
        //act
        let results = ReturnsCalculator().CalculateSimpleReturns [P1;P2;P3;longPx;shortPx]
        //assert
        results |> shouldBeAbout 0.3017857

    [<Test>]
    member x.``should be ignore any ordrs that are not dead when calculating compounded returns``() =
        //arrange
        let (P1,P2,P3) = tradeIn 80m 90m,tradeIn 70m 90m,tradeIn 100m 90m
        let longPx = Long({Value =  (oc 1m 2m); Start = DateTime()},50)
        let shortPx = Short({Value =  (oc 1m 2m); Start = DateTime()},50)
        //act
        let results = ReturnsCalculator().CalculateCompoundedReturns [P1;P2;P3;longPx;shortPx]
        //assert
        results |> shouldBeAbout 0.11453



[<TestFixture>]
type ``given I want to turn a set of numbers into a histogram``() =
    
    let generateHistogram = DescriptiveStatisticsModule.generateHistogram

    [<Test>]
    member x.``when no values are passed in then single zero valued histogram is returned``() =
        //given 
        let values = []
        //when
        let hist = generateHistogram 2 values
        //then
        hist |> should be Empty


    [<Test>]
    member x.``when split by 0 then no values mapped``() =
        //given 
        let values = [1.0;2.0;3.0;4.0;5.0;6.0]
        //when
        let hist = generateHistogram 0 values
        //then
        hist |> List.length |> should equal 0
         
    [<Test>]
    member x.``when a split by 2 ranks then should create two ranked vectors sorted by initial values``() =
        //given 
        let values = [13.1;32.22;14.33;34.4;55.5;6.6]
        //when
        let hist = generateHistogram 3 values
        //then
        hist |> List.length |> should equal 2
        ( List.nth hist 1).Values |> should equal [6.6;13.1;14.33]
        ( List.nth hist 0).Values |> should equal [32.22;34.4;55.5]

    [<Test>]
    member x.``when a histogram is split by greater number than available values then available it returns a single vector``() =
        //given 
        let values = [1.;2.;3.;5.]
        //when
        let hist = generateHistogram 5 values
        //then
        hist |> List.length |> should equal 1
        ( List.nth hist 0).Values |> should equal [1.;2.;3.;5.]


    [<Test>]
    member x.``when a histogram is split by a number less 0 it returns empty``() =
        //given 
        let values = [1.;2.;3.;5.]
        //when
        let hist = generateHistogram -4 values
        //then
        hist |> should be Empty


[<TestFixture>]
type ``given I want to calculate descriptive statistics on my models ``() =
    
    [<Test>]
    member x.``the sample variance of a simple set can be calculated``()=
        //given 
        let X = [600.;470.;170.;430.;300.]
        //when
        let sigmaSq = sampleVariance X
        //then
        sigmaSq |> should equal 27130.
    
    [<Test>]
    member x.``the sample standardDeviation of a simple set can be calculated``()=
        //given 
        let X = [85.;890.;4.;71.;150.;99.;210.]
        //when
        let sigma = sampleStandardDeviation X
        //then
        sigma |> should equal 304.27118230445745    
   
    [<Test>]
    member x.``the skewness of a simple set can be calculated``()=
        //given 
        let X = [1.;2.;3.;4.;5.;5.;3.;4.;36.;3.;2.;6.;80.;100.]
        //when
        let skewX = skewness X
        //then
        skewX |> should equal 2.0842823008335412




[<TestFixture>]
type ``given I want to find the descriptive statistics of my backtest``() =
    
    [<Test>]
    member x.``when i pass in backtest results with no data then i get 0 valued results``() =
        //given
        let backtestResults = {Returns = []; Assets = []; Trades = []}
        //when
        let descStats = getBacktestDescriptiveStatistics backtestResults
        //then
        descStats |> should equal     
            {   
                Mean = Double.NaN;
                Variance = 0.;
                StandardDev = 0.;
                Kurtosis = Double.NaN; 
                Skew = Double.NaN;  
                Histogram = [||] 
            }



    [<Test>]
    member x.``when i pass in backtest results with data then i get correct results``() =
        //given
        let returns = [0.1..0.1..0.5] |> List.map(fun x -> (DateTime(),x))
        let backtestResults = {Returns = returns; Assets = []; Trades = []}
        //when
        let descStats = getBacktestDescriptiveStatistics backtestResults
        //then
        descStats |> should equal     
            {
                Mean = 0.30000000000000004   
                Variance = 0.025;
                StandardDev = 0.15811388300841897;
                Kurtosis = 2.2666666666666666; 
                Skew = -0.0000000000000012799913687519221;  
                Histogram = [||] 
            }

