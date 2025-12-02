# Tasks - Task History and Management

## Overview

The **Tasks** module displays the complete history of all tasks sent to an agent, with their results and statuses.

## Interface

### Agent Header
Displays agent information (as in Terminal)

### Tasks Table

| Column | Description |
|---------|-------------|
| **Task ID** | Unique task identifier |
| **Command** | Sent command |
| **Status** | Execution status |
| **Actions** | Action buttons |

## Task Statuses

Tasks can have different statuses:

| Status | Badge | Description |
|--------|-------|-------------|
| **Queued** | 🔵 Blue | Task waiting for execution |
| **Running** | 🟡 Yellow | Task currently executing |
| **Complete** | 🟢 Green | Task completed successfully |
| **Error** | 🔴 Red | Error during execution |

## Available Actions

### "View Result" Button
- Displays the complete task result
- Available for **Complete** or **Error** tasks
- Opens a dedicated page with formatted result

### "Add to Loot" Button
- Saves the task result as a `.txt` file
- Only available for **Complete** tasks with output
- File is named `task_{taskId}.txt`
- Includes agent and task metadata

## Result Page

Accessible via "View Result", displays:

### Header
- Agent information
- Task ID
- Executed command
- Status

### Result
- Complete command output
- Preserved formatting
- Ability to copy text

### Actions
- **"Back" Button**: Return to task list
- **"Add to Loot" Button**: Save result

## Loot File Format

When you add a task to loots, the `.txt` file contains:

```
================================================================================
TASK OUTPUT
================================================================================

Agent ID:        [ImplantId]
Hostname:        [Hostname]
User:            [Username]
IP Address:      [IP]
Process:         [ProcessName] (PID: [ProcessId])

Task ID:         [TaskId]
Command:         [Executed command]
Execution Date:  [Date and time]
Status:          [Status]

================================================================================
OUTPUT
================================================================================

[Command output]
```

## Notifications

- ✅ **Success**: "Task output saved as task_{id}.txt"
- ❌ **Error**: "Failed to add loot" or error details

## Best Practices

1. **Backup**: Systematically add important results to loots
2. **Documentation**: Loot file format includes all necessary metadata
3. **Organization**: Files are automatically named with task ID
4. **Archiving**: Regularly review tasks and archive important results

## Typical Workflow

1. **Send a command** from Terminal
2. **Wait for execution** (status goes from Queued → Running → Complete)
3. **View result** via "View Result"
4. **Save** via "Add to Loot" if important
5. **Download** file from Loots page

## Filtering and Sorting

- Tasks are displayed in chronological order (most recent first)
- Use Ctrl+F to search for a specific command
- Status is clearly visible via colored badges

## Usage Examples

### Save an Enumeration
```
1. Command: ls C:\Users
2. Result: User list
3. Action: "Add to Loot"
4. File: task_abc123.txt (with metadata)
```

### Archive Credentials
```
1. Command: inline-assembly Rubeus.exe dump
2. Result: Kerberos tickets
3. Action: "Add to Loot"
4. File: task_def456.txt (with full context)
```

### Document Reconnaissance
```
1. Command: powershell Get-ComputerInfo
2. Result: System information
3. Action: "Add to Loot"
4. File: task_ghi789.txt (with date and agent)
```
