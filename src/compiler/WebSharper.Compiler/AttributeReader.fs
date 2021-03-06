﻿// $begin{copyright}
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

/// Shared logic for interpreting WebSharper-specific attibutes
/// from C# or F# source code or by reflection
module WebSharper.Compiler.AttributeReader

open WebSharper.Core
open WebSharper.Core.AST

module M = WebSharper.Core.Metadata

/// Parsed data from a single attribute
[<RequireQualifiedAccess>]
type private Attribute =
    | Macro of TypeDefinition * option<obj>
    | Proxy of TypeDefinition
    | Inline of option<string>
    | Direct of string
    | Pure
    | Constant of Literal
    | Generated of TypeDefinition * option<obj>
    | Require of TypeDefinition * option<obj>
    | Name of string
    | Stub
    | OptionalField
    | JavaScript of bool
    | Remote
    | RemotingProvider of TypeDefinition * option<obj>
    | NamedUnionCases of option<string>
    | DateTimeFormat of option<string> * string
    | Website of TypeDefinition
    | SPAEntryPoint
    
type private A = Attribute

/// Contains information from all WebSharper-specific attributes for a type
type TypeAnnotation = 
    {
        ProxyOf : option<TypeDefinition>
        IsJavaScript : bool
        IsStub : bool
        OptionalFields : bool
        Name : option<string>
        Requires : list<TypeDefinition * option<obj>>
        NamedUnionCases : option<option<string>>
        Macros : list<TypeDefinition * option<obj>>
        RemotingProvider : option<TypeDefinition * option<obj>>
    }

    static member Empty =
        {
            ProxyOf = None
            IsJavaScript = false
            IsStub = false
            OptionalFields = false
            Name = None
            Requires = []
            NamedUnionCases = None
            Macros = []
            RemotingProvider = None
        }

type MemberKind = 
    | Inline of string
    | Direct of string
    | InlineJavaScript
    | JavaScript
    | Constant of Literal
    | NoFallback
    | Generated of TypeDefinition * option<obj>
    | Remote of option<TypeDefinition * option<obj>>
    | Stub
    | OptionalField
    | AttributeConflict of string

/// Contains information from all WebSharper-specific attributes for a member
type MemberAnnotation =
    {
        Kind : option<MemberKind>
        Macros : list<TypeDefinition * option<obj>> 
        Name : option<string>
        Requires : list<TypeDefinition * option<obj>>
        IsEntryPoint : bool
        DateTimeFormat : list<option<string> * string>
        Pure : bool
    }

    static member BasicJavaScript =
        {
            Kind = Some JavaScript
            Macros = []
            Name = None
            Requires = []
            IsEntryPoint = false
            DateTimeFormat = []
            Pure = false
        }

    static member BasicInlineJavaScript =
        {
            Kind = Some InlineJavaScript
            Macros = []
            Name = None
            Requires = []
            IsEntryPoint = false
            DateTimeFormat = []
            Pure = false
        }

/// Contains information from all WebSharper-specific attributes for an assembly
type AssemblyAnnotation =
    {
        SiteletDefinition : option<TypeDefinition>
        Requires : list<TypeDefinition * option<obj>>
        RemotingProvider : option<TypeDefinition * option<obj>>
        IsJavaScript : bool
    }

    member this.RootTypeAnnot =
        { TypeAnnotation.Empty with
            RemotingProvider = this.RemotingProvider
            IsJavaScript = this.IsJavaScript
        }

/// Base class for reading WebSharper-specific attributes.
[<AbstractClass>]
type AttributeReader<'A>() =


    abstract GetAssemblyName : 'A -> string
    abstract GetName : 'A -> string
    abstract GetCtorArgs : 'A -> obj[]
    abstract GetTypeDef : obj -> TypeDefinition

    member private this.ReadTypeArg(attr: 'A) =
        let args = this.GetCtorArgs(attr)
        let def =
            match args.[0] with
            | :? string as s ->
                let ss = s.Split([|','|])
                Hashed {
                    FullName = ss.[0].Trim()
                    Assembly = ss.[1].Trim()
                }
            | t -> 
                try this.GetTypeDef t
                with _ -> failwith "Failed to parse type argument of attribute."
        let param =
            if args.Length = 2 then
                Some args.[1]
            else None
        def, param

    member private this.Read (attr: 'A) =
        match this.GetName(attr) with
        | "ProxyAttribute" ->
            A.Proxy (this.ReadTypeArg attr |> fst)
        | "InlineAttribute" ->
            A.Inline (Seq.tryHead (this.GetCtorArgs(attr)) |> Option.map unbox)
        | "DirectAttribute" ->
            A.Direct (Seq.head (this.GetCtorArgs(attr)) |> unbox)
        | "PureAttribute" ->
            A.Pure
        | "ConstantAttribute" ->
            A.Constant (Seq.head (this.GetCtorArgs(attr)) |> ReadLiteral)
        | "MacroAttribute" ->
            A.Macro (this.ReadTypeArg attr)
        | "GeneratedAttribute" ->
            A.Generated (this.ReadTypeArg attr)
        | "RemoteAttribute" ->
            A.Remote
        | "RequireAttribute" ->
            A.Require (this.ReadTypeArg attr)
        | "StubAttribute" ->
            A.Stub
        | "NameAttribute" ->
            match Seq.head (this.GetCtorArgs(attr)) with
            | :? string as n -> A.Name n
            | :? System.Array as p -> A.Name ("." + String.concat "." (Seq.cast<string> p))                       
            | :? int as i -> A.Name (string i)
            | x -> failwithf "Unrecognized constructor parameter type for Name attribute: %s" (x.GetType().FullName)
        | "JavaScriptAttribute" ->
            A.JavaScript (Seq.tryHead (this.GetCtorArgs(attr)) |> Option.forall unbox)
        | "OptionalFieldAttribute" ->
            A.OptionalField
        | "RemotingProviderAttribute" ->
            A.RemotingProvider (this.ReadTypeArg attr)
        | "NamedUnionCasesAttribute" ->
            A.NamedUnionCases (Seq.tryHead (this.GetCtorArgs(attr)) |> Option.map unbox)
        | "DateTimeFormatAttribute" ->
            match this.GetCtorArgs(attr) with
            | [| f |] -> A.DateTimeFormat (None, unbox f)
            | [| a; f |] -> A.DateTimeFormat (Some (unbox a), unbox f)
            | _ -> failwith "invalid constructor arguments for DateTimeFormatAttribute"
        | "WebsiteAttribute" ->
            A.Website (this.ReadTypeArg attr |> fst)
        | "SPAEntryPointAttribute" ->
            A.SPAEntryPoint
        | n -> 
            failwithf "Unknown attribute type: %s" n

    member private this.GetAttrs (parent: TypeAnnotation, attrs: seq<'A>) =
        let attrArr = ResizeArray()
        let mutable name = None
        let reqs = ResizeArray()
        let macros = ResizeArray() 
        let mutable js = None
        let mutable stub = false
        let mutable proxy = None
        for a in attrs do
            match this.GetAssemblyName a with
            | "WebSharper.Core" ->
                match this.Read a with
                | A.Name n -> name <- Some n
                | A.Require (t, p) -> reqs.Add (t, p)
                | A.Macro (m, p) -> macros.Add (m, p)
                | A.JavaScript j -> js <- Some j
                | A.Stub -> stub <- true
                | A.Proxy t -> proxy <- Some t
                | ar -> attrArr.Add ar
            | _ -> ()
        if Option.isNone js && not stub && Option.isSome proxy then 
            js <- Some true
        let isJavaScript, isStub =
            if parent.IsJavaScript then
                match js, stub with
                | (None | Some false), true -> false, true
                | Some false, false -> false, false
                | Some true, _ -> true, false
                | _ -> macros.Count = 0, false
            elif parent.IsStub then
                match js, stub with
                | Some false, false -> false, false
                | Some true, _ -> true, false
                | _ -> false, macros.Count = 0
            else 
                match js, stub with
                | (None | Some false), true -> false, true
                | Some true, _ -> true, false
                | _ -> false, false

        if parent.OptionalFields then
            if not (attrArr.Contains(A.OptionalField)) then attrArr.Add A.OptionalField
        attrArr |> Seq.distinct |> Seq.toArray, macros.ToArray(), name, proxy, isJavaScript, isStub, List.ofSeq reqs

    member this.GetTypeAnnot (parent: TypeAnnotation, attrs: seq<'A>) =
        let attrArr, macros, name, proxyOf, isJavaScript, isStub, reqs = this.GetAttrs (parent, attrs)
        {
            ProxyOf = proxyOf
            IsJavaScript = isJavaScript
            IsStub = isStub
            OptionalFields = attrArr |> Array.exists (function A.OptionalField -> true | _ -> false)
            Name = name
            Requires = reqs
            NamedUnionCases = attrArr |> Array.tryPick (function A.NamedUnionCases uc -> Some uc | _ -> None)
            Macros = macros |> List.ofArray
            RemotingProvider = 
                attrArr |> Array.tryPick (function A.RemotingProvider (r, p) -> Some (r, p) | _ -> None) 
                |> function Some x -> Some x | None -> parent.RemotingProvider
        }

    member this.GetMemberAnnot (parent: TypeAnnotation, attrs: seq<'A>) =
        let attrArr, macros, name, _, isJavaScript, isStub, reqs = this.GetAttrs (parent, attrs)
        let isEp = attrArr |> Array.contains A.SPAEntryPoint
        let isPure = attrArr |> Array.contains A.Pure
        let rp = 
            attrArr |> Array.tryPick (function A.RemotingProvider (r, p) -> Some (r, p) | _ -> None) 
            |> function Some x -> Some x | None -> parent.RemotingProvider
        let attrArr = 
            attrArr |> Array.filter (function 
                | A.SPAEntryPoint | A.Pure | A.DateTimeFormat _ | A.RemotingProvider _ -> false 
                | _ -> true)
        let kind =
            match attrArr with
            | [||] -> 
                if isJavaScript then Some JavaScript 
                elif isStub then Some Stub 
                elif macros.Length = 0 then None else Some NoFallback
            | [| A.Remote |] -> Some (Remote rp)
            | [| a |] ->
                match a with   
                | A.Inline None -> Some InlineJavaScript
                | A.Inline (Some i) -> Some (Inline i)
                | A.Direct s -> Some (Direct s)
                | A.Constant x -> Some (Constant x)
                | A.Generated (g, p) -> Some (Generated (g, p))
                | A.OptionalField -> Some OptionalField
                | _ -> Some (AttributeConflict (sprintf "Unexpected attribute: %s" (GetUnionCaseName a)))
            | _ -> Some (AttributeConflict (sprintf "Incompatible attributes: %s" (attrArr |> Seq.map GetUnionCaseName |> String.concat ", ")))  
        {
            Kind = kind
            Macros = List.ofArray macros
            Name = name
            Requires = reqs
            IsEntryPoint = isEp
            DateTimeFormat = attrArr |> Seq.choose (function A.DateTimeFormat (a,b) -> Some (a,b) | _ -> None) |> List.ofSeq
            Pure = isPure
        }
   
    member this.GetAssemblyAnnot (attrs: seq<'A>) =
        let reqs = ResizeArray()
        let mutable sitelet = None
        let mutable remotingProvider = None
        let mutable isJavaScript = false
        for a in attrs do
            match this.GetAssemblyName a with
            | "WebSharper.Core"
            | "WebSharper.Sitelets" ->
                match this.Read a with
                | A.Require (t, p) -> reqs.Add (t, p)
                | A.Website t -> sitelet <- Some t
                | A.RemotingProvider (t, p) -> remotingProvider <- Some (t, p)
                | A.JavaScript true -> isJavaScript <- true
                | _ -> ()
            | _ -> ()
             
        {
            SiteletDefinition = sitelet
            Requires = reqs |> List.ofSeq
            RemotingProvider = remotingProvider
            IsJavaScript = isJavaScript
        }        
           
type ReflectionAttributeReader() =
    inherit AttributeReader<System.Reflection.CustomAttributeData>()
    override this.GetAssemblyName attr = attr.Constructor.DeclaringType.Assembly.FullName.Split(',').[0]
    override this.GetName attr = attr.Constructor.DeclaringType.Name
    override this.GetCtorArgs attr = attr.ConstructorArguments |> Seq.map (fun a -> a.Value) |> Array.ofSeq
    override this.GetTypeDef o = Reflection.ReadTypeDefinition (o :?> System.Type) 

let attrReader = ReflectionAttributeReader()

type FST = FSharp.Reflection.FSharpType

let private mdelTy = typeof<System.MulticastDelegate>
let reflectCustomType (typ : TypeDefinition) =
    try
        let t = Reflection.LoadTypeDefinition typ
        if t.BaseType = mdelTy then
            let inv = t.GetMethod("Invoke") |> Reflection.ReadMethod |> Hashed.Get
            M.DelegateInfo {
                DelegateArgs = inv.Parameters 
                ReturnType = inv.ReturnType
            } 
        elif t.IsEnum then
            M.EnumInfo (Reflection.ReadTypeDefinition (t.GetEnumUnderlyingType()))
        elif FST.IsRecord(t, Reflection.AllMethodsFlags) then
            let tAnnot = attrReader.GetTypeAnnot(TypeAnnotation.Empty, t.GetCustomAttributesData())
        
            FST.GetRecordFields(t, Reflection.AllMethodsFlags)
            |> Seq.map (fun f ->
                let annot = attrReader.GetMemberAnnot(tAnnot, f.GetCustomAttributesData()) 
                let isOpt = 
                    annot.Kind = Some MemberKind.OptionalField 
                    && f.PropertyType.IsGenericType 
                    && f.PropertyType.GetGenericTypeDefinition() = typedefof<option<_>>
                {
                    Name = f.Name
                    JSName = match annot.Name with Some n -> n | _ -> f.Name
                    RecordFieldType = Reflection.ReadType f.PropertyType
                    DateTimeFormat = annot.DateTimeFormat |> List.tryHead |> Option.map snd
                    Optional = isOpt
                } : M.FSharpRecordFieldInfo
            )
            |> List.ofSeq |> M.FSharpRecordInfo
        elif FST.IsUnion(t, Reflection.AllMethodsFlags) then
            let tAnnot = attrReader.GetTypeAnnot(TypeAnnotation.Empty, t.GetCustomAttributesData())
            let usesNull = 
                t.GetCustomAttributesData()
                |> Seq.exists (fun a ->
                    a.Constructor.DeclaringType = typeof<CompilationRepresentationAttribute>
                    && obj.Equals(a.ConstructorArguments.[0].Value, CompilationRepresentationFlags.UseNullAsTrueValue)
                )
                && (FST.GetUnionCases(t, Reflection.AllMethodsFlags)).Length < 4
            let cases =
                FST.GetUnionCases(t, Reflection.AllMethodsFlags)
                |> Seq.map (fun c ->
                    let annot = attrReader.GetMemberAnnot(tAnnot, c.GetCustomAttributesData()) 
                    let caseInfo =
                        match annot.Kind with
                        | Some (MemberKind.Constant v) -> M.ConstantFSharpUnionCase v
                        | _ ->
                            c.GetFields()
                            |> Array.map (fun f ->
                                let fName = f.Name
                                {
                                    Name = fName
                                    UnionFieldType = Reflection.ReadType f.PropertyType
                                    DateTimeFormat =
                                        annot.DateTimeFormat |> List.tryFind (fun (n, _) -> n = Some fName) |> Option.map snd 
                                } : M.UnionCaseFieldInfo 
                            )
                            |> List.ofArray |> M.NormalFSharpUnionCase  
                    let isStatic =
                        not usesNull || not (
                            c.GetCustomAttributesData()
                            |> Seq.exists (fun a ->
                                a.Constructor.DeclaringType = typeof<CompilationRepresentationAttribute>
                                && obj.Equals(a.ConstructorArguments.[0].Value, CompilationRepresentationFlags.Instance)
                            )
                        )
                    {
                        Name = c.Name
                        JsonName = annot.Name
                        Kind = caseInfo
                        StaticIs = isStatic
                    } : M.FSharpUnionCaseInfo
                )
                |> List.ofSeq
            M.FSharpUnionInfo {
                Cases = cases
                NamedUnionCases = tAnnot.NamedUnionCases
                HasNull = usesNull && cases |> List.exists (fun c -> c.Kind = M.ConstantFSharpUnionCase Null) 
            }
        else M.NotCustomType
    with _ -> M.NotCustomType