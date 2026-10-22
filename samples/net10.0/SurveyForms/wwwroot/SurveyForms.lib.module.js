// snippet:client
// An application-owned .NET 10 rule must be kept aligned with the model's Required.
export function afterWebStarted() {
  const check = input => {
    const message = input.value.trim() ? '' : input.dataset.legacyRequired;
    document.querySelector(`[data-legacy-error="${input.id}"]`).textContent = message;
    return !message;
  };
  document.addEventListener('focusout', event => {
    if (event.target.matches('[data-legacy-required]')) check(event.target);
  });
  document.addEventListener('submit', event => {
    if (event.submitter?.formNoValidate) return;
    const valid = [...event.target.querySelectorAll('[data-legacy-required]')]
      .map(check).every(Boolean);
    if (!valid) { event.preventDefault(); event.stopImmediatePropagation(); }
  }, true);
}
// endsnippet:client
