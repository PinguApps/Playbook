// snippet:probe
window.brackenPolicy = { violations: [], pageToken: crypto.randomUUID(), enhanced: 0 };
document.addEventListener('securitypolicyviolation', event => {
    window.brackenPolicy.violations.push({ effectiveDirective: event.effectiveDirective,
        blockedURI: event.blockedURI, disposition: event.disposition, originalPolicy: event.originalPolicy });
});
document.addEventListener('blazor:enhancedload', () => window.brackenPolicy.enhanced++);
// Blazor's public enhanced-navigation event is registered after the boot script loads.
window.addEventListener('load', () => Blazor.addEventListener('enhancedload', () => window.brackenPolicy.enhanced++));
// endsnippet:probe
