module FsUnitExtensions

open NUnit.Framework
open ShogeLabs.PatternRecognition
open System
open FsUnit


let equalWithToleranceOf x y =
        let cons = NUnit.Framework.Constraints.EqualConstraint(y)
        cons.Within(x)
