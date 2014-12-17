module CapitalTests


open NUnit.Framework
open FsUnit
open System
open Deedle
open MathNet.Numerics.Statistics
open main

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