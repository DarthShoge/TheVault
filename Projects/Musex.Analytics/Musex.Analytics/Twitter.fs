namespace Muszex.Analytics
open FSharp.Data
open FSharp.Data.Toolbox

open FSharp.Data.Toolbox.Twitter

module Twitter  = 
    let twitter = 
        let key = "TzaWp7sF8L3PEKwd0Gjkjj7xf"
        let secret = "uA7LogKLkWFWS0FSHm0VTxTLaCUFQjnedqDYa1hCTNBJVlFb88" 
        Twitter.AuthenticateAppOnly(key, secret)

    let getArtistTwitterFollowers (name : string) = 
        let user = twitter.Users.Lookup([name]) 
        user |> Seq.exactlyOne
//        let tweets = twitter.Search.Tweets(name , count=200)
//        tweets.Statuses |> Seq.map(fun x -> x.User)



