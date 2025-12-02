# Implants - Implant Creation

## Overview

The **Implants** module allows you to create and configure custom implants that will connect to your listeners.

## Features

### Create an Implant

1. Click the **"Create Implant"** button
2. Fill in the configuration form:

#### Basic Configuration
- **Implant ID**: Unique implant identifier
- **Description**: Implant description (optional)

#### Listener Configuration
- **Listener**: Select the listener the implant will connect to
- **Connect Address**: Connection address (IP or domain)
- **Connect Port**: Connection port

#### Communication Configuration
- **Sleep**: Interval between check-ins (in seconds)
- **Jitter**: Random sleep variation (0-100%)
- **Kill Date**: Implant expiration date (optional)

3. Click **"Create"**
4. The implant will be generated and automatically downloaded

## Implants Table

The table displays all created implants with:

| Column | Description |
|---------|-------------|
| **Implant ID** | Unique identifier |
| **Description** | Implant description |
| **Listener** | Associated listener |
| **Sleep** | Check-in interval |
| **Jitter** | Sleep variation |

## Important Parameters

### Sleep and Jitter

**Sleep** defines the interval between each agent check-in:
- Low value (10-30s): High responsiveness, more detectable
- Medium value (60-300s): Good compromise
- High value (>300s): Maximum stealth, low responsiveness

**Jitter** adds random variation to sleep:
- 0%: Fixed interval (predictable)
- 20-30%: Recommended to avoid detection
- 50%+: High variation, less predictable

**Example**: Sleep=60s, Jitter=20%
- The agent will wait between 48s and 72s between each check-in

### Kill Date

The **Kill Date** allows you to set an expiration date:
- The implant will stop working after this date
- Useful for time-limited engagements
- Leave empty for an implant without expiration

## Notifications

- ✅ **Success**: Implant is created and downloaded
- ❌ **Error**: Displays error details

## Best Practices

1. **Unique Identifiers**: Use descriptive IDs (e.g., "Target-Workstation-01")
2. **Adapted Sleep**: Adjust according to context (monitored network = high sleep)
3. **Jitter**: Always use at least 20% jitter
4. **Kill Date**: Set a kill date for temporary engagements
5. **Documentation**: Use the Description field to note context

## Configuration Examples

### Stealthy Implant
- Implant ID: `Prod-Server-01`
- Sleep: `300` (5 minutes)
- Jitter: `30%`
- Kill Date: `2025-12-31`

### Responsive Implant
- Implant ID: `Dev-Test`
- Sleep: `10` (10 seconds)
- Jitter: `20%`
- Kill Date: Not set
