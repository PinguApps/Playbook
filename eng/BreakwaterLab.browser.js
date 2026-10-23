(() => {
    const read = id => document.getElementById(id)?.textContent;
    const math = document.querySelector('#formula math') || document.querySelector('math#formula');
    const nodes = math ? [math, ...math.querySelectorAll('*')] : [];
    const receipt = {
        url: location.href,
        userAgent: navigator.userAgent,
        recordedAt: new Date().toISOString(),
        station: read('station'),
        activation: read('activation'),
        session: read('session'),
        scale: read('scale'),
        ready: read('ready'),
        reading: read('reading'),
        outcome: read('outcome'),
        announcement: read('announcement'),
        calls: document.getElementById('announcement')?.dataset.calls || '0',
        math: nodes.map(node => ({ name: node.localName, namespace: node.namespaceURI, text: node.textContent })),
        directControlNamespace: document.getElementById('direct-control')?.namespaceURI || null
    };
    if (receipt.ready !== 'Interactive' || receipt.station !== 'West breakwater' || receipt.scale !== 'Multiplier: 3') throw new Error(JSON.stringify(receipt));
    if (nodes.length !== 7 || nodes.some(node => node.namespaceURI !== 'http://www.w3.org/1998/Math/MathML')) throw new Error(JSON.stringify(receipt));
    return receipt;
})()
