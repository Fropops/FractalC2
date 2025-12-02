# Agents - Agent Management

## Overview

The **Agents** module displays all connected agents and allows you to interact with them.

## Main Interface

### Agents Table

The table displays all agents with the following information:

| Column | Description |
|---------|-------------|
| **ID** | Short agent identifier |
| **Implant ID** | Implant name |
| **Hostname** | Machine name |
| **Username** | User under which the agent runs |
| **IP Address** | Agent IP address |
| **Last Seen** | Last connection (updated in real-time) |
| **Actions** | Action buttons |

### Agent Status

**Last Seen** indicates the time elapsed since the last connection:
- 🟢 **Green**: Active (last connection < 2x sleep)
- 🔴 **Red**: Inactive (last connection > 2x sleep)

## Available Actions

### "Interact" Button
- Opens the interactive terminal for this agent
- Allows sending commands
- Only available if the agent is active

### "Info" Button
- Displays detailed agent information
- System metadata
- Implant configuration

### "Tasks" Button
- Displays task history
- Allows viewing results
- Allows adding outputs to loots

### "Loots" Button
- Displays files retrieved from this agent
- Allows downloading or deleting loots

### "Stop" Button
- Stops the agent
- The agent will stop connecting
- ⚠️ Irreversible action

## Notifications

### New Agent
When a new agent connects, a toast notification appears:
- 🎉 **Title**: "New Agent"
- **Message**: Agent information
- **Button**: "Interact" to open the terminal

## Detailed Information

### Agent Info Page

Accessible via the "Info" button, displays:

#### Agent Metadata
- **Implant ID**: Implant identifier
- **Hostname**: Machine name
- **Username**: User
- **Domain**: Windows domain (if applicable)
- **IP Address**: IP address
- **Process**: Process name and PID
- **Integrity**: Integrity level (Low/Medium/High/System)
- **Architecture**: x86 or x64
- **CLR Version**: .NET CLR version

#### Implant Configuration
- **Sleep**: Check-in interval
- **Jitter**: Sleep variation
- **Kill Date**: Expiration date

#### Connection Information
- **First Seen**: First connection
- **Last Seen**: Last connection
- **Listener**: Listener used

## Best Practices

1. **Monitoring**: Regularly check the "Last Seen" status
2. **Organization**: Use descriptive Implant IDs to quickly identify agents
3. **Security**: Stop unused agents with the "Stop" button
4. **Documentation**: Note important information in your loots

## Filtering and Search

- Agents are sorted by last connection (most recent first)
- Use Ctrl+F in your browser to search for a specific agent

## Example Workflow

1. **Agent connects** → Toast notification appears
2. **Click "Interact"** → Opens terminal
3. **Send commands** → Execution on agent
4. **Check "Tasks"** → Verify results
5. **Retrieve files** → Check "Loots"
