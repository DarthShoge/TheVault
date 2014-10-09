namespace Muszex.Analytics.Test
open NUnit.Framework
open FsUnit
open Musex.Analytics.DataStructures

type ProviderFactory() =  
    let (|Youtube|Unmapped|) (link : string) =
        if link.Contains("youtube") then
            Youtube
        else
            Unmapped

    member x.GetProvider link : unit -> ProviderResponse =
        match link with
        | Youtube ->    let getYoutubes() =
                            let attrs = [0..5] |> Seq.map(fun x -> {Key="foo";Value=0.0})
                            {Link=link;Provider="Youtube";Attributes=attrs}
                        getYoutubes
        | Unmapped -> fun _ -> {Link=link;Provider="Unmapped";Attributes= []}

[<TestFixture>]
type ``given I want to try extract data``() = 
    
    [<Test>]
    member this.``when I pass in a link with no available provider i am returned``() =
        //given
        let factory = ProviderFactory()
        let link = "www.foo.com"
        //when
        let providerResponse = factory.GetProvider(link)()
        //then
        Assert.AreEqual(providerResponse.Provider,"Unmapped")
//        providerResponse.Provider |> should equal "Unmapped"
         
