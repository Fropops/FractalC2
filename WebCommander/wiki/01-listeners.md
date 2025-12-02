# Listeners - Listener Management

## Overview

The **Listeners** module allows you to create and manage HTTP/HTTPS listeners that listen for incoming agent connections.

## Features

### Create a Listener

1. Click the **"Create Listener"** button
2. Fill in the form:
   - **Name**: Listener name (e.g., "Local", "Production")
   - **IP Address**: Listening IP address (0.0.0.0 for all interfaces, 127.0.0.1 for localhost)
   - **Port**: Listening port (e.g., 443, 80, 8080)
   - **Secured (HTTPS)**: Check to enable HTTPS
3. Click **"Create"**

### Stop a Listener

- Click the **"Stop"** button next to the listener to stop
- A confirmation toast will appear

## Listeners Table

The table displays all active listeners with the following information:

| Column | Description |
|---------|-------------|
| **Name** | Listener name |
| **Address** | Listening IP address |
| **Port** | Listening port |
| **Secured** | Badge indicating if HTTPS is enabled |
| **Actions** | Button to stop the listener |

## Notifications

- ✅ **Success**: "Listener [name] created successfully"
- ❌ **Error**: Displays the error detail returned by the API

## Best Practices

- **HTTPS**: Always use HTTPS in production to encrypt communications
- **Ports**: Use standard ports (443, 80) to avoid firewall blocks
- **Naming**: Give descriptive names to your listeners (e.g., "Prod-HTTPS", "Dev-Local")

## Configuration Examples

### Production Listener
- Name: `Production`
- IP: `0.0.0.0`
- Port: `443`
- Secured: ✅ Yes

### Development Listener
- Name: `Dev-Local`
- IP: `127.0.0.1`
- Port: `8080`
- Secured: ❌ No
