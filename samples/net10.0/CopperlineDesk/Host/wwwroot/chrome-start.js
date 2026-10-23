// snippet:complete
// Demonstration switch only: this value is public and controlled by the browser.
const requested = new URLSearchParams(location.search).get('browserEnvironment') ?? 'Review';
const environment = /^(review|production)$/i.test(requested) ? requested : 'Review';
Blazor.start({ webAssembly: { environment } });
// endsnippet:complete
