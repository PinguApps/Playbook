// Evaluate this expression in the collaborative browser on the running sample's origin.
(async () => {
  const results = [];
  const assert = (condition, message) => { if (!condition) throw new Error(message); };
  const delay = ms => new Promise(resolve => setTimeout(resolve, ms));
  const wait = async predicate => {
    for (let attempt = 0; attempt < 100; attempt++) {
      if (predicate()) return;
      await delay(50);
    }
    throw new Error('Timed out waiting for browser behaviour');
  };
  const frame = document.createElement('iframe');
  frame.style = 'width:100%;height:650px';
  document.body.append(frame);
  const load = async path => {
    await new Promise(resolve => { frame.onload = resolve; frame.src = path; });
    await wait(() => !!frame.contentWindow.Blazor);
    await delay(150);
    return frame.contentDocument;
  };
  const counts = async () => (await fetch('/counts')).json();
  const fill = (doc, values) => {
    for (const [name, value] of Object.entries(values)) {
      const input = doc.querySelector(`[name="${name}"]`);
      input.value = value;
      input.dispatchEvent(new frame.contentWindow.Event('input', { bubbles: true }));
      input.dispatchEvent(new frame.contentWindow.Event('change', { bubbles: true }));
    }
  };
  const valid = {
    'Model.Lead': 'Reef team', 'Model.Contact.Email': 'reef@example.test',
    'Model.Divers[0].Name': 'Aster', 'Model.Divers[1].Name': 'Wren'
  };
  try {
    for (const mode of ['enhanced', 'plain']) {
      let doc = await load('/' + mode);
      const inputs = [...doc.querySelectorAll('input:not([type="hidden"])')];
      assert(inputs.length === 4, 'Expected four editable fields');
      assert(new Set(inputs.map(input => input.id)).size === inputs.length, 'Duplicate editor IDs');
      assert([...doc.querySelectorAll('label')].every(label => label.control && inputs.includes(label.control)), 'Broken label association');
      assert(doc.querySelector('label').textContent === 'Survey lead', 'Display must win over DisplayName');
      assert(doc.querySelector('#fallback').textContent === 'ImportedCode', 'Property fallback');
      assert(doc.querySelectorAll('fieldset label')[0].textContent === 'Diver name', 'DisplayName fallback');
      results.push({ mode, labels: inputs.map(input => ({ id: input.id, label: input.labels[0].textContent.trim() })) });
      const before = await counts();
      const lead = doc.querySelector('[name="Model.Lead"]');
      lead.focus();
      lead.dispatchEvent(new frame.contentWindow.Event('change', { bubbles: true }));
      lead.blur();
      await wait(() => doc.body.textContent.includes('Enter a survey lead.'));
      doc.querySelector('form').requestSubmit(doc.querySelector('button'));
      await delay(250);
      assert((await counts()).posts === before.posts, mode + ': client did not block invalid form');
      fill(doc, valid);
      frame.contentWindow.surveySentinel = 'preserved';
      doc.querySelector('form').requestSubmit(doc.querySelector('button'));
      await wait(() => frame.contentDocument.querySelector('#result')?.textContent === 'Accepted');
      const after = await counts();
      assert(after.posts === before.posts + 1 && after.accepted === before.accepted + 1, mode + ': valid post not accepted');
      assert((frame.contentWindow.surveySentinel === 'preserved') === (mode === 'enhanced'), mode + ': incorrect post/navigation mode');
      results.push({ mode, invalidClientBlocked: true, validPostAccepted: true, enhancedStatePreserved: mode === 'enhanced' });

      doc = await load('/' + mode);
      fill(doc, { ...valid, 'Model.Contact.Email': '', 'Model.Divers[1].Name': '' });
      const bypassBefore = await counts();
      doc.querySelector('form').requestSubmit(doc.querySelector('[formnovalidate]'));
      await wait(() => frame.contentDocument.querySelector('#result')?.textContent === 'Rejected');
      const text = frame.contentDocument.body.textContent;
      assert(text.includes('Enter a contact address.') && text.includes('Enter a diver name.'), 'Nested/collection errors missing');
      assert((await counts()).accepted === bypassBefore.accepted, 'Bypassed invalid graph reached acceptance');
      results.push({ mode, bypassRejectedByServer: true, nestedAndCollectionErrors: true });
    }
    const localized = await load('/enhanced?culture=fr-FR&ui-culture=fr-FR');
    assert(localized.querySelector('#caption').textContent === 'Adresse de contact', 'Localized resource caption');
    assert(localized.documentElement.lang === 'fr', 'Document culture');
    results.push({ localizedResource: true, culture: 'fr-FR' });
    return { userAgent: navigator.userAgent, origin: location.origin, results };
  } finally { frame.remove(); }
})()
