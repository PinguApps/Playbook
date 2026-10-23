// snippet:browser
(async () => {
    const profile = new URLSearchParams(location.search).get('profile') ?? 'Testing';
    if (!['Testing', 'Production'].includes(profile)) throw new Error('Unknown public profile');
    const response = await fetch(`public-settings.json?profile=${profile}`, { cache: 'no-store' });
    if (!response.ok) throw new Error(`Public settings: ${response.status}`);
    const settings = await response.json();
    // Test controls are explicit public runtime values, never server OS variables.
    const query = new URLSearchParams(location.search);
    settings.variables.BOARD_DUPLICATE = query.has('duplicate') ? '1' : '0';
    settings.variables.BOARD_SKIP_PROVIDER = query.has('skipProvider') ? '1' : '0';
    await Blazor.start({
        environment: settings.environment,
        configureRuntime: runtime => runtime.withEnvironmentVariables(settings.variables)
    });
})().catch(error => { document.getElementById('app').textContent = `Startup failed: ${error.message}`; });
// endsnippet:browser
