# List anchoring evidence

Historical authoring observations from October 2026. Compilation stays in the
sample records; these observations do not establish a new run, broader platform
support or GA verification. Reuse only while the claim and relevant inputs remain
valid; see [capture and refresh](../contributing.md#capture-and-refresh-sample-evidence).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-784.md).

## Actual rendered evidence

[Browser report](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-784.browser.evidence.json) records actual Chrome 155 on
Windows x64, Interactive Server, real provider calls and browser observers.
`eng/OrchardJournal.browser.js` asserts the rendered IDs/geometry and local input
state after Start/End/None edge changes, fresh provider materialization, prepends,
appends and expansion above the reader. Its core returns at an explicit native
input boundary. Drive ten real PageUp key presses in the focusable journal, let
layout settle, then call `window.orchardAuthoring.appendAfterNativeScroll()`.
That stage asserts an actual earlier provider window and checks retained ID/offset
after append. It selects None; drive a real Home key, let the head render, then
call `window.orchardAuthoring.prependAfterNativeHome()`. Only completion of all
three stages is a passing check. No observer is mocked, and no mutation runs
while the stages wait for native input. Native End/Home also load the actual
distant tail/head. No calculated-offset-only proof.

The newer list renders 35 rows for four visible middle rows, not all 120. Initial
heights are 84/104/124px. Entry 22 expands from 104 to 232px while entry 23 remains
at approximately -8px (less than 0.5px drift after prepend). The reader note
survives fresh provider instances while rendered. End follows tail 122; scrolling
away retains the earlier row after append. None preserves the old head after a
prepend. The older bounded list passes the same policies using its own module.

T3 preview opened but two snapshots failed with
`Preview automation snapshot timed out after 15000ms`; evaluation found no
virtualized rows. The existing Chrome extension fallback delivered real framework
layout/observer callbacks. This is a browser-surface limitation, not a framework
blocker. No Safari/table-layout, WebAssembly/Auto, other-platform or GA proof is
claimed. No required Human-only proof remains for the declared local route.

An earlier repeat failed: after scripted Home/offset movement, append moved entry
22 at -8px to entry -1 at 0px. It superseded the initial pass and is retained in
the report. The pinned browser source distinguishes user input from self-restore
and keeps pending edge jumps. Direct offset assignment did not establish the
settled user-scroll state. The staged native-input check above replaces that
fixture path; native PageUp loaded entries 72–106, then append retained entry 82
within 0.7px in a separate trace. This does not establish a general programmatic
scroll contract; PIN-785 owns that guidance.
