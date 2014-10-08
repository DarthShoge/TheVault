// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open Musex.Analytics
open FSharp.Data

[<EntryPoint>]
let main argv = 
    printfn "%A" argv
//    let art = ArtistProvider.getArtistByName "Kanye West"
//    let links = art.Value.``Web Link(s)`` |> Seq.map(fun x -> (x.URL,x.Title)) |> Seq.toArray
    let twits = Twitter.getArtistTwitterFollowers "Asa_official"
    let allRecs = ArtistProvider.getSimilarArtists "Key Wane"
    let hbk = Soundcloud.getArtistProfiles "IAMNOBODI"
    0 // return an integer exit code
