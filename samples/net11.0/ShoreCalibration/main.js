// snippet:startup
import { dotnet } from './_framework/dotnet.js';

await dotnet.withDiagnosticTracing(false).create();
await dotnet.run();
// endsnippet:startup
