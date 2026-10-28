// snippet:complete
window.cedar = {
    echo(value, element) {
        if (!(element instanceof HTMLElement) || element.id !== 'interop-marker') {
            throw new Error('Framework ElementReference converter was not retained');
        }
        return structuredClone(value);
    }
};
// endsnippet:complete
