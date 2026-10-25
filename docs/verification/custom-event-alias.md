# Custom event alias evidence

Historical authoring observations from October 2026. Compilation stays in the
sample records; these observations do not establish a new run, broader platform
support or GA verification. Reuse only while the claim and relevant inputs remain
valid; see [capture and refresh](../contributing.md#capture-and-refresh-sample-evidence).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-799.md).

## Actual consumer proof

| Consumer | SDK / language | Executed runtime / implicit asset | Observed behavior |
| --- | --- | --- | --- |
| Canonical .NET 10 | 10.0.402 / C# 14 | 10.0.13 | One trusted native beforeinput, two component deliveries |
| Canonical .NET 11 | 11.0.100-rc.2.26504.105 / C# 15 | 11.0.0-rc.2.26504.105 | Same-name registration throws; cairnedit delivers once |
| Returned-Markdown .NET 10 | Same independent old pins | 10.0.13 | Same actual old dispatch and meaningful payload |
| Returned-Markdown .NET 11 | Same independent new pins | 11.0.0-rc.2.26504.105 | Same new failure control and repaired dispatch |

Each real Chrome keypress generated exactly one trusted native `beforeinput`
with `data=a` and `inputType=insertText`. Actual server component output was
`North cairn | insertText | a`; counts were 2 on the old comparison and 1 on the
new pair. This bounded observation is not an exactly-once delivery guarantee.
A distinct alias was already valid on .NET 10.

The new-framework control calls the actual loaded Blazor registration API with
`beforeinput` as both alias and browserEventName. It throws the descriptive
same-name error. Both versions reject `click` with the older already-registered
error; that control alone would not establish a framework change. No mocked
DOM, HtmlRenderer, synthetic dispatched event or source-only execution claim is
used. Registration rejection is exercised directly after startup, rather than
leaving the repaired application in a broken-initializer state.

The actual served framework files were:

- .NET 10: 200,645 bytes,
  `60086d7d0e7a848a032ea979f60163379f7528d3775151b92c1641502641b056`.
- .NET 11: 245,494 bytes,
  `065eac53496a10915dc8c3e19894d6bbb673c804c4f7ae7573a8364cc0e060c7`.

The newer bytes contain the separate same-name guard; the older bytes do not.
The pinned official
[old source](https://github.com/dotnet/aspnetcore/blob/v10.0.13/src/Components/Web.JS/src/Rendering/Events/EventTypes.ts)
and
[new source](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Web.JS/src/Rendering/Events/EventTypes.ts)
were reviewed separately from execution. The official
[initializer contract](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/startup?view=aspnetcore-11.0)
establishes that afterWebStarted receives Blazor and beforeWebStart receives
options. The inventory's compatibility-guide URL was inaccessible; it is not
claimed as read.

## Limits and review scope

This is stable API guidance executed with RC2, not GA validation. It covers one
Windows Chrome and Web App Interactive Server with prerendering. Static SSR
alone cannot dispatch .NET events. Other browsers/OS, WebAssembly/Hybrid, AOT,
deployment and older-target/new-assets routes remain unverified. No external
account, required Human proof, provider or workload was needed or provisioned.
The author's installed workload inventory is retained; workloads:[] means no
required workload, not an empty installed inventory.

PR Agent exclusions were assessed: the bounded evidence records, source,
configuration and tests remain reviewable. No new exclusion is justified.
The existing generated audit-asset exclusions are unchanged.
