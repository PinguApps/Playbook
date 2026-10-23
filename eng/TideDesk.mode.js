(async () => {
    const wait = ms => new Promise(resolve => setTimeout(resolve, ms));
    for (let i = 0; i < 200; i++) {
        document.querySelector('#check')?.click();
        await wait(100);
        if (Number(document.querySelector('#completed')?.textContent) > 0) {
            return { url: location.href, runtime: document.querySelector('#runtime').textContent,
                count: document.querySelector('#completed').textContent, at: new Date().toISOString(),
                browser: navigator.userAgent };
        }
    }
    throw new Error('Actual interactive click did not complete');
})()
