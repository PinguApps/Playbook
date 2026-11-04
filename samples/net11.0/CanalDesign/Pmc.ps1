# snippet:complete
# Visual Studio Package Manager Console, CanalDesign loaded in the solution.
# Select Release in Visual Studio, save all files, restore and Build Solution.
# Verify both target and startup projects are the freshly built CanalDesign.
Add-Migration OpenWestBasin -Project CanalDesign -StartupProject CanalDesign -Context CanalContext -OutputDir Migrations/Canal -NoBuild
# After scaffolding, rebuild: the new migration is not in the previous assembly.
# Only then, against this sample's disposable local SQLite database:
Update-Database -Project CanalDesign -StartupProject CanalDesign -Context CanalContext -NoBuild
# endsnippet:complete
