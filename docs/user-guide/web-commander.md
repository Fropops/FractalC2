# WebCommander Guide

**WebCommander** provides a modern, graphical user interface for the FractalC2 framework. It is accessible via a web browser (default: `http://127.0.0.1:5001`).

## 1. Managing Listeners

Navigate to the **Listeners** page from the sidebar.

*   **View**: The table lists all active listeners with their ports and security status.
*   **Create**: Click the **Create** button.
    *   **Name**: Give a unique name to identify the listener.
    *   **Port**: The binding port (e.g., 80, 443).
    *   **Secured**: Check this box to enable HTTPS.
    *   Click **Start** to launch the listener.
*   **Stop**: Click the **Stop** button next to a running listener to shut it down.

## 2. Managing Implants

Navigate to the **Implants** page.

*   **View**: See all generated implants and their configurations.
*   **Generate**: Click the **Generate** button.
    *   **Listener**: Select an ACTIVE listener from the dropdown. This determines where the implant calls back or CUSTOM to select a custom endpoint.
    *   **Endpoint**: Enter the endpoint URL (autofill if listener is selected).
    *   **Type**: Choose the payload format (`exe`, `dll`, `powershell`, `linux-elf`, etc.).
    *   **Architecture**: `x64` or `x86` (note that there is x86 limitations - not available for all implants).
    *   Click **Generate**.
*   **Download**: After generation, a download button/icon will appear next to the implant in the list. Click it to save the file.
*   **Script**: You may also see options to copy PowerShell one-liners or other staging commands directly from the UI.

## 3. Interacting with Agents

**Dashboard (Home)** / **Topology**
*   The home page often shows a topology view of connected agents.
*   New agents will appear here or in the **Agents** list.

**The Terminal**
The core interaction happens in the **Terminal** page.

1.  Navigate to **Terminal** by clicking on the **Interact** button next to the agent you want to task.
2.  **Command Input**: Type commands into the input bar at the bottom.
    *   Supports the same commands as the CLI (e.g., `ls`, `shell whoami`, `upload`).
3.  **Output**: Task results and command outputs are displayed in the main chat-like window.

**Task History & Loot**
*   The implementation provide dedicated views for **Task History** (to see past commands) and **Loot** (to browse files downloaded from agents).
