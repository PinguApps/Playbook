# Canonical public JSON files

wwwroot/appsettings.json:

<!-- snippet:base-json -->
{"Board":{"Endpoint":"Base JSON endpoint","BaseOnly":"Base JSON","Caption":"Base JSON caption"}}
<!-- endsnippet:base-json -->

wwwroot/appsettings.Testing.json:

<!-- snippet:testing-json -->
{"Board":{"Endpoint":"Testing JSON endpoint","Caption":"Testing JSON caption","JsonOnly":"Testing JSON"}}
<!-- endsnippet:testing-json -->

wwwroot/appsettings.Production.json:

<!-- snippet:production-json -->
{"Board":{"Endpoint":"Production JSON endpoint","Caption":"Production JSON caption","JsonOnly":"Production JSON"}}
<!-- endsnippet:production-json -->

environments/Testing.json:

<!-- snippet:testing-public -->
{"environment":"Testing","variables":{"Board__Caption":"Testing public caption","Board__Endpoint":"https://testing.estuary.invalid/dispatch"}}
<!-- endsnippet:testing-public -->

environments/Production.json:

<!-- snippet:production-public -->
{"environment":"Production","variables":{"Board__Caption":"Production public caption","Board__Endpoint":"https://production.estuary.invalid/dispatch"}}
<!-- endsnippet:production-public -->
