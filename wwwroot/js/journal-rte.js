// wwwroot/js/rte.js
// Simple Rich Text Editor helper for Blazor (MAUI WebView)
// NOTE: document.execCommand is deprecated, but still works in WebView for milestone purposes.
// We cast document to "any" to avoid VS/TypeScript squiggles.

window.rte = {
    exec: (editorId, command, value) => {
        const el = document.getElementById(editorId);
        if (!el) return;

        // Keep focus inside editor so commands apply correctly
        el.focus();

        // Cast document to any to silence IDE typing errors
        const d = /** @type {any} */ (document);

        try {
            d.execCommand(command, false, value ?? null);
        } catch (e) {
            console.warn("rte.exec failed:", command, e);
        }
    },

    formatBlock: (editorId, tagName) => {
        const el = document.getElementById(editorId);
        if (!el) return;

        el.focus();

        const d = /** @type {any} */ (document);

        try {
            d.execCommand("formatBlock", false, tagName); // e.g. "H2", "P"
        } catch (e) {
            console.warn("rte.formatBlock failed:", tagName, e);
        }
    },

    createLinkWithPrompt: (editorId) => {
        const el = document.getElementById(editorId);
        if (!el) return;

        el.focus();

        const url = prompt("Enter link URL (e.g., https://example.com):");
        if (!url) return;

        const d = /** @type {any} */ (document);

        try {
            d.execCommand("createLink", false, url);
        } catch (e) {
            console.warn("rte.createLink failed:", e);
        }
    },

    unlink: (editorId) => {
        const el = document.getElementById(editorId);
        if (!el) return;

        el.focus();

        const d = /** @type {any} */ (document);

        try {
            d.execCommand("unlink", false, null);
        } catch (e) {
            console.warn("rte.unlink failed:", e);
        }
    },

    getHtml: (editorId) => {
        const el = document.getElementById(editorId);
        return el ? el.innerHTML : "";
    },

    setHtml: (editorId, html) => {
        const el = document.getElementById(editorId);
        if (!el) return;
        el.innerHTML = html || "";
    }
};
