# Commander CLI Guide

**Commander** is the command-line interface for the FractalC2 framework. It allows you to manage the TeamServer, listeners, and implants, as well as interact with agents.

## Getting Started

Launch the Commander executable (e.g., `./Commander`) to start the shell. You will be connected to the locally running TeamServer by default (or as configured).

## 1. Managing Listeners

Listeners are the entry points for your agents to communicate with the TeamServer.

**List Listeners**
```bash
listener show
```

**Start a Listener**
*   **HTTP (Port 80)**:
    ```bash
    listener start --name HttpListener --port 80 --secured false
    ```
*   **HTTPS (Port 443)**:
    ```bash
    listener start --name HttpsListener --port 443 --secured true
    ```

**Stop a Listener**
```bash
listener stop --name HttpListener
```

## 2. Managing Implants

Implants are the payloads enabling execution on target machines.

**List Implants**
```bash
implant show
```

**Generate an Implant**
Use the `generate` action to create a new payload. You must specify a listener or an endpoint.
```bash
implant generate --listener HttpsListener --type exe --arch x64 --download
```
*   `--listener`: Name of an existing listener to connect to.
*   `--type`: Output format (`exe`, `dll`, `ps` (Powershell), `elf` (Linux), `svc` (Service), `bin` (Shellcode)).
*   `--arch`: Architecture (`x64` or `x86`).
*   `--download`: Automatically downloads the generated file to the current directory.

**Download an Existing Implant**
If you didn't download it during generation:
```bash
implant download --name <ImplantName>
```

**Generate Stagers (One-liners)**
To get PowerShell one-liners or bash commands for an existing implant:
```bash
implant script --name <ImplantName>
```

## 3. Interacting with Agents

Once an implant runs on a target, it will check in and appear as an Agent.

**List Agents**
```bash
agents
```
*(Note: `agents` command usually lists all active sessions).*

**Interact with an Agent**
To enter the context of a specific agent to send commands:
```bash
interact <Agent Index>
```
*   **Example**: `interact 1`

**Sending Commands**
Once inside an agent context, you can run standard agent commands (see [Agent Commands](agent-commands.md)):
```bash
(Agent/admin@DESKTOP) > ls
(Agent/admin@DESKTOP) > whoami
(Agent/admin@DESKTOP) > shell ipconfig
```

**Background / Exit Agent**
To return to the main menu without killing the agent:
```bash
back
```


**Exit Commander**
To exit the FractalC2 CLI:
```bash
exit
```

