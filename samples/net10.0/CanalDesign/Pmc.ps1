# snippet:complete
# EF10 Visual Studio Package Manager Console has no NoBuild switch here.
Add-Migration OpenWestBasin -Project CanalDesign -StartupProject CanalDesign -Context CanalContext -OutputDir Migrations/Canal
Update-Database -Project CanalDesign -StartupProject CanalDesign -Context CanalContext
# endsnippet:complete
