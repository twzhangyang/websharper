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

[<Proxy(typeof<Choice<_,_>>)>]
type private ChoiceProxy<'T1,'T2> =
    | Choice1Of2 of 'T1
    | Choice2Of2 of 'T2

[<Proxy(typeof<Choice<_,_,_>>)>]
type private ChoiceProxy<'T1,'T2,'T3> =
    | Choice1Of3 of 'T1
    | Choice2Of3 of 'T2
    | Choice3Of3 of 'T3

[<Proxy(typeof<Choice<_,_,_,_>>)>]
type private ChoiceProxy<'T1,'T2,'T3,'T4> =
    | Choice1Of4 of 'T1
    | Choice2Of4 of 'T2
    | Choice3Of4 of 'T3
    | Choice4Of4 of 'T4

[<Proxy(typeof<Choice<_,_,_,_,_>>)>]
type private ChoiceProxy<'T1,'T2,'T3,'T4,'T5> =
    | Choice1Of5 of 'T1
    | Choice2Of5 of 'T2
    | Choice3Of5 of 'T3
    | Choice4Of5 of 'T4
    | Choice5Of5 of 'T5

[<Proxy(typeof<Choice<_,_,_,_,_,_>>)>]
type private ChoiceProxy<'T1,'T2,'T3,'T4,'T5,'T6> =
    | Choice1Of6 of 'T1
    | Choice2Of6 of 'T2
    | Choice3Of6 of 'T3
    | Choice4Of6 of 'T4
    | Choice5Of6 of 'T5
    | Choice6Of6 of 'T6

[<Proxy(typeof<Choice<_,_,_,_,_,_,_>>)>]
type private ChoiceProxy<'T1,'T2,'T3,'T4,'T5,'T6,'T7> =
    | Choice1Of7 of 'T1
    | Choice2Of7 of 'T2
    | Choice3Of7 of 'T3
    | Choice4Of7 of 'T4
    | Choice5Of7 of 'T5
    | Choice6Of7 of 'T6
    | Choice7Of7 of 'T7
