# Loot & Exfiltration Management — Functional Documentation

## Purpose and Business Value

During red team operations and penetration tests, capturing evidence (credentials, configuration files, confidential databases, desktop screenshots) demonstrates assessment impact. The **Loot & Exfiltration Management** module provides:
- **Centralized Engagement Vault**: Automatically associates exfiltrated files, screen captures, and saved task outputs with the originating compromised host.
- **Visual Screenshot Gallery**: Thumbnail grid viewer enabling rapid browsing of captured victim desktop screenshots without downloading large files.
- **Direct Browser Exfiltration & Ingestion**: One-click download of exfiltrated artifacts to the operator's machine, or manual upload of external artifacts up to 100MB.

---

## Actors and Triggers

- **Red Team Operator**: Browses captured screenshots, downloads exfiltrated files, or manually uploads evidence to the loot repository.
- **Automated Agent Tasks**: Commands such as `capture` (screenshots), `download` (file transfer), or the "Add to Loot" task button push artifacts directly into the loot vault.

---

## Inputs and Outputs

### Inputs
- **Loot Upload**: Browser file input accepting files up to 100MB.
- **Navigation**: Tab toggles between **Images** and **Files**.
- **Inspection**: Clicking on a screenshot card to inspect it in full resolution.

### Outputs
- **Loot Image Gallery** (`/loots/{AgentId}` -> Images Tab):
  - Thumbnail gallery rendering base64 image data previews with responsive card layouts and hover effects.
- **Full-Screen Image Viewer** (`/loots/{AgentId}/image/{FileName}`):
  - High-resolution modal viewer with zoom and delete controls.
- **File Catalog** (`/loots/{AgentId}` -> Files Tab):
  - Table of text logs, downloaded binaries, and database dumps with file size and **Download** / **Delete** actions.
- **Direct File Download**: JavaScript-driven binary stream download directly through the browser.

---

## Operational Workflows

### 1. Reviewing and Downloading Exfiltrated Files

```mermaid
sequenceDiagram
    autonumber
    actor Op as Operator
    participant UI as Loots Page (/loots/{AgentId})
    participant View as Full Image Viewer
    participant TS as TeamServer Loot API
    participant Browser as Web Browser

    Op->>UI: Navigates to Loots -> Selects "Images" Tab
    UI->>TS: GET /api/Loot/{AgentId} (with thumbnails)
    TS-->>UI: Returns image metadata and thumbnail previews
    UI-->>Op: Displays screenshot gallery

    Op->>UI: Clicks screenshot card "desktop_2026.png"
    UI->>View: Navigate to /loots/{AgentId}/image/{FileName}
    View->>TS: GET full-size image data
    TS-->>View: Returns high-resolution image data
    View-->>Op: Displays full-screen screenshot preview

    Op->>UI: Switches to "Files" tab -> Clicks "Download" on "passwords.kdbx"
    UI->>TS: GET full file binary data
    TS-->>UI: Returns Base64 payload
    UI->>Browser: Invoke window.downloadFile()
    Browser-->>Op: File saved to operator's Downloads directory
```

---

## Business Rules and Edge Cases

- **Automatic Media Detection**: When files are uploaded or exfiltrated, WebCommander inspects file extensions (`.png`, `.jpg`, `.jpeg`, `.gif`, `.bmp`, `.webp`) and automatically categorizes them into either the image gallery or the file list.
- **Bandwidth-Optimized Gallery**: The initial gallery view fetches lightweight thumbnails rather than full image payloads, conserving network bandwidth during low-speed or mobile tethered operations.

---

## Dependencies on Other Systems

- **TeamServer Loot API**: Serves file data and thumbnail streams (`/api/Loot`).
- **JavaScript Interop (`downloadFile`)**: Manages browser blob creation and download triggers.

For technical implementation details, see [Technical: Components & UI](../../Technical/WebCommander/components-and-ui.md).
