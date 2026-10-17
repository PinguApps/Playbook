# One Playbook across .NET releases

Use `PinguApps.Playbook` for the project, repository, and NuGet package, with `playbook` as the executable command. Pingu explicitly chose to expand the same package each year instead of publishing separate year-specific packages and commands. This keeps the product identity stable and avoids installing different global tools that compete for the same command name. Preserve release-specific guidance as the package grows; target selection defaults and the support lifetime remain separate planning decisions.
