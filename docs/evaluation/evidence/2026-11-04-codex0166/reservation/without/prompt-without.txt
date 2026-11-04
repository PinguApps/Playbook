# Reserve a reed-bed observation hide

Reject invalid reservations before the booking handler does any work. A code is
required and the party size must be between one and four. Availability requires
waiting for Availability.IsAvailableAsync; declare this rule alongside the model's
other validation rules. Return HTTP 400 with errors keyed by Code or Guests for
invalid requests, and 204 for a valid reservation. A closed code is unavailable.

While availability is pending, the request must remain unfinished and the handler
must not run. Propagate request cancellation to the availability check; canceled
checks must never book. Preserve the route, model properties, service and control
instrumentation. Control routes hold/release checks so you can exercise ordering.
This is validation, not an atomic capacity-allocation system.

Keep the pinned SDK/framework/language settings. No extra packages or services are
needed. Any solution meeting these behaviors is acceptable. You have ten minutes
including checks. Report what ran and anything unfinished; no corrective follow-ups.
