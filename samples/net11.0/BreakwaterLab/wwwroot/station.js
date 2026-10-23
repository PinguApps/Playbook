// snippet:complete
export function announce(station, reading) {
    if (!station) {
        throw new Error("A station name is required.");
    }
    const output = document.getElementById("announcement");
    output.textContent = station + ": " + reading;
    output.dataset.calls = String(Number(output.dataset.calls || 0) + 1);
}

// endsnippet:complete
