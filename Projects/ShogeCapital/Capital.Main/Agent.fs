module Agents
open main
open Capital.Extensions
open System

type Agent<'T> = MailboxProcessor<'T>
type AgentAction = 
        |Incr of int
        |Get of AsyncReplyChannel<int list>


type StockDataAction = 
    | Request of (string * DateTime * DateTime)
    | Download of AsyncReplyChannel<(string*Tick array) list>

let agent = Agent<AgentAction>.Start(fun inbox ->
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
            return! loop ((stock,result)::arr)
        |Download(r) -> 
            r.Reply(arr)
            return! loop arr

        }
    loop results
    )


let loadStocksWithAgents (symbols : string seq) startDate endDate =
    let splitStocks = symbols |> Seq.toList |>List.takeSkip 3
    let rec callAgents batch agents =
        match batch with
        |[]  -> agents
        |h::t ->  
            let agent = createStockAgent()
            for v in h do agent.Post(Request(v,startDate,endDate))
            callAgents t (agent::agents)
    let allAgents = (callAgents splitStocks []) |> Seq.toArray
    let chiefAgent = allAgents.[0]
    let allData = chiefAgent.PostAndReply(fun replyChannel -> Download replyChannel)
    allData

let x =agent.PostAndReply(fun replyChannel -> Get replyChannel)