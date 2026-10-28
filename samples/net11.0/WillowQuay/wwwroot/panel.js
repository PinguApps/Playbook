// snippet:complete
window.willowPanel = {
    lastReference: null,
    async read(reference) {
        this.lastReference = reference;
        // Use the declared identifier, not the C# method name ReceiveLevel.
        return await reference.invokeMethodAsync("AcceptLevel", 137);
    }
};
// endsnippet:complete
