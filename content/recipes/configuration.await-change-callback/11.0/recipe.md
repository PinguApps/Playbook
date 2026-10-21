# Complete asynchronous reload work before rearming notifications

## Goal

Refresh a harbour catalogue after configuration changes without overlapping refreshes, and report failures where the refresh runs.

## Older approach

A .NET 10 change callback can signal an application-owned bounded channel. One worker awaits each refresh and reports its errors. This is valid when the application needs an explicit task to join during shutdown. The single pending slot intentionally coalesces changes; it is not a reliable event log.

{{snippet:older}}

## Newer approach

.NET 11 provides Func<Task> overloads of ChangeToken.OnChange, including stateful callbacks. The registration rearms after the returned task completes. The callback must handle its own asynchronous failures; awaiting internally does not return a completion task to the subscriber.

{{snippet:newer}}

## Migration

1. Use a net11.0 console project, C# 15.0, Nullable/ImplicitUsings enabled and a FrameworkReference to Microsoft.AspNetCore.App. Import Microsoft.Extensions.Primitives; the older channel also imports System.Threading.Channels. Put the class in your application namespace.
2. Pass a token producer that obtains the latest token each time, such as configuration.GetReloadToken, and a refresh function returning its complete Task. Retain the subscription for its intended lifetime.
3. Supply a nonthrowing error reporter. Catch refresh failures inside the callback; decide whether to log, retry separately or stop the application. Do not rely on a global unobserved-task event for normal error reporting.
4. Audit existing async lambdas when recompiling: overload resolution may now choose Func<Task> instead of Action/async void. Use an explicit Action only when deliberately retaining that older contract.
5. Dispose the registration when notifications should stop. If shutdown must join or cancel active refresh work, own that task/cancellation protocol explicitly, as the older worker does.

## Constraints

Notifications during an active refresh coalesce into a subsequent refresh; intermediate changes are not all delivered. Synchronous exceptions thrown before a delegate returns its Task propagate to the trigger. Faults after asynchronous suspension are not propagated to the trigger and are left unobserved unless handled in the callback. Rearm still occurs in finally. Dispose unregisters; it neither awaits nor cancels in-flight work. The newer wrapper deliberately makes no shutdown-join promise. The reporter itself must not throw.

This is the consumer lifetime contract, separate from whether a file provider successfully loaded new data. It establishes no last-known-good configuration policy and no durable queue. No browser or filesystem watcher execution is claimed.

## Evidence

Actual .NET 10/11 RC2 executions use cancellation change tokens, controlled task barriers and bounded waits. They prove no overlap, coalescing, caught asynchronous failure and a later successful refresh. .NET 11 additionally checks synchronous exception propagation and completion after disposal. The discarded framework task's unobserved-fault policy is pinned source evidence, not a nondeterministic GC/event test. Exact builds, output and input hashes are saved; GA remains unverified.
