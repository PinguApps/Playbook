// snippet:module
const tag = 'bracken-menu';
if (!customElements.get(tag)) {
    class BrackenMenu extends HTMLElement {
        #collapse = event => {
            if (!(event.target instanceof Element)) return;
            const area = event.target.closest('.nav-scrollable');
            const toggle = this.querySelector('input[type="checkbox"]');
            if (area && this.contains(area) && toggle?.checked) toggle.checked = false;
        };
        connectedCallback() { this.addEventListener('click', this.#collapse); }
        disconnectedCallback() { this.removeEventListener('click', this.#collapse); }
    }
    customElements.define(tag, BrackenMenu);
}
// endsnippet:module
