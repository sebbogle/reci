// In-memory File System Access API shim for Playwright E2E tests.
// Replaces window.fileSystem before the app loads so the Blazor app
// operates against a virtual file tree seeded from window.__testRecipes.

(() => {
    const recipes = window.__testRecipes || [];

    // Virtual file tree: { "": { files: { "name.reci": content }, dirs: Set } }
    // Top-level keys are directory names ("" = root).
    const tree = { "": { files: {}, dirs: new Set() } };

    function ensureDir(name) {
        if (name && !tree[name]) {
            tree[name] = { files: {}, dirs: new Set() };
            tree[""].dirs.add(name);
        }
    }

    function sanitizeName(name) {
        if (!name || !name.trim()) return "Untitled";
        let sanitized = name.replace(/[<>:"/\\|?*\x00-\x1F]/g, "");
        sanitized = sanitized.trim().replace(/\.+$/, "");
        return sanitized || "Untitled";
    }

    function toFileName(recipeName) {
        let sanitized = sanitizeName(recipeName);
        const maxBase = 196; // 200 - ".reci".length
        if (sanitized.length > maxBase) {
            sanitized = sanitized.substring(0, maxBase).trimEnd();
        }
        return sanitized + ".reci";
    }

    // Seed the virtual file tree from the test recipe array.
    for (const recipe of recipes) {
        const group = recipe.group || "";
        const fileName = toFileName(recipe.name);
        const dirName = group ? sanitizeName(group) : "";

        if (dirName) {
            ensureDir(dirName);
        }

        const dir = dirName || "";
        if (!tree[dir]) {
            tree[dir] = { files: {}, dirs: new Set() };
            if (dir) tree[""].dirs.add(dir);
        }

        // Serialize recipe as the app would (camelCase, indented, no nulls).
        tree[dir].files[fileName] = JSON.stringify(recipe, (key, value) => {
            if (value === null) return undefined;
            return value;
        }, 2);
    }

    let connected = true;
    const folderName = "Test Recipes";

    window.fileSystem = {
        isSupported: function () {
            return true;
        },

        pickDirectory: async function () {
            connected = true;
            return folderName;
        },

        hasStoredDirectory: async function () {
            return true;
        },

        reconnectDirectory: async function () {
            connected = true;
            return folderName;
        },

        disconnectDirectory: async function () {
            connected = false;
        },

        isConnected: function () {
            return connected;
        },

        getDirectoryName: function () {
            return connected ? folderName : null;
        },

        listEntries: async function (subfolder) {
            const dirName = subfolder || "";
            const dir = tree[dirName];
            if (!dir) return [];

            const entries = [];

            // List subdirectories (only from root).
            if (!subfolder) {
                for (const d of tree[""].dirs) {
                    entries.push({ name: d, kind: "directory" });
                }
            }

            // List files.
            for (const name of Object.keys(dir.files)) {
                entries.push({ name: name, kind: "file" });
            }

            return entries;
        },

        readFile: async function (path) {
            const { dir, fileName } = resolvePath(path);
            const dirObj = tree[dir];
            if (!dirObj || !(fileName in dirObj.files)) {
                throw new Error(`File not found: ${path}`);
            }
            return dirObj.files[fileName];
        },

        writeFile: async function (path, content) {
            const { dir, fileName } = resolvePath(path);
            ensureDir(dir);
            const dirObj = tree[dir] || tree[""];
            dirObj.files[fileName] = content;
        },

        deleteFile: async function (path) {
            const { dir, fileName } = resolvePath(path);
            const dirObj = tree[dir];
            if (dirObj) {
                delete dirObj.files[fileName];
            }
        },

        createDirectory: async function (name) {
            ensureDir(name);
        },

        deleteDirectory: async function (name) {
            delete tree[name];
            tree[""].dirs.delete(name);
        },

        moveFile: async function (fromPath, toPath) {
            const from = resolvePath(fromPath);
            const to = resolvePath(toPath);

            const srcDir = tree[from.dir];
            if (!srcDir || !(from.fileName in srcDir.files)) {
                throw new Error(`Source file not found: ${fromPath}`);
            }

            const content = srcDir.files[from.fileName];
            delete srcDir.files[from.fileName];

            ensureDir(to.dir);
            const destDir = tree[to.dir] || tree[""];
            destDir.files[to.fileName] = content;
        },

        renameDirectory: async function (oldName, newName) {
            const srcDir = tree[oldName];
            if (!srcDir) throw new Error(`Directory not found: ${oldName}`);

            ensureDir(newName);
            const destDir = tree[newName];

            for (const [name, content] of Object.entries(srcDir.files)) {
                destDir.files[name] = content;
            }

            delete tree[oldName];
            tree[""].dirs.delete(oldName);
        },

        downloadBlob: async function (filename, base64Content, mimeType) {
            // Capture download data for test assertions instead of triggering a real download.
            window.__lastDownload = {
                filename: filename,
                base64Content: base64Content,
                mimeType: mimeType,
                timestamp: Date.now()
            };
        }
    };

    function resolvePath(path) {
        const parts = path.split("/").filter(Boolean);
        const fileName = parts.pop();
        const dir = parts.join("/") || "";
        return { dir, fileName };
    }
})();
