# Grid row actions evidence

Historical October 2026 authoring observations. Follow the [evidence reuse policy](../contributing.md#choose-the-check-and-retain-its-result).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-782.md).

## Evidence and dependency boundary

- Actual SDK 10.0.402/C# 14 and SDK 11.0.100-rc.2.26504.105/C# 15 compile the two
  Web SDK projects with locked QuickGrid 10.0.13 / 11.0.0-rc.2.26504.105 and
  SDK-aligned Internal.Assets. No optional workloads or database provider.
- Samples inherit only samples/Directory.Build.props and the samples package
  boundary. Local explicit package pins disable central version management for
  these consumers. Root production/test targets remain net10.0 and unchanged.
  Existing Renovate policy protects all sample locations; no permission change
  is required. Both dependency-policy checks pass (328 extracted dependencies,
  285 protected sample entries).
- [Real browser report](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-782.browser.evidence.json) records selected IDs,
  descriptions and callback counts with source/configuration and artifact hashes.
  Newer text-cell clicks select 42 then 17; Enter on Open Signal lantern selects
  42 and increments exactly once. Older text cells do nothing; its button actions
  select 42 then 17. Static pages on both targets remain selected=0/opens=0 after
  row/button clicks. This is actual Chrome/Interactive Server execution on
  Windows x64, separate from saved **compile** evidence. T3 preview snapshots
  timed out twice; the existing Chrome extension provided the execution fallback.
- Recorded sample compilation/execution starts from fe9e7f0 with sourceDirty=true;
  this is authoring proof, not a clean reviewed-head rebuild. The normal freshness
  gate binds exact inputs independently of the commit marker. No GA, assistive
  technology, WebAssembly/Auto, other-OS, AOT or real database claim is made.
