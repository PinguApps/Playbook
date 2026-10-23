// snippet:complete
export function afterWebStarted(blazor) {
    blazor.registerCustomEventType('beforeinput', {
        browserEventName: 'beforeinput',
        createEventArgs: event => ({
            data: event.data ?? '',
            inputType: event.inputType,
            station: event.target.dataset.station
        })
    });
}
// endsnippet:complete
