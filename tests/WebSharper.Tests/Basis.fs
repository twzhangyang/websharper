// $begin{copyright}
//
// This file is part of WebSharper
//
// Copyright (c) 2008-2016 IntelliFactory
//
// Licensed under the Apache License, Version 2.0 (the "License"); you
// may not use this file except in compliance with the License.  You may
// obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
// implied.  See the License for the specific language governing
// permissions and limitations under the License.
//
// $end{copyright}

module WebSharper.Tests.Basis

open WebSharper
open WebSharper.JavaScript
open WebSharper.Testing

#nowarn "40" // recursive values

[<JavaScript>]
let rec private fac : int -> int = function
    | 0 -> 1
    | n -> n * fac (n - 1)

[<JavaScript>]
let rec private factorial n =
    match n with
    | 0 -> 1
    | n -> n * factorial (n - 1)

[<JavaScript>]
let private tailRecFactorialCurried n =
    let rec factorial acc n =
        match n with
        | 0 -> acc
        | n -> factorial (n * acc) (n - 1)
    factorial 1 n

[<JavaScript>]
let private tailRecFactorialCurried2 n =
    let rec factorial acc = function
        | 0 -> acc
        | n -> factorial (n * acc) (n - 1)
    factorial 1 n

[<JavaScript>]
let private tailRecFactorialTupled n =
    let rec factorial (acc, n) =
        match n with
        | 0 -> acc
        | n -> factorial (n * acc, n - 1)
    factorial (1, n)

[<JavaScript>]
let private tailRecSingle n =
    let rec f n =
        if n > 0 then f (n - 1) else 0
    f n

[<JavaScript>]
let tailRecScoping n =
    let rec f acc n =
        if n > 0 then f ((fun () -> n) :: acc) (n - 1) else acc
    f [] n

[<JavaScript>]
let private tailRecSingleNoReturn n =
    let rec f n =
        if n > 0 then f (n - 1)
    f n

[<JavaScript>]
let private tailRecSingleUsedInside n =
    let mutable setf = fun x -> 0
    let rec f n =
        setf <- fun x -> f x
        if n > 0 then f (n - 1) else 0
    f n

[<JavaScript>]
let private tailRecMultiple n =
    let rec f n =
        if n > 0 then g (n - 1) else 0
    and g n =
        if n > 0 then f (n - 1) else 1
    f n

[<JavaScript>]
let private tailRecMultipleNoReturn n =
    let rec f n =
        if n > 0 then g (n - 1)
    and g n =
        if n > 0 then f (n - 1)
    f n

[<JavaScript>]
let private tailRecWithMatch l =
    let rec f acc l =
        match l with
        | [] -> acc
        | h :: t -> f (h :: acc) t
    f [] l

[<JavaScript>]
let rec moduleTailRecSingle n =
    if n > 0 then moduleTailRecSingle (n - 1) else 0

let rec [<JavaScript>] moduleTailRecMultiple1 n =
    if n > 0 then moduleTailRecMultiple2 (n - 1) else 0
and [<JavaScript>] moduleTailRecMultiple2 n =
    if n > 0 then moduleTailRecMultiple1 (n - 1) else 1

[<JavaScript>]
type TailRec() =
    let rec classTailRecSingle n =
        if n > 0 then classTailRecSingle (n - 1) else 0

    let rec classTailRecCurried n m =
        if n > 0 then classTailRecCurried (n - 1) (m - 1) else 0

    let rec classTailRecSingleUsedInside n =
        let mutable setf = fun x -> 0
        let rec f n =
            setf <- fun x -> f x
            if n > 0 then f (n - 1) else 0
        f n

    let rec classTailRecMultiple1 n =
        if n > 0 then classTailRecMultiple2 (n - 1) else 0
    and classTailRecMultiple2 n =
        if n > 0 then classTailRecMultiple1 (n - 1) else 1

    member this.TailRecSingle n = classTailRecSingle n
    member this.TailRecMultiple n = classTailRecMultiple1 n

    member this.TailRecSingle2 n =
        if n > 0 then this.TailRecSingle2 (n - 1) else 0

[<JavaScript>]
let private tailRecWithValue n =
    let rec f n =
        if n > 0 then f (n - i) else i
    and i = 1
    f n

[<JavaScript>]
let private tailRecMultipleWithValue n =
    let rec f n =
        if n > 0 then g (n - i) else 0
    and g n =
        if n > 0 then f (n - i) else 1
    and i = 1
    f n

[<JavaScript>]
let rec private forall f = function
    | []      -> true
    | x :: xs -> if f x then forall f xs else false

[<JavaScript>]
let private shadow bar  =
    let bar = bar
    bar

[<JavaScript>]
let rec private shadowRec bar =
    let bar = bar
    bar

module private Peano =

    type Peano = Z | S of Peano

    [<JavaScript>]
    let rec toNat = function
        | Z   -> 0
        | S x -> 1 + toNat x

    [<JavaScript>]
    let rec ofNat = function
        | 0 -> Z
        | x -> S (ofNat (x - 1))

type private T1 [<JavaScript>] () =

    [<JavaScript>]
    member this.Property = "Initial Value"

    [<JavaScript>]
    member this.Member(s:string) = "Member: " + s

    [<JavaScript>]
    static member StaticMember(s:string) = "Static Member: " + s

    [<JavaScript>]
    static member StaticMemberUnit() = ()

    [<JavaScript>]
    static member StaticMemberCurry(s:string)(s2:string) = s + s2

    [<JavaScript>]
    static member StaticProperty = "Initial Static Value"

[<JavaScript>] type private T2 = { [<Name "Y">] X : int }

[<Inline "isNaN($x)">]
let private isNaN (x: double) = System.Double.IsNaN x

[<JavaScript>]
let InnerGenerics pred l =
    let rec loop l cont =
        match l with
        | [] -> ([],[])
        | x::[] when pred x -> 
            (cont l, [])
        | x::xs when not (pred x) -> (cont [], l)
        | x::xs when pred x -> loop xs (fun rest -> cont (x::rest))
        | _ -> failwith "Unrecognized pattern"
    loop l id

[<JavaScript>]
let Tests =
    TestCategory "Basis" {

        Test "Comparisons" {
            isTrueMsg (not (not true))  "not (not true)"
            isTrueMsg (not false)       "not false"
            isTrueMsg (not (1 <> 1))    "1 <> 1"
            isTrueMsg (1 <> 2)          "1 <> 2"
            isTrueMsg (1 < 2)           "1 < 2"
            isTrueMsg (not (1 < 1))     "1 < 1"
            isTrueMsg (not (1 < 0))     "1 < 0"
            isTrueMsg (2 > 1)           "2 > 1"
            isTrueMsg (not (2 > 2))     "2 > 2"
            isTrueMsg (not (2 > 3))     "2 > 3"
            isTrueMsg (1 <= 2)          "1 <= 2"
            isTrueMsg (1 <= 1)          "1 <= 1"
            isTrueMsg (not (1 <= 0))    "1 <= 0"
            isTrueMsg (2 >= 1)          "1 >= 1"
            isTrueMsg (2 >= 2)          "1 >= 2"
            isTrueMsg (not (2 >= 3))    "2 >= 3"
        }

        let closedLet =
            let a : list<int> = List.empty
            fun () -> a

        Test "Let" {
            equalMsg [] (closedLet()) "[] = closedLet"
        }

        Test "Factorial" {
            equalMsg (6 * 5 * 4 * 3 * 2) (fac 6)       "fac 6"
            equalMsg (6 * 5 * 4 * 3 * 2) (factorial 6) "factorial 6"
        }

        Test "Tail calls" {
            equalMsg (6 * 5 * 4 * 3 * 2) (tailRecFactorialCurried 6) "curried tail call"
            equalMsg (6 * 5 * 4 * 3 * 2) (tailRecFactorialCurried2 6) "curried tail call with function"
            equalMsg (6 * 5 * 4 * 3 * 2) (tailRecFactorialTupled 6) "tupled tail call"
            equalMsg 0 (tailRecSingle 5) "single let rec"
            equalMsg [1; 2; 3; 4; 5] (tailRecScoping 5 |> List.map (fun f -> f())) "scoping while tail call optimizing"
            equalMsg [ 1; 2; 3 ] (tailRecWithMatch [ 3; 2; 1 ]) "single let rec with non-inlined match expression"
            equalMsg 1 (tailRecMultiple 5) "mutually recursive let rec"
            equalMsg 1 (tailRecWithValue 5) "mutually recursive let rec with a function and a value"
            equalMsg 1 (tailRecMultipleWithValue 5) "mutually recursive let rec with two functions and a value"
            equalMsg 0 (moduleTailRecSingle 5) "single let rec in module"
            equalMsg 1 (moduleTailRecMultiple1 5) "mutually recursive let rec in module 1"
            equalMsg 0 (moduleTailRecMultiple2 5) "mutually recursive let rec in module 2"
            let o = TailRec()
            equalMsg 0 (o.TailRecSingle 5) "single let rec in class constructor"
            equalMsg 1 (o.TailRecMultiple 5) "mutually recursive let rec in class constructor"
            // test if there is no infinite loop
            tailRecSingleNoReturn 5
            tailRecMultipleNoReturn 5
        }

        let propPeano x = x = Peano.toNat (Peano.ofNat x)

        Test "forall" {
            isTrueMsg (forall (fun x -> x > 0) [1..10]) "forall x in 1..10: x > 0"
        }

        Test "Peano" {
            isTrueMsg (propPeano 0)              "propPeano 0"
            isTrueMsg (propPeano 1)              "propPeano 1"
            isTrueMsg (propPeano 2)              "propPeano 2"
            isTrueMsg (forall propPeano [3..10]) "propPeano 3..10"
        }

        let rec fact = function
            | 0 -> 1
            | n -> n * (fact (n - 1))

        Test "Nesting" {
            equalMsg (fact 6) 720 "fact 6 = 720"
        }

        Test "While" {
            let i     = ref 0
            let accum = ref 0
            let _ =
                while !i <= 3 do
                    accum := !accum + !i
                    i := !i + 1
            equal !accum 6
        }

        Test "For" {
            let accum = ref 0
            let _ =
                for i in 0 .. 3 do
                    accum := !accum + i
            equal !accum 6
        }

        Test "Floats" {
            notEqualMsg 1. 2.             "1. <> 2."
            equalMsg 1. 1.                "1. = 1."
            equalMsg (3./2.) 1.5          "3./2. = 1.5"
            equalMsg (1 + 2 * 4 / 6) 2    "1 + 2 * 4 / 6 = 2"
            let fEpsilon = 1.40129846e-45f
            equalMsg (1.f + fEpsilon) 1.f "1.f + \\epsilon = 1.f"
            let dEpsilon = 4.940656458e-324
            equalMsg (1. + dEpsilon) 1.   "1. + \\epsilon = 1."
            notEqualMsg fEpsilon 0.f      "\\epsilon <> 0.f"
            notEqualMsg dEpsilon 0.       "\\epsilon <> 0."
        }

        Test "NaN" {
            notEqualMsg (box nan) null "box nan <> null"
            forEach [(+); (-); (*); (/)] (fun op -> Do {
                forEach { 0. .. 10. } (fun x -> Do {
                    isTrueMsg (isNaN (op x nan)) ("op(x,nan) = nan where x=" + string x)
                })
            })
        }

        Test "Infinity" {
            notEqualMsg (box infinity) null   "infinity <> null"
            equalMsg (1./0.) infinity         "1./0. = infinity"
            forEach { 0. .. 10. } (fun x -> Do {
                equalMsg (x / infinity) 0. ("x/infinity = 0 where x = " + string x)
            })
        }

        Test "Booleans" {
            isTrueMsg (true && true)  "true && true"
            isTrueMsg (true || false) "true || false"
        }

        Test "Ranges" {
            equalMsg [|1..5|] [|1;2;3;4;5|] "1..5"
            equalMsg [1..5] [1;2;3;4;5] "1..5"
        }

        Test "Tuples" {
            let (a, b, c) = (1, 2, 3)
            equal (a + b + c) 6
            let t = ("Hello ", "Szia ", "Hej")
            let (t1, t2, t3) = t
            equal (t1 + t2 + t3) "Hello Szia Hej"
            isTrueMsg ((1, 2) < (1, 3)) "(1, 2) < (1, 3)"
            isTrueMsg ((1, 2) > (1, 1)) "(1, 2) > (1, 1)"
            isTrueMsg ((1, 8) < (2, 1)) "(1, 8) < (2, 1)"
        }

//        Test "Struct tuples" {
//            let struct (a, b, c) = struct (1, 2, 3)
//            equal (a + b + c) 6
//            let t = struct ("Hello ", "Szia ", "Hej")
//            let struct (t1, t2, t3) = t
//            equal (t1 + t2 + t3) "Hello Szia Hej"
//            isTrueMsg (struct (1, 2) < struct (1, 3)) "(1, 2) < (1, 3)"
//            isTrueMsg (struct (1, 2) > struct (1, 1)) "(1, 2) > (1, 1)"
//            isTrueMsg (struct (1, 8) < struct (2, 1)) "(1, 8) < (2, 1)"
//        }

        Test "Currying" {
            let add (x, y) = x + y
            let add' y = add y
            equal (add' (1, 2)) 3
        }

        let rec odd =
            function  0 -> false
                    | n -> even (n-1)
        and even =
            function  0 -> true
                    | n -> odd (n-1)

        Test "Recursion" {
            isTrueMsg (even 12) "even 12"
            isTrueMsg (odd 23) "odd 23"
        }

        Test "Shadowing" {
            equal (shadow 1) 1
            equal (shadowRec 1) 1
        }

        Test "Equality" {
            isTrueMsg (1 = 1)                         "1 = 1"
            isTrueMsg (1 <> 2)                        "1 <> 2"
            isTrueMsg (box (1,2,3) <> box (1,(2,3)))  "(1,2,3) <> (1,(2,3))"
            isTrueMsg (null = null)                   "null = null"
            isTrueMsg ("" <> null)                    "\"\" <> null"
            isTrueMsg ("Szia" = "Szia")               "\"Szia\" = \"Szia\""
            isTrueMsg ("Szia" <> "Hello")             "\"Szia\" = \"Hello\""
            isTrueMsg ([1;2;3;4] = [1..4])            "[1;2;3;4] = [1;2;3;4]"
            isTrueMsg (Some 3 = Some 3)               "Some 3 = Some 3"
            isTrueMsg ([|1;2;3;4|] = [|1..4|])        "[|1;2;3;4|] = [|1;2;3;4|]"
            isTrueMsg ([|1;2;3|] <> [|1..4|])         "[|1;2;3|] <> [|1;2;3;4|]"
            isTrueMsg ([1;2;3] <> [1..4])             "[1;2;3] <> [1;2;3;4]"
            isTrueMsg ((1,2,3) = (1,2,3))             "(1,2,3) = (1,2,3)"
            isTrueMsg (forall (fun x -> x=x) [0..10]) "forall x in 0..10, x = x"
            isTrueMsg ((1,"b",3) <> (1,"a",3))        "(1,\"b\",3) <> (1,\"a\",3)"
            isTrueMsg (box 0 <> box "")               "0 <> \"\""
            isTrueMsg (box 0 <> box "0")              "0 <> \"0\""
            isTrueMsg (box 0 <> box false)            "0 <> false"
            isTrueMsg (box 0 <> box JS.Undefined)     "0 <> undefined"
            isTrueMsg (box false <> box JS.Undefined) "false <> undefined"
            isTrueMsg (null <> box JS.Undefined)      "null <> undefined"
            isTrueMsg (box " \t\r\n" <> box 0)        "whitespace <> 0"
        }

        Test "Objects" {
            let t = T1()
            equalMsg (t.Member "X") "Member: X"                 "t1.Member"
            equalMsg t.Property "Initial Value"                 "t1.Property"
            equalMsg (T1.StaticMember "X") "Static Member: X"   "T1.StaticMember"
            equalMsg T1.StaticProperty "Initial Static Value"   "T1.StaticProperty"
            equalMsg (T1.StaticMemberCurry "x" "y") "xy"        "T1.StaticMemberCurry"
        }

        Test "Renaming" {
            equal {X=1}?Y 1
        }

        Test "JavaScript object" {
            let o = New [ "a", box "1"; "b", box 2 ]
            isTrueMsg (o?a = "1" && o?b = 2) "List of tuples"
            let o2 = [ "a", 1; "b", 2 ] |> List.map (fun (n, v) -> n, box (v + 1)) |> New
            isTrueMsg (o2?a = 2 && o2?b = 3) "Mapped list of tuples"
            let o3 = New [| "a", box "1"; "b", box 2 |]
            isTrueMsg (o3?a = "1" && o3?b = 2) "Array of tuples"
            let o4 = New (seq { yield "a", box "1"; yield "b", box 2; })
            isTrueMsg (o4?a = "1" && o4?b = 2) "Sequence of tuples"
            let o5 = New [ "a" => "1"; "b" => 2 ]
            isTrueMsg (o5?a = "1" && o5?b = 2) "List of (=>) calls"
            let o6 = [ "a", 1; "b", 2 ] |> List.map (fun (n, v) -> n, box (v + 1)) |> New
            isTrueMsg (o6?a = 2 && o6?b = 3) "Mapped list of (=>) calls"
            let o7 = New [| "a" => "1"; "b" => 2 |]
            isTrueMsg (o7?a = "1" && o7?b = 2) "Array of (=>) calls"
            let o8 = New (seq { yield "a" => "1"; yield "b" => 2; })
            isTrueMsg (o8?a = "1" && o8?b = 2) "Sequence of (=>) calls"
        }

        Test "JS.Inline" {
            equal (JS.Inline "1 + 2") 3
            equal (JS.Inline("$0 + $1", 1, 2)) 3
            let r = ref 0
            let next() =
                incr r 
                !r
            equal (JS.Inline("$0 + $1 + $1", next(), next())) 5
        }

        Test "simplified match" {
            let res =
                let mutable r = 0
                match 1 + 1 with
                | 0 -> r <- 1
                | 1
                | 2 -> r <- 3
                | 5 -> r <- 6
                | _ -> ()
                r
            equal res 3
        }

        Test "Let rec initialization" {
            let res =
                let rec a = b() + 1
                and b() = 2
                a
            equal res 3 
            let res2 =
                let rec a = b + 1
                and b = 2
                a
            equal res2 3           
        }

        Test "Local function" {
            let res =
                let f x = x + 1
                f 1, f 2
            equal res (2, 3)
        }
    }
