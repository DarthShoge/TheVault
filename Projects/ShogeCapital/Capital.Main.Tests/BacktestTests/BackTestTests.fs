module Capital.Main.Tests.BackTestTests

open NUnit.Framework
open ShogeLabs.Strategies.PatternRecognition
open System
open FsUnit
open Capital.DataStructures
open ShogeLabs.Strategies.Patterns
open FSharp.Charting
open Deedle
open Capital.Main.Tests.TestHelpers
open FsUnitExtensions
open Capital.Main.MarketSimulation


[<TestFixture>]
type BacktesterTests() =

    let backtester =  BackTester()
    
    [<Test>]
    member given.``I call getOwenedOrders a empty single order vector i am returned a owned order verctor with 0s``() =
        //given
        let daysWorthOfData = 50
        let orders= series [ for v in  [0..daysWorthOfData] -> dt (float v) => NoOrder ]
        //when
        let results = backtester.GetOwnedOrders orders
        //then
        let expectedResults = series [ for v in  [0..daysWorthOfData] -> dt (float v) => 0.00 ]
        results |> should (equalWithToleranceOf 0.000001) expectedResults


    [<Test>]
    member given.``a order was executed on the 1st day with no exit order is held ``() =
      //given
        let orders= series [(dt 0. )=> Buy(100.);(dt 1. )=> NoOrder;(dt 2. )=> NoOrder
                            (dt 3. )=> NoOrder;(dt 4. )=> NoOrder
                             ]
        //when
        let results = backtester.GetOwnedOrders orders
        //then
        let expectedResults = series [(dt 0. )=> 100.;(dt 1. )=> 100.;(dt 2. )=> 100.;
                                        (dt 3. )=> 100.;(dt 4. )=> 100.]
        results |> should (equalWithToleranceOf 0.000001) expectedResults

    [<Test>]
    member given.``several orders are executed on the 1st day correct amounts of orders held ``() =
      //given
        let orders= series [(dt 0. )=> AggregateOrder(AggregateOrder(Buy(50.),Buy(100.)), Sell(100.));(dt 1. )=> NoOrder;
                            (dt 2. )=> NoOrder;(dt 3. )=> NoOrder;(dt 4. )=> NoOrder
                             ]
        //when
        let results = backtester.GetOwnedOrders orders
        //then
        let expectedResults = series [(dt 0. )=> 50.;(dt 1. )=> 50.;(dt 2. )=> 50.;
                                        (dt 3. )=> 50.;(dt 4. )=> 50.]
        results |> should (equalWithToleranceOf 0.000001) expectedResults

    [<Test>]
    member given.``several orders are executed correct amounts of orders held ``() =
      //given
        let orders= series [(dt 0. )=> AggregateOrder(AggregateOrder(Buy(50.),Buy(100.)), Sell(100.));(dt 1. )=> NoOrder;
                            (dt 2. )=> NoOrder;(dt 3. )=> Sell(50.);(dt 4. )=> NoOrder
                             ]
        //when
        let results = backtester.GetOwnedOrders orders
        //then
        let expectedResults = series [(dt 0. )=> 50.;(dt 1. )=> 50.;(dt 2. )=> 50.;
                                        (dt 3. )=> 0.;(dt 4. )=> 0.]
        results |> should (equalWithToleranceOf 0.000001) expectedResults
    [<Test>]
    member given.``I am getting cash vector and passed single empty orders vector and empty historic vector cashflow remains the same``() =
        //given
        let orders = series [for v in 1..50 -> (dt (float v)) => NoOrder]
        let data = seq [for v in 1..50 -> tck 0. (n.Sample()) (dt (float v)) ]
        let startCash = 10000.
        //when
        let results = backtester.GetBacktestCash(orders, data, startCash)
        //then
        let expectedResults = series [for v in 1..50 -> (dt (float v)) => startCash]
        results |> should (equalWithToleranceOf 0.000001) expectedResults


    [<Test>]
    member given.```I am getting cash vector and passed buy order, ordersieze * corresponding price are subtracted from cash flow vector``() =
        //given
        let orders = series [(dt 0. )=> NoOrder;(dt 1. )=> Buy(100.);
                            (dt 2. )=> NoOrder;(dt 3. )=> NoOrder;(dt 4. )=> NoOrder
                             ]
        let data = seq [tck 0. 10. (dt 0. );tck 0. 10. (dt 1. );
                            tck 0. 10. (dt 2. );tck 0. 10. (dt 3. );tck 0. 10. (dt 4. )
                             ]
        let startCash = 10000.
        //when
        let results = backtester.GetBacktestCash( orders ,data ,startCash)
        //then
        let expectedResults = series [(dt 0. )=> 10000.;(dt 1. )=> 9000.;
                            (dt 2. )=> 9000.;(dt 3. )=> 9000.;(dt 4. )=>  9000.
                             ]
        results |> should (equalWithToleranceOf 0.000001) expectedResults
        

    [<Test>]
    member given.``I am getting cash vector and passed single empty orders vector and empty historic data frame cashflow remains the same``() =
        //given
        let orders = frame [ 
                                "AAA" => series [for v in 1..50 -> (dt (float v)) => NoOrder];
                                "BBB" => series [for v in 1..50 -> (dt (float v)) => NoOrder];
                                "CCC" => series [for v in 1..50 -> (dt (float v)) => NoOrder];
                           ]
        let data = BackTester.ticksToDataFrame 
                            (seq [
                                    "AAA" => seq [for v in 1..50 -> tck 0. (n.Sample()) (dt (float v)) ];
                                    "BBB" => seq [for v in 1..50 -> tck 0. (n.Sample()) (dt (float v)) ];
                                    "CCC" => seq [for v in 1..50 -> tck 0. (n.Sample()) (dt (float v)) ];
                                            ])
        let startCash = 10000.
        //when
        let results = backtester.GetBacktestCash(orders ,data ,startCash)
        //then
        let expectedResults = series [for v in 1..50 -> (dt (float v)) => startCash]
        results |> should (equalWithToleranceOf 0.000001) expectedResults



    [<Test>]
    member given.```I am getting cashflow vector and passed single order frame, ordersieze * price for single column frame price are subtracted from cash flow vector``() =
        //given
        let orders =frame ["AAA" => (series [(dt 0. )=> NoOrder;(dt 1. )=> Buy(100.);(dt 2. )=> NoOrder;(dt 3. )=> NoOrder;(dt 4. )=> NoOrder])]
        let data = BackTester.ticksToDataFrame 
                    (seq [
                            "AAA" =>  seq [tck 0. 10. (dt 0. );tck 0. 10. (dt 1. );tck 0. 10. (dt 2. );tck 0. 10. (dt 3. );tck 0. 10. (dt 4. )];
                            "BBB" =>  seq [tck 0. 50. (dt 0. );tck 0. 50. (dt 1. );tck 0. 50. (dt 2. );tck 0. 50. (dt 3. );tck 0. 50. (dt 4. )];
                            "CCC" =>  seq [tck 0. 50. (dt 0. );tck 0. 50. (dt 1. );tck 0. 50. (dt 2. );tck 0. 50. (dt 3. );tck 0. 50. (dt 4. )];
                             ])
        let startCash = 10000.
        //when
        let results = backtester.GetBacktestCash( orders ,data ,startCash)
        //then
        let expectedResults = series [(dt 0. )=> 10000.;(dt 1. )=> 9000.;
                            (dt 2. )=> 9000.;(dt 3. )=> 9000.;(dt 4. )=>  9000.
                             ]
        results |> should (equalWithToleranceOf 0.000001) expectedResults


    [<Test>]
    member given.```I am getting cashflow vector and passed several order frames, ordersieze * price for single column frame price are subtracted from cash flow vector``() =
        //given
        let orders =frame [
                            "AAA" => (series [(dt 0. )=> NoOrder;(dt 1. )=> Buy(100.);(dt 2. )=> NoOrder;(dt 3. )=> NoOrder;(dt 4. )=> NoOrder])
                            "BBB" => (series [(dt 0. )=> NoOrder;(dt 1. )=> NoOrder;(dt 2. )=> Buy(100.);(dt 3. )=> NoOrder;(dt 4. )=> NoOrder])
                            ]
        let data = BackTester.ticksToDataFrame 
                    (seq [
                            "AAA" =>  seq [tck 0. 10. (dt 0. );tck 0. 10. (dt 1. );tck 0. 10. (dt 2. );tck 0. 10. (dt 3. );tck 0. 10. (dt 4. )];
                            "BBB" =>  seq [tck 0. 10. (dt 0. );tck 0. 10. (dt 1. );tck 0. 10. (dt 2. );tck 0. 10. (dt 3. );tck 0. 10. (dt 4. )];
                            "CCC" =>  seq [tck 0. 50. (dt 0. );tck 0. 50. (dt 1. );tck 0. 50. (dt 2. );tck 0. 50. (dt 3. );tck 0. 50. (dt 4. )];
                             ])
        let startCash = 10000.
        //when
        let results = backtester.GetBacktestCash( orders ,data ,startCash)
        //then
        let expectedResults = series [(dt 0. )=> 10000.;(dt 1. )=> 9000.;
                            (dt 2. )=> 8000.;(dt 3. )=> 8000.;(dt 4. )=>  8000.
                             ]
        results |> should (equalWithToleranceOf 0.000001) expectedResults


    [<Test>]
    member given.```I am getting the cash of my owned stocks and passed empty ownership frame, ordersieze * price for single column frame price are subtracted from cash flow vector``() =
        //given
        let owned =frame []
        let data = frame [
                            "AAA" =>  series [(dt 0. ) => tck 0. 10.; (dt 1. ) => tck 0. 10.;(dt 2. ) =>tck 0. 10.;(dt 3. ) =>tck 0. 10. ;(dt 4. ) =>tck 0. 10. ];
                            "BBB" =>  series [(dt 0. ) => tck 0. 10.; (dt 1. ) => tck 0. 10.;(dt 2. ) =>tck 0. 10.;(dt 3. ) =>tck 0. 10. ;(dt 4. ) =>tck 0. 10. ];
                            "CCC" =>  series [(dt 0. ) => tck 0. 50.; (dt 1. ) => tck 0. 50.;(dt 2. ) =>tck 0. 50.;(dt 3. ) =>tck 0. 50. ;(dt 4. ) =>tck 0. 50. ];
                             ]
        let startCash = 10000.
        //when
        let results = backtester.GetOwnedCash( owned ,data )
        //then
        let expectedResults = series []
        results |> should equal expectedResults
