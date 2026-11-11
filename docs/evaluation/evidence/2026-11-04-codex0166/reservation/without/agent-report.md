I’ll inspect the endpoint and validation setup, add the awaited availability rule, then check response codes, ordering, and cancellation.

The endpoint currently has no validation step. I’ll add an endpoint filter that awaits a validation method on `Reservation`, keeping availability alongside the required-code and guest-count rules. Invalid fields will return immediately; valid fields will wait for availability using the request cancellation token.

The code builds with the pinned SDK. `dotnet build` was blocked by an SDK network check, so I used the same SDK’s MSBuild directly. The app is running locally; I’m checking invalid inputs, held availability checks, closed codes, and cancellation through the control routes.

Implemented model-level async validation, awaited by an endpoint filter before the booking handler runs.

Verified:
- Invalid fields return 400 with `Code`/`Guests` errors.
- Pending checks keep requests unfinished and prevent booking.
- Released valid checks return 204; closed codes return 400.
- Cancellation reaches availability and prevents booking after release.

Pinned SDK/framework/language settings, service, and control routes remain unchanged.

Build passed using the pinned SDK’s MSBuild after `dotnet build` hit a blocked network check. All live checks in `checks.ps1` passed. Nothing unfinished.
