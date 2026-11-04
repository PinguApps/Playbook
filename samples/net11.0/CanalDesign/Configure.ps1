# snippet:complete
# Run from the CanalDesign project directory, with the matching local EF11 tool.
New-Item -ItemType Directory .config -Force | Out-Null
@'
{
  "project": ".",
  "startupProject": ".",
  "context": "CanalContext",
  "configuration": "Release",
  "framework": "net11.0"
}
'@ | Set-Content -LiteralPath .config/dotnet-ef.json
# From any descendant, defaults select CanalContext. CLI selection wins:
# dotnet ef dbcontext info --context ArchiveContext
# endsnippet:complete
