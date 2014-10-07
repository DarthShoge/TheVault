namespace Musex.Analytics
open FSharp.Data
open Musex.Analytics.DataStructures

module ArtistProvider = 
    let provider = FSharp.Data.FreebaseData.GetDataContext()
    type similarArtistProvider = JsonProvider<"templates/similar.json">

    let getSimilarArtists (name: string) = 
        let validName = name.Replace(" ","+").Trim()
        let query = sprintf "http://developer.echonest.com/api/v4/artist/similar?api_key=%s&name=%s&format=json&bucket=id:7digital-US&limit=true" 
                                Configuration.echoNest.Key 
                                name

        let loader = similarArtistProvider.Load(query)
        loader.Response.Artists |> Seq.map(fun x -> {Name= x.Name})

    let allArtists =  
        provider.``Arts and Entertainment``
                .Music
                .``Featured artists``
                |> Seq.map(fun x -> x)

    let getArtistNames = 
        allArtists |> Seq.map(fun x -> x.Name)

    let getArtistByName name = 
        allArtists |> Seq.tryFind(fun x -> x.Name = name || x.``Also known as`` |> Seq.exists(fun y -> y = name) )

    let getArtistsAlbums name =
        let result = allArtists |> Seq.tryFind(fun x -> x.Name = name)
        result