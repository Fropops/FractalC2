# Loots - Retrieved File Management

## Overview

The **Loots** module allows managing all files retrieved from an agent: screenshots, task results, exfiltrated files, etc.

## Interface

### Agent Header
Displays agent information

### "Add Loot" Button
- Directly opens the browser file selector
- Allows manually uploading a file
- Limit: 100 MB per file
- No file type restrictions

### Tabs

#### "Images" Tab
- Displays image files (jpg, jpeg, png, gif, bmp, webp)
- Gallery view with thumbnails
- Click on an image to display it fullscreen

#### "Files" Tab
- Displays all other files
- Table view with name and actions

## Actions on Loots

### For Images

**From gallery**:
- Click on an image → Fullscreen display

**In fullscreen**:
- **"Back" Button**: Return to gallery
- **"Delete" Button**: Delete image
  - Confirmation required
  - Success/error toast notification

### For Files

**From table**:
- **"Download" Button**: Download file
  - Loading indicator during download
  - File is downloaded to your downloads folder
- **"Delete" Button**: Delete file
  - Confirmation required
  - Success/error toast notification

## File Upload

### Process
1. Click **"+ Add Loot"**
2. Select a file in the selector
3. Upload starts automatically
4. A toast notification confirms success
5. File appears in the appropriate tab

### File Types
- **Images**: Automatically detected and placed in "Images" tab
- **Others**: Placed in "Files" tab

### Size Limit
- Maximum: **100 MB** per file
- For larger files, use other methods

## Notifications

- ✅ **Upload successful**: "Uploaded [filename]"
- ✅ **Deletion successful**: "Deleted [filename]"
- ❌ **Error**: Error details

## Best Practices

1. **Organization**: Use descriptive filenames
2. **Backup**: Regularly download important loots
3. **Cleanup**: Delete unnecessary loots to save space
4. **Documentation**: Task files automatically include metadata
5. **Security**: Encrypt sensitive loots after download

## Loot Sources

Loots can come from several sources:

### 1. Tasks
- Command results saved via "Add to Loot"
- Format: `task_{taskId}.txt`
- Includes complete metadata

### 2. Screenshots
- Screenshots taken by the agent
- Format: Images (png, jpg)
- Displayed in "Images" tab

### 3. Exfiltrated Files
- Files downloaded from the agent
- Any file type
- Original name preserved

### 4. Manual Upload
- Files uploaded via "Add Loot" button
- Useful for sharing files between operators
- Or for archiving external results

## Usage Examples

### Archive Reconnaissance Results
```
1. Execute: powershell Get-ADUser -Filter *
2. In Tasks: "Add to Loot"
3. In Loots: Download task_xxx.txt
4. Analyze offline
```

### Manage Screenshots
```
1. Agent takes a screenshot
2. Image appears in Loots > Images
3. Click to view
4. Download if necessary
5. Delete after archiving
```

### Share Tools
```
1. Click "Add Loot"
2. Select a local tool
3. Upload to server
4. Other operators can download it
```

## Space Management

- Monitor loot sizes
- Regularly delete unnecessary files
- Download and archive important loots locally
- Task files are generally small (<1 MB)
- Screenshots can be large (>1 MB)

## Security

⚠️ **Important**:
- Loots are stored on the TeamServer
- Download and encrypt sensitive data
- Delete loots after local archiving
- Don't leave sensitive data on the server
