module NewStrategyTests
//open NUnit.Framework
open FsUnit
open System
open DataStructures
open ShogeStrategies
open MoneyManagerModule
open NewStrategyModule
open TestHelpers
//
//
//[<TestFixture>]
//type ``trend following entry strategy should work correctly``() =
//
//    let strategy = TrendFollowerEntry()
//
//    let buy  = strategy.Enter
//
//    let buyToOrder d (startBal:decimal) a = 
//        let riskMan = SimpleRiskManager()
//        strategy.EnterToOrder d startBal (riskMan.Offer) a
//
//    member I.AssertResultWasEqual result expected =
//        match result with 
//           |LiveSignal(sut) -> sut = expected |> should be True
//
//    
//    [<Test>]
//    member I.``should return empty signal list if the point collection is empty ``() =
//        //arrange
//        let points = []
//        //act
//        let result = buy (up 1) points 
//        //assert
//        result |> should be Empty
//
//
//    [<Test>]
//    member I.``if set to buy after 1 day up should return Some live signal next open after on one consective day up``() =
//        //arrange
//        let signalRaisedAt ={Start = seedDate 1;Value = oc 5m 10m}
//        let pointOfPurchase ={Start = seedDate 2;Value = oc 5m 5m}
//        let points = [signalRaisedAt; pointOfPurchase]
//        //act
//        let result::_ = buy (up 1) points
//        //assert
//        I.AssertResultWasEqual result pointOfPurchase
//
//    [<Test>]
//    member I.``should not buy after a down day if set to buy on one day up``() = 
//        //arrange 
//        let pointOfNonPurchase ={Start = seedDate 1;Value = oc 10m 5m}
//        let points = [pointOfNonPurchase;{Start = seedDate 1;Value = oc  5m 10m}]
//        //act
//        let result = buy (up 1) points
//        //assert
//        result = [] |> should be True
//
//    [<Test>]
//    member I.``should not buy if in a staganant market``() = 
//        //arrange 
//        let pointOfNonPurchase = {Start = seedDate 1;Value = oc 5m 5m}
//        let points = [0..10] |> List.map(fun x -> {Start = seedDate x;Value = oc 5m 5m})
//        //act
//        let result = buy (up 1) points
//        //assert
//        result = [] |> should be True
//
//
//    [<Test>]
//    member I.``should not return none if no signal is generated``() = 
//        //arrange 
//        let pointOfNonPurchase = {Start = seedDate 1;Value = oc 5m 5m}
//        let points = [0..10] |> List.map(fun x -> {Start = seedDate x;Value = oc (50m - decimal x) (50m - 1m - decimal x)})
//        //act
//        let result = buy (up 1) points
//        //assert
//        result = [] |> should be True
//
//    
//    [<Test>]
//    member I.``should buy on third day after down day then up day if set to buy on one day up``() = 
//        //arrange 
//        let pointOfNonPurchase ={Start = seedDate 1;Value = oc 10m 5m}
//        let signalRaisedAt ={Start = seedDate 2;Value = oc 5m 10m}
//        let pointOfPurchase ={Start = seedDate 3;Value = oc 5m 5m}
//        let points = [pointOfNonPurchase;signalRaisedAt;pointOfPurchase]
//        //act
//        let result::_ = buy (up 1) points
//        //assert
//        I.AssertResultWasEqual result pointOfPurchase
//
//
//    [<Test>]
//    member I.``should buy after a signal raised date irrespective of order of point collection``() =
//        //arrange 
//        let pointOfNonPurchase ={Start = seedDate 1;Value = oc 10m 5m}
//        let signalRaisedAt ={Start = seedDate 2;Value = oc 5m 10m}
//        let pointOfPurchase ={Start = seedDate 3;Value = oc 5m 5m}
//        let jumbledPoints = [pointOfPurchase;signalRaisedAt;pointOfNonPurchase;]
//        //act
//        let result::_ = buy (up 1) jumbledPoints
//        //assert
//        I.AssertResultWasEqual result pointOfPurchase
//
//    [<Test>]
//    member I.``if set to buy after 2 days up should return Some live signal after 2 days on the open after signal was raised``() =
//        //arrange
//        let firstDayUp ={Start = seedDate 1;Value = oc 5m 10m}
//        let signalRaisedAt ={Start = seedDate 2;Value = oc 10m 15m}
//        let pointOfPurchase ={Start = seedDate 3;Value = oc 15m 15m}
//        let points = [firstDayUp;signalRaisedAt; pointOfPurchase]
//        //act
//        let result::_ = buy (up 2) points
//        //assert
//        I.AssertResultWasEqual result pointOfPurchase
//
//        
//    [<Test>]
//    member I.``should buy on fifth day after several down days if set to buy on 2 days up``() = 
//        //arrange 
//        let signalRaisedAt ={Start = seedDate 3;Value = oc 10m 15m}
//        let pointOfPurchase ={Start = seedDate 4;Value = oc 15m 5m}
//        let points = [
//            {Start = seedDate 0;Value = oc 10m 5m};
//            {Start = seedDate 1;Value = oc 5m 5m};
//            {Start = seedDate 2;Value = oc 5m 10m};
//            signalRaisedAt;
//            pointOfPurchase
//            ]
//        //act
//        let result::_ = buy (up 2) points
//        //assert
//        I.AssertResultWasEqual result pointOfPurchase
//
//    [<Test>]
//    member I.``if set to buy after 1 day down should return Some live signal next open after on one consective day down``() =
//        //arrange
//        let signalRaisedAt ={Start = seedDate 1;Value = oc 10m 5m}
//        let pointOfPurchase ={Start = seedDate 2;Value = oc 5m 5m}
//        let points = [signalRaisedAt; pointOfPurchase]
//        //act
//        let result::_ = buy (down 1) points
//        //assert
//        I.AssertResultWasEqual result pointOfPurchase
//
//    [<Test>]
//    member I.``If multiple signals are raised consecutively on points then all the signals are returned``()=
//        //arrange 
//        let fstSignalRaisedAt =  {Start = seedDate 2;Value = oc 5m 10m};
//        let sndsignalRaisedAtAndFstPurchase ={Start = seedDate 3;Value = oc 10m 15m}
//        let sndPointOfPurchase ={Start = seedDate 4;Value = oc 15m 5m}
//        let points = [
//            {Start = seedDate 0;Value = oc 10m 5m};
//            {Start = seedDate 1;Value = oc 5m 5m};
//            fstSignalRaisedAt;
//            sndsignalRaisedAtAndFstPurchase;
//            sndPointOfPurchase
//            ]
//        //act
//        let result::nextResult::_ = buy (up 1) points
//        //assert
//        I.AssertResultWasEqual result sndsignalRaisedAtAndFstPurchase
//        I.AssertResultWasEqual nextResult sndPointOfPurchase
//
//
//    [<Test>]
//    member I.``If multiple signals are raised on sparse points then all the signals are returned``()=
//        //arrange 
//        let fstPurchase ={Start = seedDate 1;Value = oc 10m 8m}
//        let sndPointOfPurchase ={Start = seedDate 4;Value = oc 15m 5m}
//        let points = [
//            {Start = seedDate 0;Value = oc 5m 10m};
//            fstPurchase
//            {Start = seedDate 2;Value = oc 8m 5m};
//            {Start = seedDate 3;Value = oc 5m 10m};
//            sndPointOfPurchase
//            ]
//        //act
//        let result::nextResult::_ = buy (up 1) points
//        //assert
//        I.AssertResultWasEqual result fstPurchase
//        I.AssertResultWasEqual nextResult sndPointOfPurchase
//
//    [<Test>]
//    member I.``Integration test : should raise 3 signals when set on 2 up over long series of data``()=
//        //arrange 
//        let fstPurchase = {Start = seedDate 3;Value = oc 15m 12m};
//        let sndPointOfPurchase = {Start = seedDate 10;Value = oc 15m 19m};
//        let trdPointOfPurchase = {Start = seedDate 11;Value = oc 19m 21m};
//        let points = [ {Start = seedDate 1;Value = oc 5m 10m};
//                       {Start = seedDate 2;Value = oc 10m 15m};
//                       fstPurchase;
//                       {Start = seedDate 4;Value = oc 12m 8m};
//                       {Start = seedDate 5;Value = oc 8m 3m};
//                       {Start = seedDate 6;Value = oc 3m 7m};
//                       {Start = seedDate 7;Value = oc 7m 5m};
//                       {Start = seedDate 8;Value = oc 5m 11m};
//                       {Start = seedDate 9;Value = oc 11m 15m};
//                       sndPointOfPurchase;
//                       trdPointOfPurchase;
//                        ]
//        //act
//        let result::nextResult::finalResult::_ = buy (up 2) points
//        //assert
//        I.AssertResultWasEqual result fstPurchase
//        I.AssertResultWasEqual nextResult sndPointOfPurchase
//        I.AssertResultWasEqual finalResult trdPointOfPurchase
//
//    [<Test>]
//    member I.``should be able to generate empty orders when no data is passed in``() =
//        //arrange
//        let data = []
//        //act
//        let result = buyToOrder (up 2) 10000m data
//        //assert
//        result |> should be Empty
//     
//    [<Test>]
//    member I.``if set to order after 1 day up should return Some orders for right price on next open after on one consective day up``() =
//        //arrange
//        let signalRaisedAt ={Start = seedDate 1;Value = oc 591m 610m}
//        let pointOfPurchase ={Start = seedDate 2;Value = oc 610m 578m}
//        let points = [signalRaisedAt; pointOfPurchase]
//        //act
//        let result::_ = buyToOrder (up 1) 100000m points
//        //assert
//        match result with
//        |Long(pointOfPurchase, 1) -> true 
//        |_ -> false
//        |> should be True
//
//    [<Test>]
//    member I.``should not order after a down day if set to buy on one day up``() = 
//        //arrange 
//        let pointOfNonPurchase ={Start = seedDate 1;Value = oc 10m 5m}
//        let points = [pointOfNonPurchase;{Start = seedDate 1;Value = oc  5m 10m}]
//        //act
//        let result = buyToOrder (up 1) 100000m points
//        //assert
//        result = [] |> should be True
//
//    
//    [<Test>]
//    member I.``should not place a order if in a staganant market``() = 
//        //arrange 
//        let pointOfNonPurchase = {Start = seedDate 1;Value = oc 5m 5m}
//        let points = [0..10] |> List.map(fun x -> {Start = seedDate x;Value = oc 5m 5m})
//        //act
//        let result = buyToOrder (up 1) 100000m points
//        //assert
//        result = [] |> should be True
//
//    [<Test>]
//    member I.``if set to order after 1 day down should return Some Short order next open after on one consective day down``() =
//        //arrange
//        let signalRaisedAt ={Start = seedDate 1;Value = oc 431m 380m}
//        let pointOfPurchase ={Start = seedDate 2;Value = oc 380m 420m}
//        let points = [signalRaisedAt; pointOfPurchase]
//        //act
//        let result::_ = buyToOrder (down 1) 100000m points
//        //assert
//        match result with
//        |Short(pointOfPurchase, 2) -> true 
//        |_ -> false
//        |> should be True
//
//    [<Test>]
//    member I.``Integration test : should raise 3 orders when set on 2 up over long series of data``()=
//        //arrange 
//        let fstPurchase = {Start = seedDate 3;Value = oc 15m 12m};
//        let sndPointOfPurchase = {Start = seedDate 10;Value = oc 15m 19m};
//        let trdPointOfPurchase = {Start = seedDate 11;Value = oc 19m 21m};
//        let points = [ {Start = seedDate 1;Value = oc 5m 10m};
//                       {Start = seedDate 2;Value = oc 10m 15m};
//                       fstPurchase;
//                       {Start = seedDate 4;Value = oc 12m 8m};
//                       {Start = seedDate 5;Value = oc 8m 3m};
//                       {Start = seedDate 6;Value = oc 3m 7m};
//                       {Start = seedDate 7;Value = oc 7m 5m};
//                       {Start = seedDate 8;Value = oc 5m 11m};
//                       {Start = seedDate 9;Value = oc 11m 15m};
//                       sndPointOfPurchase;
//                       trdPointOfPurchase;
//                        ]
//        //act
//        let result::nextResult::finalResult::_ = buyToOrder (up 2) 100000m points
//        //assert
//
//        result.GetPoint = fstPurchase |> should be True
//        nextResult.GetPoint = sndPointOfPurchase |> should be True
//        finalResult.GetPoint = trdPointOfPurchase |> should be True
//        match result with Long(xval,quant)-> quant = 66 |> should be True
//        match nextResult with Long(xval,quant)-> quant = 66 |> should be True
//        match finalResult with Long(xval,quant)-> quant = 51 |> should be True
//
//
//
//
//[<TestFixture>]
//type ``Trend following exit strategy should work correctly``() =
//    
//    let AssertStillLive result notSoldOn =
//        let assertion = match result with
//            |LiveSignal(y) -> y = notSoldOn 
//            |_ -> false
//
//        assertion |> should be True 
//
//    let AssertOrderStillLive result notSoldOn =
//        let assertion = match result with
//            |Long(y,_)|Short(y,_) -> y = notSoldOn 
//            |_ -> false
//
//        assertion |> should be True 
//
//    let AssertDeadAt result soldOn =
//        let assertion = match result with
//            |DeadSignal(x,y) -> y = soldOn 
//            |_ -> false
//        assertion |> should be True 
//
//    let rand = Random()
//    
//    let AssertOrderDeadAt result soldOn =
//        let assertion = match result with
//            |DeadOrder(x,y) -> y = soldOn 
//            |_ -> false
//
//        assertion |> should be True 
//    
//    let strategy = TrendFollowerExit()
//    let ohclPoint s o c = {Start = seedDate s; Value = oc o c}
//    let randomActiveSignal =
//        let firstVal =  rand.NextDouble() * 100.0 |> decimal
//        let sndVal =  rand.NextDouble() * 100.0 |> decimal
//        Long({Start = seedDate( rand.Next(50)); Value = oc firstVal sndVal },5)
//
//    let exit sigs dir data = strategy.Exit dir sigs data
//
//    let getLiveOrders orders = 
//            orders 
//            |> unRavel
//            |> Seq.filter(fun x -> match x with |Long(_) |Short(_) -> true| _ -> false)
//            |> Seq.toList
//
//    let exitOrders dir orders data = strategy.ExitOrder dir orders data
//
//    [<Test>]
//    member I.``should return none if there are no signals passed in``() = 
//        //arrange
//        //act
//        let results = exit [] (down 1) [] 
//        //assert
//        results = [] |> should be True
//
//    [<Test>]
//    member I.``should return some values when raisedSignals are passed into exit``() = 
//        //arrange
//        let currentSignals = [LiveSignal({Start = seedDate 1;Value = oc 5m 10m}; )]
//        //act
//        let results = exit currentSignals (down 1) []
//        //assert
//        results |> should equal currentSignals 
//
//    [<Test>]
//    member I.``should return the same signals when there is no data present``() = 
//        //arrange
//        let currentSignals = [LiveSignal({Start = seedDate 1;Value = oc 5m 10m}; )]
//        //act
//        let results = exit currentSignals (down 1) []
//        //assert
//        results = currentSignals |> should be True 
//        
//    [<Test>]  
//    member I.``should make signals dead if set to 1 day down signal should sell 1 day after signal was purchased``() =
//        //arrange
//        let signalPurchased = {Start = seedDate 1;Value = oc 10m 8m};
//        let signalSoldOn = {Start = seedDate 2;Value = oc 8m 10m};
//        let currentSignals = [LiveSignal( signalPurchased )]
//
//        let data = [signalPurchased;signalSoldOn]
//        //act
//        let result::_ = exit currentSignals (down 1) data
//        //assert
//        AssertDeadAt result signalSoldOn
//
//
//    [<Test>]
//    member I.``should not make signals dead if set to 2 days down and only down for 1 day``() =
//        //arrange
//        let signalPurchased = {Start = seedDate 1;Value = oc 10m 8m};
//        let currentSignals = [LiveSignal( signalPurchased )]
//
//        let data = [signalPurchased;{Start = seedDate 2;Value = oc 8m 11m};{Start = seedDate 3;Value = oc 11m 12m}]
//        //act
//        let result::_ = exit currentSignals (down 2) data
//        //assert
//        AssertStillLive result signalPurchased
//
//    [<Test>]
//    member I.``should make single signal dead if set to 2 days down after 2 days down``() =
//        //arrange
//        let signalPurchased = {Start = seedDate 1;Value = oc 10m 10m};
//        let signalSoldOn = {Start = seedDate 4;Value = oc 8m 6m}
//        let currentSignals = [LiveSignal( signalPurchased )]
//
//        let data = [signalPurchased;
//            {Start = seedDate 2;Value = oc 10m 9m};
//            {Start = seedDate 3;Value = oc 9m 8m};
//            signalSoldOn
//            ]
//        //act
//        let result::_ = exit currentSignals (down 2) data
//        //assert
//        AssertDeadAt result signalSoldOn
//
//    [<Test>]
//    member I.``should make all signals dead if set to 1 day down after a day down``() =
//        //arrange
//        let signalPurchased = {Start = seedDate 1;Value = oc 10m 11m};
//        let sndSignalPurchased = {Start = seedDate 2;Value = oc 11m 15m};
//        let signalSoldOn = {Start = seedDate 4;Value = oc 8m 6m}
//        let currentSignals = [LiveSignal( signalPurchased ); LiveSignal(sndSignalPurchased)]
//
//        let data = [signalPurchased;
//                    sndSignalPurchased;
//            {Start = seedDate 3;Value = oc 15m 8m};
//            signalSoldOn
//            ]
//        //act
//        let results = exit currentSignals (down 1) data
//        //assert
//        results |> Seq.iter(fun res -> AssertDeadAt res signalSoldOn)
//
//    [<Test>]
//    member I.``should sell at first possible chance when a trend condition is met``() =
//        let signalToDie = {Start = seedDate 1;Value = oc 20m 19m}
//        let signalSoldOn = {Start = seedDate 3;Value = oc 18m 13m};
//        let currentSignals = [LiveSignal( signalToDie )]
//        let data = [signalToDie;
//                   {Start = seedDate 2;Value = oc 19m 18m};
//                   signalSoldOn;
//                   {Start = seedDate 4;Value = oc 13m 7m};
//                   {Start = seedDate 5;Value = oc 7m 5m};]
// 
//        //act
//        let result::_ = exit currentSignals (down 2) data
//        //assert
//        AssertDeadAt result signalSoldOn
//
//    [<Test>]
//    member I.``should only kill one signal if two signals are passed in and one meets down trend``() =
//          //arrange
//        let signalToDie = {Start = seedDate 1;Value = oc 20m 21m};
//        let signalToLive = {Start = seedDate 5;Value = oc 11m 15m};
//        let signalSoldOn = {Start = seedDate 4;Value = oc 15m 12m}
//        let currentSignals = [LiveSignal( signalToDie ); LiveSignal(signalToLive)]
//
//        let data = [signalToDie;
//                   {Start = seedDate 2;Value = oc 21m 18m};
//                   {Start = seedDate 3;Value = oc 18m 15m};
//                   signalSoldOn;
//                   signalToLive;
//                   {Start = seedDate 6;Value = oc 12m 14m};
//                   {Start = seedDate 7;Value = oc 14m 8m};
//            ]
//        //act
//        let result::sndResult::_ = exit currentSignals (down 2) data
//        //assert
//        AssertDeadAt sndResult signalSoldOn
//        AssertStillLive result signalToLive
//
//
//    [<Test>]
//    member I.``should return a empty collection if no orders are passed in``() =
//        //arrange
//        let data = []
//        let orders = []
//        //act
//        let exitedOrders = exitOrders (down 2) orders data
//        //assert
//        exitedOrders |> should be Empty
//
//
//    [<Test>]
//    member I.``should return same collection back for orders if all orders are dead``() =
//        //arrange
//        let data = [
//            {Start = seedDate 1;Value = oc 6m 10m}
//            {Start = seedDate 2;Value = oc 10m 9m};
//            {Start = seedDate 3;Value = oc 9m 8m};
//            {Start = seedDate 4;Value = oc 8m 18m};
//           ]
//        let orders = [
//            DeadOrder(randomActiveSignal, ohclPoint 1 2m 3m)
//            DeadOrder(randomActiveSignal, ohclPoint 1 2m 3m)
//            DeadOrder(randomActiveSignal, ohclPoint 1 2m 3m)
//            DeadOrder(randomActiveSignal, ohclPoint 1 2m 3m)
//            ]
//        //act
//        let exitedOrders = exitOrders (down 2) orders data
//        //assert
//        orders 
//        |> List.forall(fun x -> match x with DeadOrder(_) -> true |_ -> false)
//        |> should be True
//
//    
//    [<Test>]
//    member I.``should not make orders dead if set to 2 days down and only down for 1 day``() =
//        //arrange
//        let signalPurchased = {Start = seedDate 1;Value = oc 10m 8m};
//        let orders = [Long( signalPurchased,5 )]
//
//        let data = [signalPurchased;{Start = seedDate 2;Value = oc 8m 11m};{Start = seedDate 3;Value = oc 11m 12m}]
//        //act
//        let result::_ = exitOrders (down 2) orders data
//        //assert
//        AssertOrderStillLive result signalPurchased
//
//    [<Test>]
//    member I.``should make order dead if set to 1 day down signal should sell 1 day after order was purchased``() =
//        //arrange
//        let signalPurchased = {Start = seedDate 1;Value = oc 10m 8m};
//        let signalSoldOn = {Start = seedDate 2;Value = oc 8m 10m};
//        let orders = [Long(signalPurchased,5)]
//
//        let data = [signalPurchased;signalSoldOn]
//        //act
//        let result::_ = exitOrders (down 1) orders data
//        //assert
//        AssertOrderDeadAt result signalSoldOn
//
//    [<Test>]
//    member I.``should make single order dead if set to 2 days down after 2 days down``() =
//        //arrange
//        let signalPurchased = {Start = seedDate 1;Value = oc 10m 10m};
//        let signalSoldOn = {Start = seedDate 4;Value = oc 8m 6m}
//        let currentSignals = [Long( signalPurchased,5 )]
//
//        let data = [signalPurchased;
//            {Start = seedDate 2;Value = oc 10m 9m};
//            {Start = seedDate 3;Value = oc 9m 8m};
//            signalSoldOn
//            ]
//        //act
//        let result::_ = exitOrders (down 2) currentSignals data
//        //assert
//        AssertOrderDeadAt result signalSoldOn
//    
//    [<Test>]
//    member I.``should sell orders at first possible chance when a trend condition is met``() =
//        let signalToDie = {Start = seedDate 1;Value = oc 20m 19m}
//        let signalSoldOn = {Start = seedDate 3;Value = oc 18m 13m};
//        let currentSignals = [Long( signalToDie,5 )]
//        let data = [signalToDie;
//                   {Start = seedDate 2;Value = oc 19m 18m};
//                   signalSoldOn;
//                   {Start = seedDate 4;Value = oc 13m 7m};
//                   {Start = seedDate 5;Value = oc 7m 5m};]
// 
//        //act
//        let result::_ = exitOrders  (down 2) currentSignals data 
//        //assert
//        AssertOrderDeadAt result signalSoldOn
//
//    [<Test>]
//    member I.``should only kill one order if two signals are passed in and one meets down trend``() =
//          //arrange
//        let signalToDie = {Start = seedDate 1;Value = oc 20m 21m};
//        let signalToLive = {Start = seedDate 5;Value = oc 11m 15m};
//        let signalSoldOn = {Start = seedDate 4;Value = oc 15m 12m}
//        let currentSignals = [Long( signalToDie,5 ); Long(signalToLive,5)]
//
//        let data = [signalToDie;
//                   {Start = seedDate 2;Value = oc 21m 18m};
//                   {Start = seedDate 3;Value = oc 18m 15m};
//                   signalSoldOn;
//                   signalToLive;
//                   {Start = seedDate 6;Value = oc 12m 14m};
//                   {Start = seedDate 7;Value = oc 14m 8m};
//            ]
//        //act
//        let result::sndResult::_ = exitOrders (down 2) currentSignals data
//        //assert
//        AssertOrderDeadAt sndResult signalSoldOn
//        AssertOrderStillLive  result signalToLive
////
////[<TestFixture>]
////type ``Strategy runner tests``() =
////
////
////    let backtestWithPrcentCapitalOffer prcentageToUse entryDirection exitStrategy startBal data window = 
////        let riskMan = SimpleRiskManager(prcentageToUse)
////        let backtester = new Backtester(riskMan.Offer);
////        let entry = TrendFollowerEntry()
////        let exit = TrendFollowerExit()
////        backtester.Backtest (entry.EnterToOrder entryDirection ) (exit.ExitOrder exitStrategy) data window startBal 
////
////    let backtest entryDirection exitStrategy startBal data window = 
////        backtestWithPrcentCapitalOffer 0.01m entryDirection exitStrategy startBal data window
////
////    [<Test>]
////    member I.``should return empty collection and the same balance if an empty data set is passed in``() =
////        //arrange
////        let values = []
////        //act
////        let result = backtest (up 2) (down 1) 10000.0m values 3
////        //assert
////        match result with
////        |{Balance = 10000.0m; TradeHistory = [];  } -> true
////        |_ -> false
////        |> should be True
////
////
////    [<Test>]
////    member I.``should have balance substracted from correctly if i buy 1 signal during backtest``() =
////        //arrange
////        let day1 = seedDate 1
////        let day2 = seedDate 2
////        let day3 = seedDate 3
////        let signalToBuy = {Start = day1;Value = oc 15m 18m}
////        let buyOn = {Start = day2;Value = oc 18m 15m};
////        let sellOn = {Start = day3;Value = oc 15m 17m}
////        let values = [signalToBuy; buyOn;sellOn]
////        //act
////        let result = backtest (up 1) (down 1) 10000.0m values 3
////        //assert
////        let expectedDead = DeadOrder(Long(buyOn,5),sellOn)
////        match result with
////        |{
////            Balance = 8176.78443206045m;
////            TradeHistory = [expectedDead]} -> true
////        |_ -> false
////        |> should be True
////
////    [<Test>]
////    member I.``Integration test : should raise 3 orders when set on 2 up over long series of data``()=
////        //arrange 
////        let fstPurchase = {Start = seedDate 3;Value = oc 200m 120m};
////        let sndPointOfPurchase = {Start = seedDate 10;Value = oc 150m 190m};
////        let trdPointOfPurchase = {Start = seedDate 11;Value = oc 190m 170m};
////        let points = [ {Start = seedDate 1;Value = oc 50m 100m};
////                       {Start = seedDate 2;Value = oc 100m 200m};
////                       fstPurchase;
////                       {Start = seedDate 4;Value = oc 120m 40m};
////                       {Start = seedDate 5;Value = oc 40m 30m};
////                       {Start = seedDate 6;Value = oc 30m 70m};
////                       {Start = seedDate 7;Value = oc 70m 50m};
////                       {Start = seedDate 8;Value = oc 50m 110m};
////                       {Start = seedDate 9;Value = oc 110m 150m};
////                       sndPointOfPurchase;
////                       trdPointOfPurchase;
////                       {Start = seedDate 12;Value = oc 170m 16m};
////                       {Start = seedDate 13;Value = oc 160m 17m};
////                        ]
////        //act
////        let result = backtest (up 2) (down 2) 100000.0m points 3
////        //assert 
////        let day1 = fstPurchase.Start
////        let day2 = sndPointOfPurchase.Start
////        let day3 = trdPointOfPurchase.Start
////        match result with
////        |{ Balance = -71674.96482231878m }-> true
//////            TradeHistory = [expectedDead]; 
//////            AssetHistory = [ day1,10000.0m;day2,99538m]} 
////        |_ -> false
////        |> should be True
////
////
////    [<Test>]
////    member I.``Integration test : should include the value of a sold trade on next potential trade``()=
////        //arrange
////        let buyOn ={Start = seedDate 2;Value = oc 5m 20m}
////        let secondBuy ={Start = seedDate 3;Value = oc 20m 18m};
////        let sellAllOn ={Start = seedDate 4;Value = oc 18m 15m};
////        let nextTrade = {Start = seedDate 7;Value = oc 6m 19m};
////        let values = [
////                {Start = seedDate 1;Value = oc 3m 5m};
////                buyOn
////                secondBuy
////                sellAllOn
////                {Start = seedDate 5;Value = oc 15m 5m};
////                {Start = seedDate 6;Value = oc 5m 6m};
////                nextTrade
////            ]
////        //act
////        let result = backtestWithPrcentCapitalOffer 0.7m (up 1) (down 1) 1000.0m values 3
////        //assert
////        let expected2ndDead = DeadOrder(Long(secondBuy,10),sellAllOn)
////        let expectedDead = DeadOrder(Long(buyOn,140),sellAllOn)
////        match result with
////        |{
////            Balance = 2175.573329804234m;
////            TradeHistory = [Long(nextTrade,253);expectedDead;expected2ndDead;]} -> true
////        |_ -> false
////        |> should be True
////
////    [<Test>]
////    member I.``Integration test : should correctly work out rolling returns values``()=
////        //arrange
////        let buyOn ={Start = seedDate 2;Value = oc 5m 20m}
////        let secondBuy ={Start = seedDate 3;Value = oc 20m 18m};
////        let sellAllOn ={Start = seedDate 4;Value = oc 18m 15m};
////        let nextTrade = {Start = seedDate 7;Value = oc 6m 19m};
////        let values = [
////                {Start = seedDate 1;Value = oc 3m 5m};
////                buyOn
////                secondBuy
////                sellAllOn
////                {Start = seedDate 5;Value = oc 15m 5m};
////                {Start = seedDate 6;Value = oc 5m 6m};
////                nextTrade
////            ]
////        //act
////        let result = backtestWithPrcentCapitalOffer 0.7m (up 1) (down 1) 1000.0m values 3
////        //assert
////        let expected2ndDead = DeadOrder(Long(secondBuy,10),sellAllOn)
////        let expectedDead = DeadOrder(Long(buyOn,140),sellAllOn)
////        let (firsResult,secondReslt) = result.Returns |> Seq.nth 0, result.Returns |> Seq.nth 1
////        firsResult |> should equal (seedDate 7, 1.175573329804234m)
////
////
////
////    [<Test>]
////    member I.``Integration test : should not charge a raised trade twice``()=
////        //arrange
////        let buyOn ={Start = seedDate 5;Value = oc 22m 19m}
////        let sellOn ={Start = seedDate 8;Value = oc 15m 13m}
////        let values = [
////                { Start = seedDate 1;Value = oc 6m 5m};
////                { Start = seedDate 2;Value = oc 5m 4m};
////                { Start = seedDate 3;Value = oc 5m 3m};
////                { Start = seedDate 4;Value = oc 3m 22m};
////                buyOn
////                {Start = seedDate 6;Value = oc 19m 18m};
////                {Start = seedDate 7;Value = oc 18m 15m};
////                sellOn
////            ]
////        //act
////        let result = backtestWithPrcentCapitalOffer 0.1m (up 1) (down 3) 1000.0m values 5
////        //assert
//////        let expectedDead = DeadOrder(Long(buyOn,140),sellAllOn)
////        match result with
////        |{
////            Balance = 617.007747743894m;} -> true
////        |_ -> false
////        |> should be True
//
//
////true |> should be True
////
////false |> should not (be True)
////
////[] |> should be Empty
////
////[1] |> should not (be Empty)
////
////"" |> should be EmptyString
////
////"" |> should be NullOrEmptyString
////
////null |> should be NullOrEmptyString
////
////null |> should be Null
////
////anObj |> should not (be Null)
////
////anObj |> should be (sameAs anObj)
////
////anObj |> should not (be sameAs otherObj)