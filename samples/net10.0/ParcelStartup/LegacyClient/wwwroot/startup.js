// snippet:complete
const style = document.body.dataset.startup;
const shape = new URLSearchParams(location.search).get("shape");
window.parcelProof = { circuit: 0, resources: [], timeout: null };
const options = {
    circuit: {
        configureSignalR: builder => {
            parcelProof.circuit++;
            builder.withServerTimeout(45000);
            const build = builder.build.bind(builder);
            builder.build = () => {
                const connection = build();
                parcelProof.timeout = connection.serverTimeoutInMilliseconds;
                return connection;
            };
        }
    },
    webAssembly: {
        applicationCulture: "fr-FR",
        loadBootResource: (type, name) => {
            parcelProof.resources.push({ type, name });
            return undefined; // Keep framework fetching and integrity checks.
        }
    }
};
// .NET 10 legacy scripts require their own flat option object.
const selected = style === "web" ? options
    : style === "server" ? options.circuit : options.webAssembly;
Blazor.start(selected).catch(error => {
    parcelProof.error = String(error);
    console.error(error);
});
// endsnippet:complete
