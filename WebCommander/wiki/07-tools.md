# Tools - Tool and Script Library

## Overview

The **Tools** module allows managing a centralized library of tools and scripts that can be used with agents.

## Interface

### Filter Bar

#### Type Filters
- **All**: Displays all tools
- **DotNet**: .NET assemblies only
- **Exe**: Native executables only
- **Powershell**: PowerShell scripts only

#### Search Bar
- Real-time search (300ms debounce)
- Search by tool name
- Combined filter with selected type

### "Add Tool" Button
- Opens file selector
- Accepted types: `.exe` and `.ps1`
- Limit: 50 MB per file

## Tools Table

| Column | Description |
|---------|-------------|
| **Name** | Filename |
| **Type** | Badge indicating type (DotNet, Exe, Powershell) |
| **Actions** | "Use Tool" button |

### Type Badges

- 🟦 **DotNet**: .NET Assembly
- 🟩 **Exe**: Native executable
- 🟨 **Powershell**: PowerShell script

## Tool Upload

### Process
1. Click **"+ Add Tool"**
2. Select a `.exe` or `.ps1` file
3. Upload starts automatically
4. Type is automatically detected:
   - `.ps1` → Powershell
   - `.exe` → Exe (default)
5. Success/error toast notification
6. Tool appears in the list

### Automatic Type Detection
- **PowerShell**: `.ps1` extension
- **Exe**: `.exe` extension
- .NET assemblies must be uploaded with `.exe` extension

## Using a Tool

### "Use Tool" Button

Clicking "Use Tool" opens a popup with:

#### 1. Agent Selection
- Dropdown list of active agents
- Format: `(ID) ImplantId - Description`

#### 2. Tool Name
- Displayed as read-only
- Uploaded filename

#### 3. Execution Method (.NET tools only)
- **inline-assembly** (default): In-memory execution
- **execute-assembly**: Fork & Run

#### 4. Parameters (Not displayed for PowerShell)
- Text area for arguments
- Optional
- Passed to execution command

### Generated Commands

Depending on tool type, the generated command differs:

#### PowerShell Tools
```
powershell-import script.ps1
```
- No parameters
- Script is imported for later use

#### Exe Tools
```
execute-pe tool.exe [parameters]
```
- Fork & Run
- Optional parameters

#### .NET Tools
```
inline-assembly tool.exe [parameters]
# or
execute-assembly tool.exe [parameters]
```
- Choice of execution method
- Optional parameters

## Notifications

- ✅ **Upload successful**: "Tool [name] uploaded successfully"
- ❌ **Error**: Error details

## Best Practices

1. **Naming**: Use descriptive names for your tools
2. **Organization**: Use filters to organize your library
3. **Versions**: Include version in name (e.g., `Rubeus-v2.0.exe`)
4. **Documentation**: Document parameters in a separate file
5. **Testing**: Test tools on a test agent before production use

## Common Tool Examples

### Common .NET Tools
- **Rubeus.exe**: Kerberos manipulation
- **Seatbelt.exe**: System enumeration
- **SharpHound.exe**: BloodHound collection
- **SharpUp.exe**: Privilege escalation enumeration

### Common PowerShell Scripts
- **Invoke-Mimikatz.ps1**: Credential extraction
- **PowerView.ps1**: Active Directory enumeration
- **Invoke-Kerberoast.ps1**: Kerberoasting
- **PowerUp.ps1**: Privilege escalation

### Native Executables
- **procdump.exe**: Process dump
- **PsExec.exe**: Remote execution
- **mimikatz.exe**: Credential extraction

## Typical Workflow

### Use a .NET Tool
```
1. Filter by "DotNet"
2. Find "Rubeus.exe"
3. Click "Use Tool"
4. Select agent
5. Choose "inline-assembly"
6. Enter parameters: "dump"
7. Click "Execute"
8. Terminal opens with: inline-assembly Rubeus.exe dump
9. Send command
```

### Use a PowerShell Script
```
1. Filter by "Powershell"
2. Find "Invoke-Mimikatz.ps1"
3. Click "Use Tool"
4. Select agent
5. Click "Execute" (no parameters)
6. Terminal opens with: powershell-import Invoke-Mimikatz.ps1
7. Send command
8. Then use: powershell Invoke-Mimikatz
```

### Use a Native Executable
```
1. Filter by "Exe"
2. Find "procdump.exe"
3. Click "Use Tool"
4. Select agent
5. Enter parameters: "-ma lsass.exe lsass.dmp"
6. Click "Execute"
7. Terminal opens with: execute-pe procdump.exe -ma lsass.exe lsass.dmp
8. Send command
```

## Library Management

### Organization
- Use filters to navigate quickly
- Search allows finding a specific tool
- Tools are sorted alphabetically

### Maintenance
- Delete obsolete tools (feature coming soon)
- Update tools with new versions
- Document version changes

## Security

⚠️ **Important**:
- Tools are stored on the TeamServer
- Always verify tool sources
- Scan tools before upload
- Don't upload unauthorized malicious tools
- Respect rules of engagement
