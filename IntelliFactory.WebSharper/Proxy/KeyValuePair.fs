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

namespace IntelliFactory.WebSharper

[<Proxy(typeof<System.Collections.Generic.KeyValuePair<_,_>>)>]
type private KeyValuePairProxy<'K,'V> =
    [<Inline "{K: $key, V: $value}">]
    new (key: 'K, value: 'V) = {}
    member this.Key     with [<Inline "$this.K">] get () = X<'K>
    member this.Value   with [<Inline "$this.V">] get () = X<'V>
