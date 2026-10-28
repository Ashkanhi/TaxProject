// Offline interaction checks against the dashboard script. No browser, DB or tax API.
const fs = require('node:fs');
const vm = require('node:vm');
const assert = require('node:assert/strict');
const path = require('node:path');
class Element {
    constructor(dataset = {}) { this.dataset = dataset; this.style = {}; this.events = {}; this.children = []; this.hidden = false; this.classList = { add() {}, remove() {} }; }
    setAttribute(name, value) { this[name] = value; }
    addEventListener(name, callback) { this.events[name] = callback; }
    append(...children) { this.children.push(...children); }
    replaceChildren() { this.children = []; }
    closest(selector) { return selector === '[data-dashboard-select]' ? (this.dataset.dashboardSelect !== undefined ? this : null) : (this.dataset.dashboardState !== undefined ? this : null); }
    focus() { this.focused = true; }
}
const source = path.join(__dirname, '../TaxWebAppCore8');
const view = fs.readFileSync(path.join(source, 'Views/NewTax/Dashboard.cshtml'), 'utf8');
const cards = [...view.matchAll(/class="dashboard-metric [^"]+" data-dashboard-state="(\d)"/g)].map(match => new Element({ dashboardState: match[1] }));
assert.deepEqual(cards.map(card => card.dataset.dashboardState), ['1', '2', '4', '3']);
const ids = new Map();
const byId = id => { if (!ids.has(id)) ids.set(id, new Element()); return ids.get(id); };
const root = byId('newtaxDashboard'); root.dataset = { demo: 'true' };
const groups = ['Unsent', 'Pending', 'Accepted', 'Failed'];
const values = groups.map(value => new Element({ value }));
const bars = groups.map(bar => new Element({ bar }));
const barRows = bars.map(bar => { const row = new Element(); row.querySelector = () => bar; return row; });
const boxes = [];
root.querySelectorAll = selector => ({ '[data-value]': values, '[data-bar]': bars,
    '.dashboard-bar-row': barRows, '[data-dashboard-state]': [...cards, ...barRows], '[data-dashboard-select]': boxes }[selector] || []);
root.querySelector = selector => cards.find(card => selector.includes(`"${card.dataset.dashboardState}"`));
let displayed = [], networkRequests = 0, confirmResult = false, tableOptions, currentPage = 0, filteredNumbers;
const tableRows = options => ({ data: () => ({ toArray: () => {
    const rows = filteredNumbers ? displayed.filter(row => filteredNumbers.has(row.RealInvoiceNumber)) : displayed;
    return options?.page === 'current' ? rows.slice(currentPage * 10, currentPage * 10 + 10) : rows;
} }) });
tableRows.add = rows => { displayed = rows; return table; };
const table = { clear() { displayed = []; return this; }, search() { return this; }, draw() { return this; },
    rows: tableRows, columns: { adjust() {} } };
const $ = () => ({ DataTable: options => { tableOptions = options; return table; } }); $.fn = { dataTable: { render: { text: () => value => value } } };
vm.runInNewContext(fs.readFileSync(path.join(source, 'wwwroot/Content/MyJS/NewTaxDashboard.js'), 'utf8'), {
    document: { getElementById: byId, createElement: () => new Element(), querySelectorAll: () => [] },
    Intl, Date, $, window: { confirm: () => confirmResult }, fetch() { networkRequests++; throw new Error('Demo must not request a server'); }
});
(async () => {
const expected = { 1: 180, 2: 240, 3: 96, 4: 684 };
for (const card of cards) {
    root.events.click({ target: card });
    const state = Number(card.dataset.dashboardState);
    assert.equal(displayed.length, expected[state]);
    assert.equal(new Set(displayed.map(row => row.RealInvoiceNumber)).size, displayed.length);
    assert.ok(displayed.every(row => row.RealInvoiceNumber.startsWith(`DEMO-${state}-`)));
    assert.equal(byId('dashboardDetails').hidden, false);
    assert.equal(card['aria-pressed'], 'true');
}
const selectionColumn = tableOptions.columns[0];
byId('dashboardSelectAll').events.click();
assert.equal(byId('dashboardSelectedCount').textContent, '۹۶', 'Select all includes all pages');
byId('dashboardSelectAll').events.click();
assert.equal(byId('dashboardSelectedCount').textContent, '۰', 'Clear all clears selection');
filteredNumbers = new Set(displayed.slice(3, 6).map(row => row.RealInvoiceNumber));
byId('dashboardSelectAll').events.click();
assert.equal(byId('dashboardSelectedCount').textContent, '۳', 'Select all respects the current search');
byId('dashboardSelectAll').events.click(); filteredNumbers = null;
assert.match(selectionColumn.render(null, 'display', displayed[0], { row: 0 }), /checkbox/);
const movedNumbers = [displayed[0].RealInvoiceNumber, displayed[10].RealInvoiceNumber];
function select(index, checked = true) {
    const box = new Element({ dashboardSelect: String(index) }); box.checked = checked;
    boxes.push(box); root.events.change({ target: box });
}
select(0); currentPage = 1; select(10);
assert.equal(byId('dashboardSelectedCount').textContent, '۲');
await byId('dashboardSendSelected').events.click();
assert.equal(displayed.length, 96, 'Cancel must preserve rows and selection');
assert.equal(byId('dashboardSelectedCount').textContent, '۲');
confirmResult = true;
await byId('dashboardSendSelected').events.click();
assert.equal(displayed.length, 94);
root.events.click({ target: cards.find(card => card.dataset.dashboardState === '2') });
assert.equal(displayed.length, 242);
assert.ok(movedNumbers.every(value => displayed.some(row => row.RealInvoiceNumber === value && row.ReferenceNumber.startsWith('DEMO-RETRY-'))));
assert.equal(selectionColumn.render(null, 'display', displayed[0], { row: 0 }), '', 'Pending rows must not be selectable');
assert.equal(byId('dashboardSendActions').hidden, true);
currentPage = 0;
barRows[0].events.keydown({ key: 'Enter', preventDefault() {} });
assert.equal(displayed.length, 180);
byId('dashboardRefresh').events.click();
assert.equal(displayed.length, 180);
byId('dashboardSelectAll').events.click();
assert.equal(byId('dashboardSelectedCount').textContent, '۱۰۰', 'Select all respects the batch limit');
byId('dashboardSelectAll').events.click();
for (let index = 0; index < 101; index++) select(index);
assert.equal(byId('dashboardSelectedCount').textContent, '۱۰۰', 'Selection is limited to 100 invoices');
root.events.click({ target: cards.find(card => card.dataset.dashboardState === '1') });
assert.equal(byId('dashboardSelectedCount').textContent, '۰');
const unsentNumber = displayed[0].RealInvoiceNumber;
select(0);
await byId('dashboardSendSelected').events.click();
assert.equal(displayed.length, 179, 'A single unsent invoice can be sent');
root.events.click({ target: cards.find(card => card.dataset.dashboardState === '2') });
assert.equal(displayed.length, 243);
assert.ok(displayed.some(row => row.RealInvoiceNumber === unsentNumber && row.ReferenceNumber.startsWith('DEMO-SEND-')));
byId('dashboardDetailsClose').events.click();
assert.equal(byId('dashboardDetails').hidden, true);
assert.equal(networkRequests, 0);
console.log('Dashboard checks passed: four lists, batch retry across pages, cancel, single unsent send, selection limit/reset, counts, keyboard, refresh, close, zero network requests.');
})().catch(error => { console.error(error); process.exitCode = 1; });
