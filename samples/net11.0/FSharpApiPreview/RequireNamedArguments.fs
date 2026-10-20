// snippet:attribute
namespace System.Diagnostics.CodeAnalysis

open System

// Supplied by this sample: the RC2 BCL does not ship this attribute.
[<AttributeUsage(AttributeTargets.Method ||| AttributeTargets.Constructor, AllowMultiple = false)>]
type RequireNamedArgumentsAttribute() =
    inherit Attribute()
// endsnippet:attribute
