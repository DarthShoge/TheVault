module CapitalTests
open NUnit.Framework
open FsUnit
open System
open Deedle
open MathNet.Numerics.Statistics
open Capital.Engine
open Capital.EventProfiler

[<TestFixture>]
type ``event profiler should``() =

    let dt(y,m,d) = DateTime(y,m,d)

    let n = MathNet.Numerics.Distributions.Normal(0.,1.)
    let dateOffset length = [1..length] 
                            |> Seq.map(fun x -> dt(2011,02,01 + x)) 
                            |> Seq.toArray

    [<Test>]
    member given.``there are no stock values available return 0 matrix``() =
        let dates = dateOffset 5
        let mkt = series [for d in dates do yield d => n.Sample() ]
        let s = series []
        let stocks = frame ["LUX" => s; "SPX" => mkt]
        let events = EventProfiler("SPX").FindEvents "LUX" stocks dates
        let expected = series [for d in (dates |> Seq.skip 1) do yield d =>0]
        events |> should equal expected

    [<Test>]
    member given.``If one standard event occurs on second day tthen event should yield one on that day``() =
        let dates = [|dt(2012,01,11);dt(2012,01,12);dt(2012,01,13)|]
        let aapl = series [
                            dt(2012,01,11) => 10.1
                            dt(2012,01,12) => 10.
                            dt(2012,01,13) => 7.
                            ]
        let mkt = series  [
                            dt(2012,01,11) => 1.1
                            dt(2012,01,12) => 1.
                            dt(2012,01,13) => 1.3
                            ]
        let stocks = frame ["AAPL" => aapl; "LSX" => mkt]
        let events = EventProfiler("LSX").FindEvents "AAPL" stocks dates
        events |> should equal (series[ dt(2012,01,12) => 0;  dt(2012,01,13) => 1])


    [<Test>]
    member given.``a user defined event occurs event should yield one on event date``() =
     let dates = [|dt(2012,01,10);dt(2012,01,11);dt(2012,01,12);dt(2012,01,13)|]
     let aapl = series [
                         dt(2012,01,10) => 1.
                         dt(2012,01,11) => 1.
                         dt(2012,01,12) => 1.
                         dt(2012,01,13) => 2.
                         ]
     let mkt = series  [
                         dt(2012,01,10) => 1.
                         dt(2012,01,11) => 1.
                         dt(2012,01,12) => 1.
                         dt(2012,01,13) => 2.
                         ]
     let stocks = frame ["AAPL" => aapl; "LSX" => mkt]
     let events = EventProfiler("LSX",fun sym mkt -> mkt = 1. && sym = 1. ).FindEvents "AAPL" stocks dates
     events |> should equal (series[dt(2012,01,11) => 0;  dt(2012,01,12) => 0;  dt(2012,01,13) => 1])

    [<Test>]
    member given.``profiled event does not have enough data to for lookback backend the empty profile vector is returned``() =
        let aapl = series [dt(2012,01,10) => 0.11; dt(2012,01,11) => 0.13
                           dt(2012,01,12) => 0.12; dt(2012,01,13) => 0.15 ]

        let events = series[dt(2012,01,11) => 1;  dt(2012,01,12) => 0;  dt(2012,01,13) => 0]
        let profiler = EventProfiler("LSX",fun sym mkt -> mkt = 1. && sym = 1. )
        let result = profiler.ProfileEvents aapl events 2
        result |> should equal []

    [<Test>]
    member given.``profiled event does not have enough data to for lookback frontend the empty profile vector is returned``() =
        let aapl = series [dt(2012,01,10) => 0.11; dt(2012,01,11) => 0.13
                           dt(2012,01,12) => 0.12; dt(2012,01,13) => 0.15 ]

        let events = series[dt(2012,01,11) => 0;  dt(2012,01,12) => 0;  dt(2012,01,13) => 1]
        let profiler = EventProfiler("LSX",fun sym mkt -> mkt = 1. && sym = 1. )
        let result = profiler.ProfileEvents aapl events 2
        result |> should equal []


    [<Test>]
    member given.``profiled event has enough data for the lookback bckend then a profile vector with correct data is returned``() =
        let aapl = series [dt(2012,01,10) => 0.11; dt(2012,01,11) => 0.13
                           dt(2012,01,12) => 0.12; dt(2012,01,13) => 0.15 ]

        let events = series[dt(2012,01,11) => 0;  dt(2012,01,12) => 1;  dt(2012,01,13) => 0]
        let profiler = EventProfiler("LSX",fun sym mkt -> mkt = 1. && sym = 1. )
        let result = profiler.ProfileEvents aapl events 1
        result.[0] |> should equal (series [-1 => 0.13; 0 => 0.12; 1 => 0.15 ])

    [<Test>]
    member given.``profiled event has a look back of 2  then a profile vector with correct data is returned``() =
        let aapl = series [dt(2012,01,09) => 0.22; dt(2012,01,10) => 0.11; dt(2012,01,11) => 0.13
                           dt(2012,01,12) => 0.12; dt(2012,01,13) => 0.15 ]

        let events = series[dt(2012,01,11) => 1;  dt(2012,01,12) => 0;  dt(2012,01,13) => 0]
        let profiler = EventProfiler("LSX",fun sym mkt -> mkt = 1. && sym = 1. )
        let result = profiler.ProfileEvents aapl events 2
        result.[0] |> should equal (series [-2 => 0.22;-1 => 0.11; 0 => 0.13; 1 => 0.12; 2 => 0.15  ])


    [<Test>]
    member given.``there are multiple profiled events then a collection of profile vectors with correct data is returned``() =
        let aapl = series [dt(2012,01,10) => 0.11; dt(2012,01,11) => 0.13
                           dt(2012,01,12) => 0.12; dt(2012,01,13) => 0.15 ]

        let events = series[dt(2012,01,11) => 1;  dt(2012,01,12) => 1;  dt(2012,01,13) => 0]
        let profiler = EventProfiler("LSX",fun sym mkt -> mkt = 1. && sym = 1. )
        let result = profiler.ProfileEvents aapl events 1
        result.Length |> should equal 2
        result.[1] |> should equal (series [-1 => 0.11; 0 => 0.13; 1 => 0.12 ])
        result.[0] |> should equal (series [-1 => 0.13; 0 => 0.12; 1 => 0.15 ])


    [<Test>]
    member integration.``profiled events should contain the correct the correct expected values``() =
        let stox = frame [ "LUX" => series [dt(2012,01,10) => 3.1; dt(2012,01,11) => 2.8;dt(2012,01,12) => 1.8; dt(2012,01,13) => 1.9; dt(2012,01,14) => 1.89 ]
                           "LSX" => series [dt(2012,01,10) => 2.03; dt(2012,01,11) => 2.12;dt(2012,01,12) => 3.6; dt(2012,01,13) => 3.58; dt(2012,01,14) => 3.62 ]
                            ]
        let dates = [|dt(2012,01,10); dt(2012,01,11);dt(2012,01,12); dt(2012,01,13); dt(2012,01,14) |]
        let profiler = EventProfiler("LSX",fun sym mkt -> sym < -0.35 && mkt > 0.6 )
        let result = profiler.ProfileAll stox dates 1
        let ser = result.Columns.[0]
        let actual = result |> Frame.getCol(0) |> Series.map(fun k (v:double) -> Math.Round(v,3))
        actual |> should equal ( series [-1 => -0.097; 0 => -0.357; 1 => 0.056])



    [<Test>]
    member given.``there are any profiled eventswith a zero returned they are filtered out``() =
        let stox = frame [ "LUX" => series [dt(2012,01,09) => 3.1;dt(2012,01,10) => 2.8; dt(2012,01,11) => 2.8;dt(2012,01,12) => 1.8; dt(2012,01,13) => 1.9; dt(2012,01,14) => 1.89 ]
                           "LSX" => series [dt(2012,01,09) => 2.03; dt(2012,01,10) => 2.12; dt(2012,01,11) => 2.12;dt(2012,01,12) => 3.6; dt(2012,01,13) => 3.58; dt(2012,01,14) => 3.62 ]
                            ]
        let dates = [|dt(2012,01,10); dt(2012,01,11);dt(2012,01,12); dt(2012,01,13); dt(2012,01,14) |]
        let profiler = EventProfiler("LSX",fun sym mkt -> sym < -0.35 && mkt > 0.6 )
        let result = profiler.ProfileAll stox dates 1
        let ser = result.Columns.[0]
        let actual = result |> Frame.getCol(0) |> Series.map(fun k (v:double) -> Math.Round(v,3))

        actual |> should equal ( series [-1 => -0.097; 0 => -0.357; 1 => 0.056])