(() => {
    const testFiles = __TEST_FILES__;

    const files = new Map();
    const dirs = new Set();

    for (const [path, content] of Object.entries(testFiles)) {
        files.set(path, typeof content === "string" ? content : JSON.stringify(content));
        const parts = path.split("/").filter(Boolean);
        if (parts.length > 1) {
            dirs.add(parts[0]);
        }
    }

    const mutations = [];

    function logMutation(op, detail) {
        mutations.push({ op, detail, ts: Date.now() });
    }

    function entriesIn(subfolder) {
        const entries = [];
        const seenDirs = new Set();

        for (const path of files.keys()) {
            const parts = path.split("/").filter(Boolean);

            if (subfolder) {
                if (parts.length < 2 || parts[0] !== subfolder) continue;
                // File directly inside this subfolder
                if (parts.length === 2) {
                    entries.push({ name: parts[1], kind: "file" });
                }
            } else {
                // Root level
                if (parts.length === 1) {
                    entries.push({ name: parts[0], kind: "file" });
                } else if (parts.length > 1 && !seenDirs.has(parts[0])) {
                    seenDirs.add(parts[0]);
                    entries.push({ name: parts[0], kind: "directory" });
                }
            }
        }

        // Also include explicitly-created directories (even if empty)
        if (!subfolder) {
            for (const d of dirs) {
                if (!seenDirs.has(d)) {
                    entries.push({ name: d, kind: "directory" });
                }
            }
        }

        return entries;
    }

    // ── Mock API ────────────────────────────────────────────────
    window.fileSystem = {
        isSupported: () => true,

        pickDirectory: async () => "TestRecipes",

        hasStoredDirectory: async () => true,

        reconnectDirectory: async () => "TestRecipes",

        disconnectDirectory: async () => { },

        isConnected: () => true,

        getDirectoryName: () => "TestRecipes",

        listEntries: async (subfolder) => entriesIn(subfolder ?? null),

        readFile: async (path) => {
            const content = files.get(path);
            if (content === undefined) {
                throw new Error(`Mock FS: file not found: ${path}`);
            }
            return content;
        },

        writeFile: async (path, content) => {
            files.set(path, content);
            const parts = path.split("/").filter(Boolean);
            if (parts.length > 1) {
                dirs.add(parts[0]);
            }
            logMutation("writeFile", { path, content });
        },

        deleteFile: async (path) => {
            files.delete(path);
            logMutation("deleteFile", { path });
        },

        createDirectory: async (name) => {
            dirs.add(name);
            logMutation("createDirectory", { name });
        },

        deleteDirectory: async (name) => {
            for (const path of [...files.keys()]) {
                if (path.startsWith(name + "/")) {
                    files.delete(path);
                }
            }
            dirs.delete(name);
            logMutation("deleteDirectory", { name });
        },

        moveFile: async (fromPath, toPath) => {
            const content = files.get(fromPath);
            if (content === undefined) {
                throw new Error(`Mock FS: file not found: ${fromPath}`);
            }
            files.set(toPath, content);
            files.delete(fromPath);
            const parts = toPath.split("/").filter(Boolean);
            if (parts.length > 1) {
                dirs.add(parts[0]);
            }
            logMutation("moveFile", { fromPath, toPath });
        },

        renameDirectory: async (oldName, newName) => {
            for (const [path, content] of [...files.entries()]) {
                if (path.startsWith(oldName + "/")) {
                    const newPath = newName + path.substring(oldName.length);
                    files.set(newPath, content);
                    files.delete(path);
                }
            }
            dirs.delete(oldName);
            dirs.add(newName);
            logMutation("renameDirectory", { oldName, newName });
        },

        downloadBlob: async (filename, base64Content, mimeType) => {
            logMutation("downloadBlob", { filename, mimeType });
        }
    };

    // Expose mutations for test assertions
    window.__fsMutations = mutations;
    // Expose files map for inspection
    window.__fsFiles = files;
})();
