// wwwroot/js/journal-rte.js
// Small helper for a Teams-like rich text editor (contenteditable)

window.journalRte = {
    // Run a formatting command (bold/italic/lists/headings/etc.)
    exec: function (command, value) {
        document.execCommand(command, false, value ?? null);
    },

    // Get current HTML from the editor
    getHtml: function (editorId) {
        const el = document.getElementById(editorId);
        return el ? (el.innerHTML ?? "") : "";
    },

    // Set HTML into the editor (used when loading/editing an entry)
    setHtml: function (editorId, html) {
        const el = document.getElementById(editorId);
        if (!el) return;
        el.innerHTML = html ?? "";
    },

    // Focus editor (nice UX after clicking toolbar buttons)
    focus: function (editorId) {
        const el = document.getElementById(editorId);
        if (el) el.focus();
    }
};
