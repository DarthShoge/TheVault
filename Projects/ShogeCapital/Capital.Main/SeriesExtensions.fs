namespace Capital.Extensions
open Deedle

module Seq =

    let tryTake (n : int) (s : _ seq) =
        let e = s.GetEnumerator ()
        let i = ref 0
        seq {
            while e.MoveNext () && !i < n do
                i := !i + 1
                yield e.Current
        }


module List =
    let  slide (n : int) (s : _ list) =
        let rec buildWindows v sequence acc = 
            match sequence with
            |[] -> acc
            |h::t ->let currentV,newSeq =  
                        if v >= n then
                            let newSeq = sequence|> Seq.tryTake n |> Seq.toList
                            if n > (t |> List.length) then (newSeq,[])
                            else (newSeq, t)
                        else (sequence|> Seq.tryTake v |> Seq.toList , sequence)
                    buildWindows (v+1) newSeq (currentV::acc)
        buildWindows 1 s [] |> List.rev

    let  flow  (accum : 'a list -> 'b list -> 'b list) (s : _ list) =
        let rec trickle (acc: 'b list) k = 
            if s.Length <= k then acc
            else
                let nwList = s |> Seq.take k |> Seq.toList
                let accumulator = (accum nwList acc)
                trickle accumulator (k + 1)
        trickle [] 1


    let cut n s =
        let rec cutIter (n: int) acc (ss : _ list) =
            let trueN = if  n > ss.Length then ss.Length else n
            if ss |> Seq.length = 0 || n <= 0 then acc
            else cutIter n ((ss |> Seq.take trueN)::acc) (ss |> Seq.skip trueN |> Seq.toList)
        cutIter n [] s 

