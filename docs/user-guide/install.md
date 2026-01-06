# Installation Guide

This guide describes how to install FractalC2 using the provided installation script. The script is designed for Debian-based Linux environments (e.g., Kali Linux, Ubuntu).

## Prerequisites
*   A Debian-based Linux distribution (Debian, Ubuntu, Kali).
*   `wget` installed.
*   Internet connection (to download repositories and dependencies).

## Quick Install (One-Liner)

The easiest way to install FractalC2 is to use the automated script hosted on GitHub.

### Full Installation (TeamServer + WebCommander + Commander)
This command installs all components and dependencies. **Run as root (sudo)**.

```bash
wget -qO- https://raw.githubusercontent.com/Fropops/FractalC2/refs/heads/main/Install/install.sh | sudo bash -s -- All
```

### Full Installation (Without auto-start)
Installs all components but does not launch them immediately after installation.

```bash
wget -qO- https://raw.githubusercontent.com/Fropops/FractalC2/refs/heads/main/Install/install.sh | sudo bash -s -- All noRun
```

### Component-Specific Installation

If you only need specific components, you can install them individually.

**Install only WebCommander:**
```bash
wget -qO- https://raw.githubusercontent.com/Fropops/FractalC2/refs/heads/main/Install/install.sh | sudo bash -s -- WebCommander
```

**Install only TeamServer:**
```bash
wget -qO- https://raw.githubusercontent.com/Fropops/FractalC2/refs/heads/main/Install/install.sh | sudo bash -s -- TeamServer
```

**Install only Commander (CLI):**
```bash
wget -qO- https://raw.githubusercontent.com/Fropops/FractalC2/refs/heads/main/Install/install.sh | sudo bash -s -- Commander
```

## Post-Installation

After a successful installation, the components will be running on local interfaces:

*   **TeamServer**: `http://127.0.0.1:5000`
    *   **Default User**: `Admin`
    *   **API Key**: Generated during install (check the output logs).
*   **WebCommander**: `http://127.0.0.1:5001`

### Running Manually

If you installed with `noRun` or need to restart the services later, you can find the binaries in the installation folder (usually created in the current directory).

**Start TeamServer**:
```bash
./TeamServer/TeamServer
```

**Start WebCommander**:
```bash
./WebCommanderHost/WebCommanderHost
```

**Start Commander**:
```bash
./Commander/Commander
```
