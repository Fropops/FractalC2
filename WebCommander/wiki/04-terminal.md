# Terminal - Interactive Command Interface

## Overview

The **Terminal** is the main interface for interacting with an agent. It allows sending commands and viewing results in real-time.

## Interface

### Agent Header
The header displays agent information:
- **Implant ID**
- **Hostname**
- **Username**
- **IP Address**
- **Last Seen** (automatically updated)

### Terminal Area
- Displays command and result history
- Automatic scroll to bottom
- Format: `[timestamp] > command` then result

### Command Bar
- Input field to enter commands
- "Send" button to send
- "Enter" key for quick send

## Usage

### Send a Command

1. Type the command in the input field
2. Press **Enter** or click **"Send"**
3. The command is added to history
4. The result appears upon reception

### Command History

- **Up Arrow**: Previous command
- **Down Arrow**: Next command
- History is preserved during the session

### Auto-completion

Type the beginning of a command and press **Tab** to:
- See command suggestions
- Auto-complete

## Available Commands

### System Commands

| Command | Description | Example |
|----------|-------------|---------|
| `ls` / `dir` | List files | `ls C:\Users` |
| `cd` | Change directory | `cd C:\Windows` |
| `pwd` | Current directory | `pwd` |
| `cat` | Display a file | `cat file.txt` |
| `mkdir` | Create a folder | `mkdir newfolder` |
| `del` / `rm` | Delete a file | `del file.txt` |
| `ps` | List processes | `ps chrome` |

### Execution Commands

| Command | Description | Example |
|----------|-------------|---------|
| `shell` | Execute via cmd.exe | `shell whoami` |
| `powershell` | Execute via PowerShell | `powershell Get-Process` |
| `inline-assembly` | Execute .NET assembly | `inline-assembly tool.exe args` |
| `execute-assembly` | Fork & Run assembly | `execute-assembly tool.exe` |
| `execute-pe` | Execute native PE | `execute-pe tool.exe` |
| `powershell-import` | Import PS script | `powershell-import script.ps1` |

### Management Commands

| Command | Description | Example |
|----------|-------------|---------|
| `help` | Display help | `help` |
| `help <cmd>` | Command help | `help ls` |
| `exit` | Close terminal | `exit` |

## Advanced Features

### Pre-filled Commands

Some interface actions pre-fill the terminal:
- **Use Tool**: Generates the command to use a tool
- Commands are ready to be sent

### History Persistence

- Command history is loaded at startup
- All previous commands are available
- Automatic scroll to bottom when loading

### Agent Status

- If the agent is **inactive**, a warning is displayed
- Commands can still be sent
- They will be executed at the next check-in

## Best Practices

1. **Short Commands**: Prefer short and precise commands
2. **Verification**: Check current directory with `pwd` before operations
3. **PowerShell**: Use `powershell` for complex commands
4. **Tools**: Use appropriate execution commands according to tool type
5. **History**: Check history in the "Tasks" page for detailed results

## Command Examples

### Reconnaissance
```bash
# System information
shell systeminfo
powershell Get-ComputerInfo

# Users
shell net user
shell whoami /all

# Network
shell ipconfig /all
shell netstat -ano
```

### Enumeration
```bash
# Processes
ps
ps chrome

# Files
ls C:\Users
cat C:\Windows\System32\drivers\etc\hosts

# Services
powershell Get-Service
```

### Tool Execution
```bash
# .NET Assembly
inline-assembly Rubeus.exe dump

# PowerShell Script
powershell-import Invoke-Mimikatz.ps1
powershell Invoke-Mimikatz

# Native Executable
execute-pe procdump.exe -ma lsass.exe lsass.dmp
```

## Keyboard Shortcuts

- **Enter**: Send command
- **↑**: Previous command
- **↓**: Next command
- **Tab**: Auto-completion (if available)
- **Ctrl+C**: Copy selected text
