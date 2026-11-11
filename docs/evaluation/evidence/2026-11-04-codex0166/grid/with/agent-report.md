I’ll check the project and relevant Playbook guidance, then implement query-based links for both tables and run the available checks.

Playbook’s .NET 11 guidance applies here: each QuickGrid can restore its own URL state using distinct query names and stable column titles. I’ll use that support with explicit ascending and descending links that preserve the current page. The guidance was verified against RC2; I’ll check it against this project’s pinned RC3.

The RC3 build passes, and an HTTP request restores both tables independently on page two. The rendered links preserve `filter=open%20water`, the other table’s state, and the current page when sorting. I’m checking all sort/page combinations and following the rendered links next.

Implemented both tables with independent query state, two rows per page, and ordinary sorting/paging links. Sorting preserves the page, other table, and filter spaces. Route, data, IDs, and all pins remain unchanged; no JavaScript, interactivity, or services added.

Consulted applicable Playbook .NET 11 guidance.

Checks passed: Release build via `dotnet msbuild`; 172 HTTP row assertions covering 64 combined views, 16 rendered links, filters, and saved addresses. `dotnet build` was blocked by an SDK network request.

Nothing remains to implement. Actual browser back/forward navigation was not tested.
