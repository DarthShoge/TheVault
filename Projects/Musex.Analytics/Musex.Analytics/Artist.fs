namespace Musex.Analytics
open FSharp.Data
open Musex.Analytics.DataStructures

module Soundcloud =
    type provider = JsonProvider<"templates/SCUsers.json">
    type profileProvider = JsonProvider<"templates/web-profiles.json">
    let getArtisDetails name =
        let query = sprintf "http://api.soundcloud.com/users.json?q=%s&client_id=%s" name Configuration.soundCloud.Key
        let data = provider.Load(query)
        match data |> Seq.toList with
        |[] -> None
        |x ->   let value = x |> Seq.head
                Some({Id= value.Id;Name=value.FirstName.Value;FollowerCount= value.FollowersCount;FollowingCount = value.FollowingsCount})

    let getArtistProfiles name = 
        let artist = getArtisDetails name
        let query = sprintf "http://api.soundcloud.com/users/%i/web-profiles.json?client_id=%s" artist.Value.Id Configuration.soundCloud.Key
        let data = profileProvider.Load(query)
        data |> Seq.map(fun x -> (
        ("UserName",x.Username.String),
        ("Service",x.Service),
        ("Url",x.Url)
        ))

module ArtistProvider = 
    let provider = FSharp.Data.FreebaseData.GetDataContext()
    type similarArtistProvider = JsonProvider<"templates/similar.json">

    let getSimilarArtists (name: string) : Artist seq = 
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