# Await data validation before accepting a model

## Goal

A fictional reef survey station accepts a docking request only after its permit ledger answers. Keep ordinary required-field checks, await the decision, and propagate cancellation or service failure instead of treating unfinished work as valid.

## Older approach

.NET 10 DataAnnotations validates synchronously. An application-owned asynchronous coordinator can first collect annotation errors and then await a service rule. This is a valid explicit boundary, not a synchronous network call hidden inside an attribute.

{{snippet:older}}

## Newer approach

.NET 11 supplies AsyncValidationAttribute, IAsyncValidatableObject and asynchronous Validator entry points. The attribute owns the reusable permit rule; the caller must actually await the asynchronous validation path. Required handles missing input. The synchronous override deliberately rejects a path that cannot perform the required check.

{{snippet:newer}}

This complete in-process ledger makes the example executable without an account. The host completes Decision with the asynchronously obtained verdict; a real remote implementation replaces that mechanism and preserves its token.

{{snippet:ledger}}

For a whole-model asynchronous rule, implement both inherited synchronous and asynchronous contracts. This independent example reports an object-level error and demonstrates an asynchronous iterator; it does not pretend a synchronous fallback can perform I/O.

{{snippet:object}}

## Migration

1. Use a net11.0 console project with C# 15.0, nullable and implicit usings enabled. Import System.ComponentModel.DataAnnotations, System.Globalization and System.Runtime.CompilerServices for the examples. No external package is required.
2. Add the model, attribute and service. Supply the service through ValidationContext's IServiceProvider, as DockValidation does. Do not instantiate a second unrelated service inside the attribute.
3. Await DockValidation.CheckAsync with the operation's cancellation token. Accept only an empty returned error list. Display each error's ErrorMessage and MemberNames. An exception or canceled task is a failed operation, not a successful validation result.
4. Use ValidateObjectAsync for a throwing object boundary or TryValidateObjectAsync to collect errors. Pass validateAllProperties: true for all property annotations. Property/value-only boundaries use ValidatePropertyAsync/TryValidatePropertyAsync or ValidateValueAsync/TryValidateValueAsync with the correct member context and explicit attribute list.
5. Keep the caller's culture configured across the awaited operation. Verify acceptance, rejection, pending completion, cancellation and service failure before connecting any web adapter.

## Constraints

The six async Validator families are exercised against the actual RC2 artifacts. Object validation does not recursively validate an arbitrary object graph. Property errors can prevent later object-level validation; do not depend on a particular rule order or use validation as an ordered side-effect pipeline. Concurrent rules must tolerate shared-service concurrency. Cancellation reaches the rule, but the framework can use a linked token: token identity is not guaranteed.

The ledger is an asynchronous in-process policy, not a database or external-registry integration. Required is still synchronous; an async-only rule cannot silently pass a synchronous caller. Do not use Result, Wait or GetAwaiter().GetResult to bridge the contracts.

These are shared DataAnnotations APIs. They do not register Minimal API validation, discover Blazor form models or prove Microsoft.Extensions.Validation generator/localizer integration. Those adapters need their own registration, metadata and request/form evidence. Reflection-based DataAnnotations is not trimming/Native AOT proof. No older-target/newer-package route is advertised.

## Evidence

Actual .NET 10 and .NET 11 projects compile and execute the same goal. Task barriers prove validation remains pending until a verdict; real cancellation reaches the service; rejection retains the Permit member; Required and IOException propagation remain observable. en-GB/fr-FR culture flows through completion. RC2 additionally runs all six Validator families, rejects the synchronous path, and executes IAsyncValidatableObject without calling Validate. Exact SDKs, dependencies, outputs and input hashes accompany the recipe. RC2 evidence is not GA or web integration evidence.
