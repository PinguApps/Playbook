// Evaluate through the connected browser's CDP Runtime.evaluate on the worksheet origin.
// Switch actual Chrome tabs with Page.bringToFront; disable focus emulation first.
// Never replace document.hidden/visibilityState or dispatch synthetic visibility events.
(async () => {
    const snapshot = async () => ({
        at: new Date().toISOString(),
        visibility: document.visibilityState,
        syntheticVisibility: Object.hasOwn(document, 'visibilityState') || Object.hasOwn(document, 'hidden'),
        readings: document.querySelector('#readings')?.textContent,
        sheet: document.querySelector('#sheet')?.textContent,
        lease: document.querySelector('#lease')?.textContent,
        busy: document.querySelector('#busy')?.textContent,
        events: await (await fetch('/proof/events')).json()
    });
    window.tideProof = { snapshots: [], snapshot };
    window.tideProof.capture = async label => {
        const value = { label, ...await snapshot() };
        if (value.syntheticVisibility) throw new Error('Visibility was overridden.');
        window.tideProof.snapshots.push(value);
        return value;
    };
    return await window.tideProof.capture('initial');
})()
