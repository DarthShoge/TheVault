module TestHelpers
open System
open DataStructures
open ShogeStrategies
open MoneyManagerModule
open NewStrategyModule


let seedDate n = (new DateTime(2013,12,31)).AddDays (float n)
let ohlc o h l c v = {Open = o; Hi = h; Low = l; Close = c; Volume = v; AdjClose = c} 
let oc o c = {Open = o; Hi = c; Low = o; Close = c; Volume = 1000m; AdjClose = c}
