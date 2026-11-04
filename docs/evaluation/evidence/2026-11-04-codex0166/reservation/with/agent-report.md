I’ll inspect the project and consult Playbook for guidance that matches its target framework, then implement and check the validation flow within the time limit.

Playbook’s .NET 11 async validation guidance matches this project. I’ll add an availability attribute beside `[Required]` and register validation so the framework awaits the check before entering the handler. Then I’ll use the control routes to verify pending requests, rejection, success, and cancellation.

The validation change is in place, but the build command is failing because the sandbox blocks access to `builds.dotnet.microsoft.com`. The pinned SDK is installed; I’m checking whether its local build tools can run without that network request so I can complete the HTTP checks.

The build passed using the pinned SDK’s local MSBuild. HTTP checks also passed: invalid fields return keyed 400 errors; held checks leave requests unfinished with zero bookings; release produces 204 for valid requests and 400 for closed codes; cancellation reaches availability and prevents booking even after release.

Implemented model-declared async availability validation using matching .NET 11 Playbook guidance.

Verified:
- Invalid requests return 400 with Code/Guests errors.
- Pending checks keep requests unfinished and handlers idle.
- Valid reservations return 204; closed codes return 400.
- Cancellation reaches availability and prevents booking, including after release.

Pinned SDK MSBuild passed with zero warnings/errors; HTTP checks passed. Framework/language settings, service, and control routes are preserved.

Nothing functional remains unfinished. Temporary log cleanup was blocked by filesystem permissions.
