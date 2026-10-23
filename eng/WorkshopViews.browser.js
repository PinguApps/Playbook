// Run on /interactive with the bookmarked query recorded in PIN-781.md.
(async () => {
    const assert = (condition, message) => { if (!condition) throw new Error(message); };
    const wait = async (condition, message) => {
        for (let i = 0; i < 200; i++) {
            if (condition()) return;
            await new Promise(resolve => setTimeout(resolve, 50));
        }
        throw new Error(`Timeout: ${message}`);
    };
    const codes = grid => [...document.querySelectorAll(`#${grid}_ tbody tr`)]
        .map(row => row.querySelector('td')?.textContent.trim()).filter(Boolean).join(',');
    const query = () => new URL(location.href).searchParams;
    const click = selector => document.querySelector(selector).click();
    const passed = [];
    await wait(() => document.querySelector('#ready')?.textContent.includes('True'), 'real circuit');
    assert(codes('repairs') === 'R-E,R-F' && codes('loans') === 'L-B,L-A', 'Both bookmarked sorted pages');
    passed.push('bookmarked two-grid state');
    const original = location.href;
    const links = [...document.querySelectorAll('a.col-title')].map(link => link.href);
    click('#rename');
    await wait(() => document.querySelector('#repairs_ th a').textContent === 'Ticket reference', 'caption changed');
    assert(location.href === original && links.every((url, i) => url === document.querySelectorAll('a.col-title')[i].href), 'Display changes preserve stable sort identities');
    passed.push('live heading change preserves keys and URLs');
    assert(document.querySelector('#loans_ .go-next').getAttribute('aria-disabled') === 'true', 'Last page disabled');
    assert(document.querySelector('#loans_ .go-next').tabIndex === -1, 'Disabled link excluded from tab order');
    assert(getComputedStyle(document.querySelector('#loans_ .go-next')).pointerEvents === 'none', 'Disabled CSS applied');
    click('#repairs_ .go-next');
    await wait(() => query().get('repairs_page') === '3' && codes('repairs') === 'R-D,R-B', 'repairs next');
    assert(codes('loans') === 'L-B,L-A' && query().get('loans_page') === '3', 'Repairs paging leaves loans unchanged');
    click('#loans_ .go-previous');
    await wait(() => query().get('loans_page') === '2' && codes('loans') === 'L-D,L-C', 'loans previous');
    assert(query().get('repairs_page') === '3' && codes('repairs') === 'R-D,R-B', 'Loans paging leaves repairs unchanged');
    passed.push('independent page navigation');
    click('#repairs_ th:first-child a');
    await wait(() => query().get('repairs_sort') === 'code' && codes('repairs') === 'R-E,R-F', 'code ascending');
    click('#repairs_ th:first-child a');
    await wait(() => query().get('repairs_direction') === 'desc' && codes('repairs') === 'R-B,R-A', 'code descending');
    assert(codes('loans') === 'L-D,L-C', 'Repairs sorting leaves loans unchanged');
    click('#loans_ th:nth-child(2) a');
    await wait(() => query().get('loans_sort') === 'priority' && codes('loans') === 'L-A,L-F', 'loans priority');
    assert(codes('repairs') === 'R-B,R-A', 'Loans sorting leaves repairs unchanged');
    passed.push('independent sort navigation');
    history.back();
    await wait(() => query().get('loans_sort') === 'code' && codes('loans') === 'L-D,L-C', 'history back');
    history.forward();
    await wait(() => query().get('loans_sort') === 'priority' && codes('loans') === 'L-A,L-F', 'history forward');
    assert(query().get('scope') === 'open', 'Unrelated query retained');
    passed.push('back forward and unrelated query retention');
    return { kind: 'execution', recordedAt: new Date().toISOString(), browser: navigator.userAgent,
        initialUri: original, finalUri: location.href, repairs: codes('repairs'), loans: codes('loans'), passed };
})()
