﻿namespace Capital.Main.Tests

open NUnit.Framework
open FsUnit
open System
open Deedle
open main

[<TestFixture>]
type Given_i_am_operating_over_frames() = 
    


    [<Test>]
    member given.``I backfill a frame then no NA or missing items should remain``() = 
        let values = [23.;22.;Double.NaN;24.;22.] 
        let backfilled = backFill values
        backfilled |> should equal  [23.;22.;22.;24.;22.]
    

    [<Test>]
    member given.``I backfill list a fully populated list i am returned a identical list``() = 
        let values = [23.;22.] 
        let backfilled = backFill values
        backfilled |> should equal  [23.;22.]
