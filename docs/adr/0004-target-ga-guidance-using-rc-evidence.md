# Target GA guidance using RC evidence

Playbook's .NET 11 guidance targets the full release. Pingu chose to prepare it now from current release-candidate evidence, then reconcile final release notes and API behavior at GA and publish corrections or additions. This avoids maintaining a catalogue of temporary preview approaches. Actual verification records must still identify the RC or GA artifacts used; preparing GA-targeted guidance does not establish GA verification.

APIs that remain experimental after GA are included when they meet the authoring criterion. Pingu chose explicit experimental labels rather than exclusion, retaining the original Blazor AI requirement. Discovery summaries and individual recipes must disclose that status, with actual package versions, required opt-ins, and stability limitations in the detailed guidance. An experimental API is not described as a stable contract merely because its recipe targets .NET 11 GA.
