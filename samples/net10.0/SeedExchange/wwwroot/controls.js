// snippet:complete
// Original browser controls. Browser owns Origin and Sec-Fetch-Site; none are forged.
document.querySelector('#run').onclick = async () => {
  const output = document.querySelector('#result');
  const mode = new URLSearchParams(location.search).get('mode') || 'custom';
  const target = 'http://localhost:6180';
  const trusted = location.origin === target || location.origin === 'http://localhost:6181';
  const own = location.origin === target;
  const tokens = mode === 'old' || mode === 'tokens';
  const results = [];
  const frame = document.createElement('iframe');
  frame.name = 'seed-result';
  frame.title = 'Actual form response';
  document.body.append(frame);
  let token;
  try {
    await fetch('/session');
    if (tokens) {
      const html = await (await fetch('/form')).text();
      token = new DOMParser().parseFromString(html, 'text/html').querySelector('[name=__RequestVerificationToken]').value;
    }
    for (const path of ['seed', 'credentialed', 'public', 'wildcard']) {
      const id = `${mode}-${location.host}-${path}-${crypto.randomUUID()}`;
      const form = document.createElement('form');
      form.method = 'post'; form.action = `${target}/${path}?case=${id}`; form.target = frame.name;
      for (const [name, value] of Object.entries({seed: 'Fern', ...(token ? {__RequestVerificationToken: token} : {})})) {
        const input = document.createElement('input'); input.name = name; input.value = value; form.append(input);
      }
      document.body.append(form);
      try { await new Promise(resolve => { frame.onload = resolve; form.submit(); }); }
      finally { form.remove(); }
      const entry = (await (await fetch('/observations')).json())[id];
      const allowed = mode === 'standard' ? own || (path === 'credentialed' && trusted) : mode === 'tokens' ? location.hostname === 'localhost' : trusted;
      const expected = allowed ? 200 : 400;
      if (!entry || entry.status !== expected || entry.handlerCalls !== (allowed ? 1 : 0) ||
          (mode === 'custom' && entry.providerCalls !== 1)) {
        throw new Error(`${path}: expected ${expected}; ${JSON.stringify(entry)}`);
      }
      results.push({kind:'form', path, ...entry});
    }
    // Cross-origin CORS reads and credential modes are independent of simple form submission.
    for (const path of ['credentialed', 'public', 'wildcard']) {
      for (const credentials of ['omit', 'include']) {
        const id = `${mode}-${location.host}-${path}-${credentials}-${crypto.randomUUID()}`;
        let readable = false, browserStatus = null;
        try {
          const response = await fetch(`${target}/${path}?case=${id}`, {method:'POST', credentials, body:new URLSearchParams({seed:'Moss', ...(token ? {__RequestVerificationToken:token} : {})})});
          browserStatus = response.status; readable = true;
        } catch (error) { if (!(error instanceof TypeError)) throw error; }
        const entry = (await (await fetch('/observations')).json())[id];
        const allowed = mode === 'standard' ? own || (path === 'credentialed' && trusted) : mode === 'tokens' ? !tokens || (credentials === 'include' && location.hostname === 'localhost') : trusted && (mode !== 'old' || credentials === 'include');
        const expected = allowed ? 200 : 400;
        const canRead = own || (path === 'credentialed' && trusted) || (credentials === 'omit' && (path === 'wildcard' || (path === 'public' && trusted)));
        if (!entry || entry.status !== expected || entry.handlerCalls !== (allowed ? 1 : 0) || readable !== canRead ||
            (readable && browserStatus !== expected) || (mode === 'custom' && entry.providerCalls !== 1)) {
          throw new Error(`${path}/${credentials}: expected ${expected}, read ${canRead}; ${JSON.stringify({entry,readable,browserStatus})}`);
        }
        results.push({kind:'fetch', path, credentials, readable, browserStatus, ...entry});
      }
    }
    if (tokens && own) {
      for (const badToken of ['', 'invalid-token']) {
        const id = `${mode}-bad-token-${crypto.randomUUID()}`;
        const response = await fetch(`${target}/seed?case=${id}`, {method:'POST', body:new URLSearchParams({seed:'Fern', __RequestVerificationToken:badToken})});
        const entry = (await (await fetch('/observations')).json())[id];
        if (response.status !== 400 || entry.handlerCalls !== 0) throw new Error('Token failure did not block handler');
        results.push({kind:'bad-token', omitted:!badToken, ...entry});
      }
    }
    output.textContent = JSON.stringify({status:'PASS', mode, origin:location.origin, userAgent:navigator.userAgent, results}, null, 2);
  } catch (error) { output.textContent = JSON.stringify({status:'FAIL', error:String(error), mode, origin:location.origin, results}, null, 2); }
  finally { frame.remove(); }
};
// endsnippet:complete
