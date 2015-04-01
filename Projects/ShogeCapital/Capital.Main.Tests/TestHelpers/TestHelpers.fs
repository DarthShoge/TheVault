module Capital.Main.Tests.TestHelpers
open System
open Capital.DataStructures


//    let dt(y,m,d) = DateTime(y,m,d)
    let dt v = DateTime(2011,01,01).AddDays(v)
    let tck  o c d = {Date = d; O = o;H = o;L = c; C = c;AC =c; V = 100.} 
    let n = MathNet.Numerics.Distributions.Normal(0.,1.)
//    let dateOffset length = [1..length] 
//                            |> Seq.map(fun x -> dt (float x)) 
//                            |> Seq.toArray