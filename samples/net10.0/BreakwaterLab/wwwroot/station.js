// snippet:complete
export function announce(station, reading) {
    if (!station) {
        throw new Error("A station name is required.");
    }
    const output = document.getElementById("announcement");
    output.textContent = station + ": " + reading;
    output.dataset.calls = String(Number(output.dataset.calls || 0) + 1);
}

export function drawFormula(host, reading, multiplier) {
    const namespace = "http://www.w3.org/1998/Math/MathML";
    const math = document.createElementNS(namespace, "math");
    const row = document.createElementNS(namespace, "mrow");
    for (const [tag, text] of [["mi", "d"], ["mo", "="], ["mn", reading], ["mo", "×"], ["mn", multiplier]]) {
        const element = document.createElementNS(namespace, tag);
        element.textContent = String(text);
        if (tag === "mn" && !row.querySelector("mn")) element.id = "reading";
        row.append(element);
    }
    math.append(row);
    host.replaceChildren(math);
}
// endsnippet:complete
