// snippet:complete
// Controlled local adapter. No provider connection, real token or external login.
window.badgeProof = { calls: [], tokenAvailable: true };
window.AuthenticationService = {
    init(options, logging) { badgeProof.calls.push({ method: 'init', options, logging }); },
    getUser() {
        badgeProof.calls.push({ method: 'getUser' });
        return { name: 'Mira', badge: 'night-observer' };
    },
    getAccessToken(options) {
        badgeProof.calls.push({ method: 'getAccessToken', options });
        return badgeProof.tokenAvailable
            ? { status: 'success', token: { value: 'controlled-not-a-real-token', grantedScopes: ['observations.read'], expires: '2099-01-01T00:00:00Z' } }
            : { status: 'requiresRedirect', token: null };
    }
};
// endsnippet:complete
