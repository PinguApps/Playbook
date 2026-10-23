// Load before any component markup so prerender and interactive failures are observed.
window.beaconPolicy = { violations: [] };
document.addEventListener('securitypolicyviolation', event => {
    window.beaconPolicy.violations.push({
        effectiveDirective: event.effectiveDirective,
        violatedDirective: event.violatedDirective,
        blockedURI: event.blockedURI,
        disposition: event.disposition,
        sourceFile: event.sourceFile,
        lineNumber: event.lineNumber,
        sample: event.sample,
        originalPolicy: event.originalPolicy
    });
});

// Explicit negative controls, run AFTER measuring the ordinary list.
window.beaconPolicy.controls = () => {
    const attribute = document.createElement('div');
    attribute.id = 'attribute-control';
    attribute.className = 'policy-control';
    document.body.append(attribute);
    attribute.setAttribute('style', 'width: 77px');
    const element = document.createElement('style');
    element.textContent = '#element-control { width: 99px; }';
    document.head.append(element);
    const styled = document.createElement('div');
    styled.id = 'element-control';
    styled.className = 'policy-control';
    document.body.append(styled);
    const cssom = document.createElement('div');
    cssom.className = 'policy-control';
    document.body.append(cssom);
    cssom.style.setProperty('width', '88px');
    return {
        attributeWidth: getComputedStyle(attribute).width,
        elementWidth: getComputedStyle(styled).width,
        cssomWidth: getComputedStyle(cssom).width
    };
};
