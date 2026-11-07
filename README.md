# File Monitor Service

A Windows Service built with C# and .NET Framework 4.8 that continuously monitors a specified folder for new files.

When a new file is detected, the service:
- Generates a unique GUID-based filename.
- Preserves the original file extension.
- Moves the file to a destination folder.
- Records events and errors in daily log files.

## Technologies Used

- C#
- .NET Framework 4.8
- Windows Services
- FileSystemWatcher
- Windows Service Control Manager (SCM)

## Prerequisites

- Windows operating system.
- .NET Framework 4.8.
- Visual Studio (to build the project).
- Administrator privileges (to install the service).

## Configuration

The service uses `App.config` to configure its folders.

Update the following settings according to your requirements:

```xml
<appSettings>
    <add key="SourceFolder" value="C:\FileMonitor\Incoming" />
    <add key="DestinationFolder" value="C:\FileMonitor\Processed" />
    <add key="LogFolder" value="C:\FileMonitor\Logs" />
</appSettings>
```

| Setting | Description |
|---|---|
| SourceFolder | Folder monitored for new files |
| DestinationFolder | Folder where processed files are moved |
| LogFolder | Folder where daily log files are stored |

**Note:** After building the project, the configuration file is generated as `FileMonitorService.exe.config` beside the executable. The service reads this configuration file when running.

## Installation

### 1. Build the Project

Open the solution in Visual Studio and build it using the **Release** configuration.

Locate the compiled executable and its configuration file inside the Release output folder.

### 2. Install the Service

Open Command Prompt as Administrator and execute:

```cmd
sc.exe create FileMonitorService binPath= "\"C:\Path\To\FileMonitorService.exe\"" start= auto
```

Replace the example path with the actual path to your executable.

### 3. Start the Service

```cmd
sc.exe start FileMonitorService
```

### 4. Check Service Status

```cmd
sc.exe query FileMonitorService
```

A successful running service should show:

```text
STATE : 4 RUNNING
```

### 5. Stop the Service

```cmd
sc.exe stop FileMonitorService
```

### 6. Uninstall the Service

```cmd
sc.exe delete FileMonitorService
```

**Important:** The Windows service account must have permission to access the source, destination, and log folders.

## How It Works

1. The Windows Service starts in the background.
2. `FileSystemWatcher` monitors the configured source folder.
3. The service detects newly created or renamed files.
4. It generates a unique filename using a GUID.
5. It preserves the original file extension.
6. It moves the file into the destination folder.
7. If a file is temporarily locked, the service retries the operation up to 10 times.

## Logging

The service creates a daily log file inside the configured log folder.

Example:

```text
Logs/
└── 2026-11-07.txt
```

Logs include:
- Service startup and shutdown.
- New file detection.
- Successful file processing.
- Processing errors.
