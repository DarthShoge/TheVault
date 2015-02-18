module Capital.Agents
open Capital.Engine
open Capital.Extensions
open System
open Capital.DataStructures
open Deedle

type Agent<'T> = MailboxProcessor<'T>
type AgentAction = 
        |Incr of int
        |Get of AsyncReplyChannel<int list>


type StockDataAction = 
    | Request of (string * DateTime * DateTime)
    | Download of AsyncReplyChannel<(string*Tick array) list>

let a = Agent<AgentAction>.Start(fun inbox ->
    let results = []
    let rec loop arr = async{
        let! msg = inbox.Receive()
        match msg with 
        |Incr(x) ->
            Async.Sleep(7000) |> ignore
            return! loop (x::arr)
        |Get(r) -> 
            r.Reply(arr)
            return! loop arr

        }
    loop results
    )

let createStockAgent() = Agent<StockDataAction>.Start(fun inbox ->
    let results = []
    let rec loop arr = async{
        let! msg = inbox.Receive()
        match msg with 
        |Request(x) ->
            let stock,strt,``end`` = x
            let result = getStockData stock strt ``end``
            let next = (stock,result)::arr
            return! loop next
        |Download(r) -> 
            r.Reply(arr)
            return! loop arr

        }
    loop results
    )


let getStockDataParallel (symbols : string seq) startDate endDate batchSize =
    let splitStocks = symbols |> Seq.toList |> List.takeSkip batchSize
    let rec callAgents batch agents =
        match batch with
        |h::t ->  
            let agent = createStockAgent()
            h |> List.iter(fun x ->  agent.Post(Request(x,startDate,endDate)))
            let updated = (agent::agents)
            callAgents t updated
        |[]  -> agents

    let allAgents = (callAgents splitStocks [])
    let allData = allAgents |> Seq.map(fun agnt ->  agnt.PostAndReply(fun replyChannel -> Download replyChannel)) |> Seq.collect(fun x -> x)
    allData 

let loadStocksParallel symbols startDate endDate batchSize =
    let completed = getStockDataParallel symbols startDate endDate batchSize
    [for (ticker,data) in completed ->
        ticker => ((data |> toSeries (fun x -> x.AC)) |> Series.sortByKey)
    ]
    |> Frame.ofColumns
    |> Frame.fillMissing(Direction.Backward)

let x =a.PostAndReply(fun replyChannel -> Get replyChannel)