// $begin{copyright}
//
// This file is part of WebSharper
//
// Copyright (c) 2008-2013 IntelliFactory
//
// GNU Affero General Public License Usage
// WebSharper is free software: you can redistribute it and/or modify it under
// the terms of the GNU Affero General Public License, version 3, as published
// by the Free Software Foundation.
//
// WebSharper is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License
// for more details at <http://www.gnu.org/licenses/>.
//
// If you are unsure which license is appropriate for your use, please contact
// IntelliFactory at http://intellifactory.com/contact.
//
// $end{copyright}

namespace IntelliFactory.WebSharper.Sitelets

/// Represents addressable locations.
type Location = System.Uri

/// Provides a bijection between URL locations and abstract actions.
[<Sealed>]
type Router<'Action when 'Action : equality> =

    /// Tries to constructs a link to a given action. Fails with None
    /// if the action is not understood by the router.
    member Link : action: 'Action -> option<Location>

    /// Tries to route a request to an action. Fails if the request
    /// is not understood by the router.
    member Route : req: Http.Request -> option<'Action>

    /// Combines two routers. The combined router
    static member ( <|> ) : Router<'Action> * Router<'Action> -> Router<'Action>

/// Provides combinators over the Router type.
module Router =

    /// Constructs a custom new router with a given route and link functions.
    val New<'T when 'T : equality> :
        route : (Http.Request -> option<'T>) ->
        link : ('T -> option<Location>) ->
        Router<'T>

    /// Constructs a router from a finite table defining a
    /// bijection between locations and actions. Throws InvalidArgument
    /// exceptions if the table does not define a bijection.
    val Table<'T when 'T : equality> : mapping: seq<'T * string> -> Router<'T>

    /// Infers the router by analyzing an algebraic action data type.
    val Infer<'T when 'T : equality> : unit -> Router<'T>

    /// Composes several routers. For both linking and routing,
    /// the leftmost matching router is selected. Two routers can be
    /// composed with the `<|>` combinator.
    val Sum<'T when 'T : equality> : routers: seq<Router<'T>> -> Router<'T>

    /// Maps over a router, changing its action type.
    val Map<'T1,'T2 when 'T1 : equality and 'T2 : equality> :
        encode: ('T1 -> 'T2) ->
        decode: ('T2 -> 'T1) ->
        router: Router<'T1> ->
        Router<'T2>

    /// Shifts the router's locations by adding a prefix.
    val Shift<'T when 'T : equality> : prefix: string -> router: Router<'T> -> Router<'T>

    /// Creates a router with Link always failing,
    /// and Route picking a POST-ed parameter with a
    /// given key, for example PostParameter "id" routes
    /// request POST id=123 to action "123".
    val FromPostParameter : name: string -> Router<string>

    /// Creates a router with Link always failing,
    /// and Route picking a POST-ed parameter with a
    /// given key, assumes that the corresponding value is
    // a JSON string, and tries to decode it into a value.
    val FromJsonParameter<'T when 'T : equality> :
        name: string -> Router<'T>

    /// Modifies the router to use a constant URL.
    val At<'T when 'T : equality> : url: string -> Router<'T> -> Router<'T>

    /// The empty router, the unit of <|>.
    val Empty<'T when 'T : equality> : Router<'T>

    /// Maps over the action type of the router.
    val TryMap<'T1,'T2 when 'T1 : equality and 'T2 : equality> :
        encode: ('T1 -> option<'T2>) ->
        decode: ('T2 -> option<'T1>) ->
        router: Router<'T1> ->
        Router<'T2>
