# Bind JSON to constructors with reference parameters

## Goal

Restore a bin count whose existing constructor uses in/ref/ref readonly parameters, and distinguish an out parameter from an input-bound argument. Keep the intended model state when JSON is applied.

## Older approach

In net10.0 / C# 14 import `System.Text.Json` and `System.Text.Json.Serialization`, use namespace `Playbook.Samples` and standard implicit imports. A conventional value-parameter constructor is a supported JSON boundary. An adapter DTO with explicit mapping is another valid choice if the domain constructor cannot change.

{{snippet:older}}

## Newer approach

In net11.0 / C# 15 with those imports, the serializer binds in/ref/ref readonly by their element type and matching property name. Out does not consume a JSON constructor argument: the constructor gets an initialized output location, then a matching writable property can receive the JSON value.

{{snippet:newer}}

For example, `JsonSerializer.Deserialize("{\"Count\":19}", CountContext.Default.BinCount)` restores Count=19. With OutCount, `{}` retains the constructor's Count=4, whereas `{"Count":23}` ends with Count=23 through the writable property. Do not interpret the latter as out-argument input binding.

## Migration

1. Keep value parameters when they already express the domain correctly; no rewrite is required.
2. If existing reference parameters are intentional, mark the selected constructor and generate the exact closed model contracts.
3. Match input argument names/types to model members; reference modifiers do not remove ordinary constructor-binding rules.
4. Treat out values as constructor outputs; provide a writable property only if post-construction JSON assignment is intended.
5. Test generated and reflection paths separately if the application uses both.

## Constraints

Requires framework System.Text.Json on net11.0, SDK 11 / C# 15. No package, workload, provider, rendering mode or experimental opt-in. No older-target package, arbitrary ref-like-member, trimming or Native AOT route is claimed. These managed examples cover int-valued parameters with ordinary properties; they do not establish every overloaded/generic constructor shape. Immutable DTO mapping remains a valid boundary.

## Evidence

Actual previous/current examples compile and execute. RC2 verifies in/ref/ref readonly and out on both generated and reflection paths, including missing out input versus an explicit writable-property value. Saved input/toolchain records are distinct from source-reviewed general limitations. Installed MCP checks exact canonical code, full body, edition and applicability offline. No mocks, cross-compilation, GA or required Human proof.
