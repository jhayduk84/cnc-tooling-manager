# Network Storage Configuration for Setup Sheets

The CNC Tooling Manager supports storing Excel setup sheets on network drives (NAS/file servers) instead of local storage.

## Configuration

### 1. Edit `backend/CncTooling.Api/appsettings.json` or `appsettings.Development.json`

Add or modify the `FileStorage` section:

```json
{
  "FileStorage": {
    "SetupSheetsPath": "\\\\YourServer\\SharedFolder\\SetupSheets",
    "UseNetworkPath": true
  }
}
```

### 2. Configuration Options

- **SetupSheetsPath**: The UNC path to your network share
  - Example: `\\\\192.168.1.100\\CncData\\SetupSheets`
  - Example: `\\\\FileServer\\Manufacturing\\CNC\\SetupSheets`
  
- **UseNetworkPath**: Set to `true` to enable network storage, `false` for local storage

### 3. Network Path Requirements

- The path must be accessible from the server running the API
- The application pool identity or service account must have:
  - **Read** permissions to retrieve files
  - **Write** permissions to upload new files
  - **Modify** permissions to create directories

### 4. Windows Authentication Setup (if using Docker)

For Docker environments accessing Windows network shares:

```yaml
# docker-compose.yml
services:
  api:
    volumes:
      - type: bind
        source: //YourServer/SharedFolder/SetupSheets
        target: /app/setup-sheets
```

Then set `SetupSheetsPath` to `/app/setup-sheets` in your config.

### 5. Linux/Unix Network Shares

For SMB/CIFS mounts on Linux:

```bash
# Mount the share
sudo mount -t cifs //server/share /mnt/setupsheets -o username=user,password=pass

# In appsettings.json
"SetupSheetsPath": "/mnt/setupsheets"
```

## How It Works

1. **File Upload**: When `UseNetworkPath` is `true`, files are saved directly to the network path
2. **File Storage**: The full network path is stored in the database `FilePath` column
3. **File Retrieval**: The API serves files from the network path through the `/api/setupsheets/view/{id}` endpoint
4. **File Download**: Downloads are handled through `/api/setupsheets/download/{id}` endpoint

## Benefits

✅ Centralized storage accessible by multiple machines  
✅ Easy backup and disaster recovery  
✅ Shared access across teams  
✅ No local disk space limitations  
✅ Integration with existing file servers/NAS

## Switching Between Local and Network Storage

You can switch at any time by changing `UseNetworkPath`:

- **Local → Network**: Existing files remain in local storage and work fine. New uploads go to network path.
- **Network → Local**: Existing network files continue to be served. New uploads go to local storage.

## Troubleshooting

### "File not found" errors
- Verify the network path is accessible from the API server
- Check file permissions
- Ensure the path format is correct (use `\\` for UNC paths)

### Access denied errors
- Verify the service account has proper permissions
- For Docker, check volume mount permissions
- Test access manually from the server

### Mixed storage (some local, some network)
This is supported! The system detects the path type automatically:
- Paths starting with `\\` are treated as network paths
- Absolute paths (C:\, /mnt/) are treated as file system paths
- Relative paths (/setup-sheets/) are treated as web root paths

## Example Production Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sqlserver;Database=CncToolingDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
  },
  "FileStorage": {
    "SetupSheetsPath": "\\\\nas01.company.local\\manufacturing\\cnc-setup-sheets",
    "UseNetworkPath": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```
