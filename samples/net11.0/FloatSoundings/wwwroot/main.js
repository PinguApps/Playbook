// snippet:browser-host
import { dotnet } from './_framework/dotnet.js';

try {
    const runtime = await dotnet.create();
    const exports = await runtime.getAssemblyExports('FloatSoundings');
    const bridge = exports.Playbook.Samples.BufferBridge;
    let span, segment, typedArray;
    const check = (condition, message) => { if (!condition) throw new Error(message); };
    runtime.setModuleImports('soundings', {
        echo(values) {
            check(ArrayBuffer.isView(values), 'numeric array marshaling creates a typed copy');
            if (values.length) values[0] += 0.5;
            return values.map((value, index) => index === 0 ? value : value + 0.5);
        },
        editSpan(view) {
            const copy = view.slice();
            typedArray = copy.constructor;
            check(copy.length === 2 && copy[0] === 2.5 && copy[1] === -4, 'span range');
            check(bridge.ObserveSpan(view) === 2.5, 'span ToManaged export');
            view.set(new typedArray([3.5, -3]));
            span = view;
        },
        checkExpiredSpan() {
            try { span.slice(); return false; } catch { return true; }
        },
        keepSegment(view) {
            check(view.slice()[0] === 3.5, 'segment offset');
            check(bridge.ObserveSegment(view) === 3.5, 'segment ToManaged export');
            segment = view;
        },
        finishSegment() {
            try {
                const copy = segment.slice();
                check(copy.length === 2 && copy[1] === -3, 'segment survives initial call');
                segment.set(new typedArray([5.5, -1]));
            } finally { segment.dispose(); }
            let rejected = false;
            try { segment.slice(); } catch { rejected = true; }
            check(rejected, 'disposed segment rejects access');
        }
    });
    const result = exports.Playbook.Samples.BridgeChecks.Run();
    const values = [1.25, NaN, Infinity, -Infinity];
    const roundtrip = bridge.ExportArray(values);
    check(values[0] === 1.25 && roundtrip[0] === 1.75 && Number.isNaN(roundtrip[1]) && roundtrip[2] === Infinity && roundtrip[3] === -Infinity, 'array JSExport round trip');
    globalThis.proof = { result, arrayType: typedArray.name, exportArray: 'passed', lifetime: 'passed' };
    document.querySelector('#result').textContent = result;
} catch (error) {
    globalThis.proof = { error: String(error.stack || error) };
    document.querySelector('#result').textContent = globalThis.proof.error;
}
// endsnippet:browser-host
