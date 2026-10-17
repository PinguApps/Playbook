# Playbook publishing schedule

Selected for the planned weekly release-draft publisher: **Sunday 18:17 UTC**, cron **`17 18 * * 0`**. This is 18:17 UK time in winter and 19:17 during British Summer Time. The initial release remains manual. No publishing workflow has been created or enabled during planning.

Pingu requested an offset from existing organization-wide publication, including packages outside the local reference list. The selected slot follows an authenticated, read-only GitHub audit on 15 October 2026.

## Coverage

- All 35 repositories in PinguApps: 16 public and 19 private, matching organization metadata. Other organizations and personal repositories were excluded.
- All 110 default-branch workflow YAML files, at recorded commits, plus 141 paginated Actions workflow records.
- 13 scheduled YAML workflows: ten NuGet publishers, one application release/deployment publisher, and two maintenance workflows in archived repositories.
- No truncated repository trees, missing repository permissions, or workflow-content read failures. Workflow records reported active; no disabled scheduled YAML reservation was found.
- None of the inspected schedules overrides the timezone. GitHub defaults to UTC and supports explicit timezone overrides. [GitHub schedule documentation](https://docs.github.com/en/actions/reference/workflows-and-actions/events-that-trigger-workflows#schedule).

## Existing publication reservations

Public NuGet publishers, all on Sunday UTC:

- **08:02**: [Dashboard Railway](https://github.com/PinguApps/Aspire.Hosting.Dashboard.Railway/blob/90527dd40b4c65377019a34caa99542cbecb2edb/.github/workflows/publish-release-draft.yml).
- **08:17**: [RabbitMQ Railway](https://github.com/PinguApps/Aspire.Hosting.RabbitMQ.Railway/blob/1c0587ced679df99637fd1145130a17348899f32/.github/workflows/publish-release-draft.yml).
- **08:32**: [Railway Jobs](https://github.com/PinguApps/Aspire.Hosting.Railway.Jobs/blob/411a479ef9c8344fff0b53b79c39a217d5f49ebd/.github/workflows/publish-release-draft.yml).
- **08:47**: [Railway](https://github.com/PinguApps/Aspire.Hosting.Railway/blob/884b822cc29308befc8a666e22de70d33082f35b/.github/workflows/publish-release-draft.yml).
- **09:02**: [RazorStyle](https://github.com/PinguApps/RazorStyle/blob/727b2c7a3b7cda01f471c9e5bb4674b55f415f1a/.github/workflows/publish-release-draft.yml).
- **09:17**: [Upstash Redis](https://github.com/PinguApps/Aspire.Hosting.Upstash.Redis/blob/b80c35bc1b67b49c8666b30a69c770c5ad3cd630/.github/workflows/publish-release-draft.yml).
- **09:32**: [PostgreSQL Railway](https://github.com/PinguApps/Aspire.Hosting.PostgreSQL.Railway/blob/b71dfe299bed40d59cab745e860bef35c43d27b5/.github/workflows/publish-release-draft.yml).
- **09:39**: [BlazorSitemap](https://github.com/PinguApps/BlazorSitemap/blob/fd3641d5cc70396759a4a708997581a0ee3c0948/.github/workflows/publish-release-draft.yml).
- **09:47**: [Bunny Storage](https://github.com/PinguApps/Aspire.Hosting.Bunny.Storage/blob/b16807203eafb715ab6cb48e66e062214549edf2/.github/workflows/publish-release-draft.yml).

Private reservations are included without committing unrelated repository names or workflow contents: a NuGet publisher at **Sunday 10:02 UTC** and an application release/deployment at **Sunday 09:32 UTC**. Archived maintenance schedules reserve **06:00 daily** and **00:27/12:27 daily**.

## Why the evening slot

The selected slot is **8 hours 15 minutes after the latest configured publisher**. Actual schedule-event runs were also inspected because matching cron minutes alone would miss the observed delays.

On 11 October 2026:

- Dashboard's 08:02 schedule started at 14:42:49 and completed at 14:44:03 UTC. [Run evidence](https://github.com/PinguApps/Aspire.Hosting.Dashboard.Railway/actions/runs/38148295842).
- Bunny's 09:47 schedule started at 15:58:26 and completed at 16:00:06 UTC. [Run evidence](https://github.com/PinguApps/Aspire.Hosting.Bunny.Storage/actions/runs/38153505048).
- The final private NuGet publisher's 10:02 schedule started at 16:06:08 and completed at **16:06:59 UTC**. Its identity and run evidence remain in the local audit.

Historical workflow YAML at the sampled run commits confirms those morning cron values. The proposed 18:17 slot is **2 hours 10 minutes after the latest observed completion**. In the bounded sample, the longest whole publishing workflow took 299 seconds; latest scheduled NuGet publishing jobs took at most 84 seconds. These measurements are evidence for separation, not a runner-cost optimization claim.

Future GitHub scheduling delays, manual publication, and configuration changes can still overlap. Recheck the organization schedules before enabling the new workflow.

## Evidence limits and provenance

Current default-branch workflow coverage is complete for the accessible organization inventory. Run history was bounded to the latest 20 schedule-event runs per repository, latest ten runs per publishing workflow, and job-level evidence for the latest scheduled publisher runs.

One empty repository returned HTTP 409 for its absent commit and had zero Actions workflows. Two private repositories returned HTTP 403 for managed CodeQL settings because Code Security is disabled; repository metadata independently confirmed that state, with no returned schedule-event history. A third managed CodeQL entry was not configured. These settings gaps do not hide an uninspected YAML publisher.

Raw repository responses, private evidence, commit IDs, historical workflow content, and audit scripts are retained outside this repository in `C:\Users\pingu\AppData\Local\Temp\pinguapps-schedule-audit-20261015-203124`. `public-summary.json` contains the aggregate findings. The audit made no remote changes.
