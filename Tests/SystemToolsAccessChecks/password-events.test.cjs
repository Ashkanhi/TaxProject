const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

function element() {
    return {
        hidden: true, disabled: false, textContent: 'ورود', handlers: {},
        addEventListener(name, handler) { this.handlers[name] = handler; },
        querySelector() { return this; },
        setAttribute() {}, removeAttribute() {}
    };
}
const nodes = Object.fromEntries(['systemToolsPassword', 'accessToggle', 'accessCapsLock', 'accessSubmit', 'accessForm'].map(id => [id, element()]));
const windowStub = element();
vm.runInNewContext(fs.readFileSync(path.join(__dirname, '../../TaxWebAppCore8/wwwroot/Content/MyJS/SystemToolsAccess.js'), 'utf8'), {
    document: { getElementById: id => nodes[id] }, window: windowStub
});
const input = nodes.systemToolsPassword;
const caps = nodes.accessCapsLock;
for (const eventName of ['keydown', 'keyup']) {
    const handler = input.handlers[eventName];
    handler({ getModifierState(key) { assert.equal(key, 'CapsLock'); return true; } });
    assert.equal(caps.hidden, false, 'Caps Lock on shows the hint');
    handler({ type: eventName });
    assert.equal(caps.hidden, true, 'Synthetic event without getModifierState does not throw');
    handler({ getModifierState: false });
    assert.equal(caps.hidden, true, 'Non-function property does not throw');
    handler({ getModifierState() { return false; } });
    assert.equal(caps.hidden, true, 'Caps Lock off hides the hint');
}
input.handlers.keydown({ getModifierState() { return true; } });
input.handlers.blur();
assert.equal(caps.hidden, true, 'Blur clears the hint');
nodes.accessForm.handlers.submit();
assert.equal(nodes.accessSubmit.disabled, true, 'Submission still works after synthetic events');
windowStub.handlers.pageshow();
assert.equal(nodes.accessSubmit.disabled, false, 'Returning to the page enables submit');
console.log('Password keyboard event regression checks passed.');
