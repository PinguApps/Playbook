// snippet:client
// Application-owned .NET 10 duplication: keep shape and error text aligned with C#.
export function afterWebStarted() {
  const check = form => {
    const input = form.querySelector('#code');
    if (!input) return true;
    const message = !input.value.trim() ? 'Enter an accession code.'
      : input.value.length == 7 && /^[A-Z]{2}-[0-9]{4}$/.test(input.value) ? ''
      : 'Use two uppercase bed letters, a dash and four ASCII digits.';
    form.querySelector('#legacy-error').textContent = message;
    input.setAttribute('aria-invalid', String(!!message));
    return !message;
  };
  document.addEventListener('focusout', event => {
    if (event.target.id === 'code') check(event.target.form);
  });
  document.addEventListener('submit', event => {
    if (!event.submitter?.formNoValidate && !check(event.target)) {
      event.preventDefault();
      event.stopImmediatePropagation();
    }
  }, true);
}
// endsnippet:client
