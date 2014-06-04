module ShogeStrategies.Lab
open FSharp.Data
open Microsoft.FSharp.Collections
open MongoDB.FSharp
open MongoDB.Bson
open MongoDB.Driver
open MongoDB.Driver.Builders
open MongoDB.FSharp.Serializers
open MongoDB.FSharp.SerializationOptions
Serializers.Register()

let chems = 
        let data = FreebaseData.GetDataContext()
        data.``Products and Services``.Business.``Stock exchanges``.Individuals.NYSE.``Companies traded``
        |> Seq.filter(fun x-> x.``End date`` = null)
        |> Seq.map(fun x -> x.``Ticker symbol``) 
        |> Seq.toArray


type MyTestType = {
        Id: BsonObjectId
        Name : string;
        PhoneNumber: int;
        Balance: decimal;
        Transactions: decimal list
    }

type tester() =
    member x.saveRow d =
        Serializers.Register()
        let connectionString = "mongodb://localhost"
        let client = MongoClient(connectionString)
        let server = client.GetServer();
        let db = server.GetDatabase("test")
        let collection = db.GetCollection<MyTestType> "fooLongs"
        let id = BsonObjectId(ObjectId.GenerateNewId())
        collection.Insert {Id = id; Name = "Trishawna";PhoneNumber = 079512 ;Balance = 500000m;Transactions = [56m;34m; 90m]} 
        let results = collection.Find(Query.EQ("Name", BsonString("Trishawna")))
        ()
    
//type Class1() = 
//    member this.X = "F#"
//    let chems =  