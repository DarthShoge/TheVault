module SeqExtensionTests

open NUnit.Framework
open FsUnit
open System
open ShogeStrategies
open MoneyManagerModule
open Utilities

[<TestFixture>]
type ``given I have sequence and I want to get a sliding window then``() =
    
    [<Test>]
    member x.``then when i only have a single object to slide across only that object is returned``() =
        //given
        let sut = [1]
        //when
        let windows = sut |>  List.slide 5
        //then
        windows.[0] |> should equal [1]

    [<Test>]
    member x.``then when i have a window of one and multiple values then all windows should be one``() =
        //given
        let sut = [1;4;7;3;2;8;9;0;0;0]
        //when
        let windows = sut |>  List.slide 1
        //then
        windows.[0] |> should equal [1]
        windows.[1] |> should equal [4]
        windows.[2] |> should equal [7]
        windows.[3] |> should equal [3]
        windows.[4] |> should equal [2]
        windows.[5] |> should equal [8]
        windows.[6] |> should equal [9]

    [<Test>]
    member x.``then when i have a window of 2 it should only take 1 in the first and two in the second``() =
        //given
        let sut = [1;4;7]
        //when
        let windows = sut |>  List.slide 2
        //then
        windows.[0] |> should equal [1]
        windows.[1] |> should equal [1;4]
        windows.[2] |> should equal [4;7]

    [<Test>]
    member x.``it should take correct amount if window and list are equal size``() =
        //given
        let sut = [1;4;7]
        //when
        let windows = sut |>  List.slide 3
        //then
        windows.[0] |> should equal [1]
        windows.[1] |> should equal [1;4]
        windows.[2] |> should equal [1;4;7]

    [<Test>]
    member x.``it should slide correctly for 3 ``() =
        //given
        let sut = [1;4;7;5;8;10]
        //when
        let windows = sut |>  List.slide 3
        //then
        windows.[0] |> should equal     [1]
        windows.[1] |> should equal   [1;4]
        windows.[2] |> should equal [1;4;7]
        windows.[3] |> should equal [4;7;5]
        windows.[4] |> should equal [7;5;8]
        windows.[5] |> should equal [5;8;10]


    [<Test>]
    member x.``it should slide for empty collection``() =
        //given
        let sut = []
        //when
        let windows = sut |>  List.slide 3
        //then
        windows |> should be Empty 
        