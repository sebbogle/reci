const DB_NAME = "reci-fs";
const DB_STORE = "handles";
const DB_KEY = "directoryHandle";

let rootHandle = null;
let pendingHandle = null;

function openDb() {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open(DB_NAME, 1);
        request.onupgradeneeded = () => request.result.createObjectStore(DB_STORE);
        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
    });
}

async function storeHandle(handle) {
    const db = await openDb();
    return new Promise((resolve, reject) => {
        const tx = db.transaction(DB_STORE, "readwrite");
        tx.objectStore(DB_STORE).put(handle, DB_KEY);
        tx.oncomplete = () => resolve();
        tx.onerror = () => reject(tx.error);
    });
}

async function loadHandle() {
    const db = await openDb();
    return new Promise((resolve, reject) => {
        const tx = db.transaction(DB_STORE, "readonly");
        const request = tx.objectStore(DB_STORE).get(DB_KEY);
        request.onsuccess = () => resolve(request.result || null);
        request.onerror = () => reject(request.error);
    });
}

async function clearHandle() {
    const db = await openDb();
    return new Promise((resolve, reject) => {
        const tx = db.transaction(DB_STORE, "readwrite");
        tx.objectStore(DB_STORE).delete(DB_KEY);
        tx.oncomplete = () => resolve();
        tx.onerror = () => reject(tx.error);
    });
}

async function resolveDirectory(subfolder) {
    if (!rootHandle) return null;
    if (!subfolder) return rootHandle;
    return await rootHandle.getDirectoryHandle(subfolder);
}

async function resolveParentAndName(path) {
    const parts = path.split("/").filter(Boolean);
    const fileName = parts.pop();
    let dir = rootHandle;
    for (const part of parts) {
        dir = await dir.getDirectoryHandle(part);
    }
    return { dir, fileName };
}

window.fileSystem = {
    isSupported: function () {
        return typeof window.showDirectoryPicker === "function";
    },

    pickDirectory: async function () {
        try {
            rootHandle = await window.showDirectoryPicker({ mode: "readwrite" });
            await storeHandle(rootHandle);
            return rootHandle.name;
        } catch (err) {
            if (err.name === "AbortError") return null;
            throw err;
        }
    },

    hasStoredDirectory: async function () {
        const handle = await loadHandle();
        pendingHandle = handle;
        return handle !== null;
    },

    reconnectDirectory: async function () {
        const handle = pendingHandle || await loadHandle();
        pendingHandle = null;
        if (!handle) return null;

        let permission = await handle.queryPermission({ mode: "readwrite" });
        if (permission !== "granted") {
            permission = await handle.requestPermission({ mode: "readwrite" });
        }
        if (permission !== "granted") return null;

        rootHandle = handle;
        return rootHandle.name;
    },

    disconnectDirectory: async function () {
        rootHandle = null;
        await clearHandle();
    },

    isConnected: function () {
        return rootHandle !== null;
    },

    getDirectoryName: function () {
        return rootHandle ? rootHandle.name : null;
    },

    listEntries: async function (subfolder) {
        const dir = await resolveDirectory(subfolder);
        if (!dir) return [];

        const entries = [];
        for await (const entry of dir.values()) {
            entries.push({ name: entry.name, kind: entry.kind });
        }
        return entries;
    },

    readFile: async function (path) {
        const { dir, fileName } = await resolveParentAndName(path);
        const fileHandle = await dir.getFileHandle(fileName);
        const file = await fileHandle.getFile();
        return await file.text();
    },

    writeFile: async function (path, content) {
        const parts = path.split("/").filter(Boolean);
        const fileName = parts.pop();
        let dir = rootHandle;
        for (const part of parts) {
            dir = await dir.getDirectoryHandle(part, { create: true });
        }
        const fileHandle = await dir.getFileHandle(fileName, { create: true });
        const writable = await fileHandle.createWritable();
        await writable.write(content);
        await writable.close();
    },

    deleteFile: async function (path) {
        const { dir, fileName } = await resolveParentAndName(path);
        await dir.removeEntry(fileName);
    },

    createDirectory: async function (name) {
        if (!rootHandle) throw new Error("No directory connected");
        await rootHandle.getDirectoryHandle(name, { create: true });
    },

    deleteDirectory: async function (name) {
        if (!rootHandle) throw new Error("No directory connected");
        await rootHandle.removeEntry(name, { recursive: true });
    },

    moveFile: async function (fromPath, toPath) {
        const { dir: srcDir, fileName: srcName } = await resolveParentAndName(fromPath);
        const srcHandle = await srcDir.getFileHandle(srcName);
        const file = await srcHandle.getFile();
        const content = await file.text();

        const toParts = toPath.split("/").filter(Boolean);
        const toName = toParts.pop();
        let destDir = rootHandle;
        for (const part of toParts) {
            destDir = await destDir.getDirectoryHandle(part, { create: true });
        }
        const destHandle = await destDir.getFileHandle(toName, { create: true });
        const writable = await destHandle.createWritable();
        await writable.write(content);
        await writable.close();

        await srcDir.removeEntry(srcName);
    },

    renameDirectory: async function (oldName, newName) {
        if (!rootHandle) throw new Error("No directory connected");

        const srcDir = await rootHandle.getDirectoryHandle(oldName);
        const destDir = await rootHandle.getDirectoryHandle(newName, { create: true });

        for await (const entry of srcDir.values()) {
            if (entry.kind === "file") {
                const file = await entry.getFile();
                const content = await file.text();
                const destFile = await destDir.getFileHandle(entry.name, { create: true });
                const writable = await destFile.createWritable();
                await writable.write(content);
                await writable.close();
            }
        }

        await rootHandle.removeEntry(oldName, { recursive: true });
    },

    downloadBlob: async function (filename, base64Content, mimeType) {
        const byteCharacters = atob(base64Content);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        const blob = new Blob([byteArray], { type: mimeType });

        const url = URL.createObjectURL(blob);
        const link = document.createElement("a");
        link.href = url;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    }
};
