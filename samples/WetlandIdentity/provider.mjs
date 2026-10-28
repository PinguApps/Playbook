// Isolated authoring fixture: real OIDC code/PKCE, signed ID tokens and RP logout.
// One fixed fictional account, in-memory state/keys and loopback HTTP only.
import { Provider } from 'oidc-provider';
import { createServer } from 'node:http';
import { generateKeyPairSync, randomBytes } from 'node:crypto';

export async function startIdentity(ports) {
    const origins = ports.map(port => `http://localhost:${port}`);
    const events = [];
    const { privateKey } = generateKeyPairSync('rsa', { modulusLength: 2048 });
    const signingKey = { ...privateKey.export({ format: 'jwk' }), kid: 'wetland-run', use: 'sig', alg: 'RS256' };
    const provider = new Provider('http://localhost:5980', {
        jwks: { keys: [signingKey] },
        clients: [{
            client_id: 'wetland-permits', token_endpoint_auth_method: 'none',
            redirect_uris: origins.map(origin => `${origin}/authentication/login-callback`),
            post_logout_redirect_uris: origins.map(origin => `${origin}/authentication/logout-callback`),
            response_types: ['code'], grant_types: ['authorization_code']
        }],
        claims: { openid: ['sub'], profile: ['name'] },
        cookies: { keys: [randomBytes(32).toString('base64url')] },
        clientBasedCORS: (_ctx, origin) => origins.includes(origin),
        features: {
            devInteractions: { enabled: false },
            rpInitiatedLogout: {
                enabled: true,
                logoutSource(ctx, form) {
                    ctx.body = `<h1>End wetland identity session</h1>${form}<button id="confirm-logout" form="op.logoutForm" name="logout" value="yes">End session</button>`;
                }
            }
        },
        interactions: { url: (_ctx, interaction) => `/fixture/interaction/${interaction.uid}` },
        findAccount: async (_ctx, id) => id === 'reed-keeper'
            ? { accountId: id, claims: async () => ({ sub: id, name: 'Reed Keeper' }) }
            : undefined
    });
    provider.use(async (ctx, next) => {
        // Retain request shapes, never tokens, codes or cookie values.
        events.push({ method: ctx.method, path: ctx.path, queryKeys: Object.keys(ctx.query).sort() });
        await next();
    });
    const callback = provider.callback();
    const server = createServer(async (req, res) => {
        try {
            if (!req.url.startsWith('/fixture/interaction/')) return callback(req, res);
            const details = await provider.interactionDetails(req, res);
            if (details.prompt.name === 'consent') {
                const grant = new provider.Grant({ accountId: details.session.accountId, clientId: details.params.client_id });
                grant.addOIDCScope('openid profile');
                const grantId = await grant.save();
                return await provider.interactionFinished(req, res, { consent: { grantId } }, { mergeWithLastSubmission: true });
            }
            if (req.method === 'POST') {
                return await provider.interactionFinished(req, res, { login: { accountId: 'reed-keeper' } }, { mergeWithLastSubmission: false });
            }
            res.setHeader('Content-Type', 'text/html');
            res.end('<h1>Local test identity: Reed Keeper</h1><form method="post"><button id="identity-login">Sign in test account</button></form>');
        } catch (error) { res.statusCode = 500; res.end(error.message); }
    });
    await new Promise((resolve, reject) => { server.once('error', reject); server.listen(5980, '127.0.0.1', resolve); });
    return { events, close: () => new Promise(resolve => server.close(resolve)) };
}
