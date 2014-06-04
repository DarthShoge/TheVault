module MovingAverageStratergyTests

open NUnit.Framework
open FsUnit
open System
open MathNet
open DataStructures
open TestHelpers
open NewStrategyModule
open Utilities
open DescriptiveStatisticsModule
open ShogeStrategies.Strategies


[<TestFixture>]
type ``Moving average entry stratergy should work properly``() =
    
    let sut = AverageCalculator()

    [<Test>]
    member x.``should be able to calculate a mean of a given set of numbers``()=
        //arrange
        let points = [8.0;15.0;47.0;89.0;12.0;14.0] |> List.toSeq
        //act
        let result = sut.GetAverage points ByMean 
        //assert
        result |> should equal 30.833333333333336

    [<Test>]
    member x.``should be able to calculate a median of a given set of numbers``()=
        //arrange
        let points = [8.0;15.0;47.0;89.0;12.0;14.0] |> List.toSeq
        //act
        let result = sut.GetAverage points ByMedian 
        //assert
        result |> should equal 14.5 

    [<Test>]
    member x.``should be able to calculate the average of empty set properly``() = 
        //arrange
        let points = Seq.empty
        //act
        let result = sut.GetAverage points ByMean 
        //assert
        result |> should equal nan


    [<Test>]
    member x.``should return two moving average results``() = 
        //arrange
        let points = [5.;5.;6.] |> List.toSeq
        //act
        let result = sut.GetMovingAverages 2 points ByMean 
        //assert
        result |> should equal [5.;5.5]


    [<Test>]
    member x.``should get average of the input if size is bigger than input sequence``()=
        //arrange
        let points = [7.;8.;9.] |> List.toSeq
        //act
        let result = sut.GetMovingAverages 5 points ByMean 
        //assert
        result |> should equal [8]
    //should be able to calculate the weighted moving average

     

[<TestFixture>]
type ``Given I want to enter a trade via a moving average strategy``() = 
    let enter (parameter : MovingAvgStrategyConfig ) (x : OHLCPoint list) = 
        MovingAverageEntryStrategy(parameter).Enter x

    [<Test>]
    member x.``when I have no points then strategy returns no order``()=
        //given
        let ohcls = []
        //when
        let result = enter {Lookback = 3; Lag = 0} ohcls
        //then
        result |> should equal UnFilledOrder


    [<Test>]
    member x.``when i set a lookback of 3 day if there are not enough days of data i get an unfilled order``()=
        //given
        let ohcls = [
                    {Start = seedDate 2;Value = oc 5m 10m};
            {Start = seedDate 3;Value = oc 5m 10m}]
        //when
        let result = enter {Lookback = 3; Lag = 0} ohcls
        //then
        result |> should equal UnFilledOrder


    [<Test>]
    member x.``when i set a lookback of 3 days if the 4th day closes above the average i go long on the 5th open ``()=
        //given
        let buyOn =  {Start = seedDate 5;Value = oc 5m 10m};
        let ohcls = [{Start = seedDate 1;Value = oc 1m 1m};
                    {Start = seedDate 2;Value = oc 1m 1m};
                    {Start = seedDate 3;Value = oc 1m 1m};
                    {Start = seedDate 4;Value = oc 1m 5m};
                    buyOn]
        //when
        let result = enter {Lookback = 3; Lag = 0} ohcls
        //then
        result |> should equal (Long(buyOn,0))


    [<Test>]
    member x.``when i set a lookback of 3 days with a lag of 2 days then if the price has not been above the avg for longer than the lag dont buy``()=
        //given
        let ohcls = [{Start = seedDate 1;Value = oc 4m 1m};
                    {Start = seedDate 2;Value = oc 11m 1m};
                    {Start = seedDate 3;Value = oc 8m 1m};
                    {Start = seedDate 4;Value = oc 4m 5m};
                    {Start = seedDate 5;Value = oc 14m 5m};
                    ]
        //when
        let result = enter {Lookback = 4; Lag = 2} ohcls
        //then
        result |> should equal UnFilledOrder

    [<Test>]
    member x.``when i set a lookback of 3 days with a lag of 2 days then if the price has been above the avg for longer than the lag buy``()=
        //given
        let buyOn = {Start = seedDate 5;Value = oc 14m 5m};
        let ohcls = [{Start = seedDate 1;Value = oc 3m 1m};
                    {Start = seedDate 2;Value = oc 4m 1m};
                    {Start = seedDate 3;Value = oc 8m 1m};
                    {Start = seedDate 4;Value = oc 9m 5m};
                    buyOn
                    ]
        //when
        let result = enter {Lookback = 3; Lag = 2} ohcls
        //then
        result |> should equal (Long(buyOn,0))

    [<Test>]
    member x.``when i set a lookback of 3 days with a lag of 4 days then if the price has been above the avg for longer than the lag buy``()=
        //given
        let buyOn = {Start = seedDate 7;Value = oc 11m 5m};
        let ohcls = [{Start = seedDate 1;Value = oc 1m 1m};
                    {Start = seedDate 2;Value = oc 3m 1m};
                    {Start = seedDate 3;Value = oc 5m 1m};
                    {Start = seedDate 4;Value = oc 6m 5m};
                    {Start = seedDate 5;Value = oc 7m 5m};
                    {Start = seedDate 6;Value = oc 10m 5m};
                    buyOn
                    ]
        //when
        let result = enter {Lookback = 3; Lag = 4} ohcls
        //then
        result |> should equal (Long(buyOn,0))


[<TestFixture>]
type ``Given I want to exit a trade via a moving average strategy``() = 

    let exit (parameter : MovingAvgStrategyConfig) points order =
        MovingAverageExitStrategy(parameter).Exit points order
                

    [<Test>]
    member x.``when i enter in a unfilled order i am returned an unfilled order``() =
        //given
        let ohcls = [{Start = seedDate 1;Value = oc 3m 1m};
                    {Start = seedDate 2;Value = oc 4m 1m};
                    {Start = seedDate 3;Value = oc 8m 1m};
                    {Start = seedDate 4;Value = oc 9m 5m};
                    ]
        //when
        let result = exit {Lookback = 2; Lag = 1} ohcls UnFilledOrder
        //then
        result |> should equal UnFilledOrder

    [<Test>]
    member x.``when i enter in a dead order i am returned an dead order``() =
        //given
        let order = DeadOrder( Long({Start = DateTime(); Value = oc 9m 8m},0),{Start = DateTime(); Value = oc 9m 8m})
        let ohcls = [{Start = seedDate 1;Value = oc 3m 1m};
                    {Start = seedDate 2;Value = oc 4m 1m};
                    {Start = seedDate 3;Value = oc 8m 1m};
                    {Start = seedDate 4;Value = oc 9m 5m};
                    ]
        //when
        let result = exit {Lookback = 2; Lag = 1} ohcls order
        //then
        result |> should equal order


    [<Test>]
    member x.``when i enter in a order and there is no data i am returned the same order``() =
        //given
        let ohcls = []
        let order = Long({Start = DateTime(); Value = oc 9m 8m},0)
        //when
        let result = exit {Lookback = 1; Lag = 1} ohcls order
        //then
        result |> should equal order



    [<Test>]
    member x.``when i enter in a order when the lookback is >= to the amount of data then i am returned the order``() =
        //given
        let ohcls = [{Start = seedDate 1;Value = oc 17m 1m};
                    {Start = seedDate 2;Value = oc 8m 1m};
                    {Start = seedDate 3;Value = oc 6m 1m};
                    {Start = seedDate 4;Value = oc 2m 1m};]

        let order = Long({Start = DateTime(); Value = oc 9m 8m},0)
        //when
        let result = exit {Lookback = 4; Lag = 1} ohcls order
        //then
        result |> should equal order

    [<Test>]
    member x.``when a lookback of 3 is given and a series crosses below the moving avg on the third open then order is sold on 4th open``() =
        //given
        let soldOn = {Start = seedDate 4;Value = oc 10m 1m};
        let ohcls = [{Start = seedDate 1;Value = oc 17m 1m};
                    {Start = seedDate 2;Value = oc 15m 1m};
                    {Start = seedDate 3;Value = oc 6m 1m};
                    soldOn]

        let order = Long({Start = seedDate 0; Value = oc 9m 8m},0)
        //when
        let result = exit {Lookback = 3; Lag = 1} ohcls order
        //then
        result |> should equal (DeadOrder(order,soldOn))



    [<Test>]
    member x.``when a lookback of 3 and a lag of 3 is given and a series crosses below the moving avg on the third open then order is sold on 6th open``() =
        //given
        let soldOn = {Start = seedDate 6;Value = oc 3m 1m};
        let ohcls = [{Start = seedDate 1;Value = oc 17m 1m};
                    {Start = seedDate 2;Value = oc 15m 1m};
                    {Start = seedDate 3;Value = oc 6m 1m};
                    {Start = seedDate 4;Value = oc 5m 1m};
                    {Start = seedDate 5;Value = oc 4m 1m};
                    soldOn]

        let order = Long({Start = seedDate 0; Value = oc 9m 8m},0)
        //when
        let result = exit {Lookback = 3; Lag = 3} ohcls order
        //then
        result |> should equal (DeadOrder(order,soldOn))