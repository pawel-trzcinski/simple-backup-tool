# simple-backup-tool
Simple console application to create backups of your files.

# Configuration
Application is configured with appsettings.json file only.
Structure:
```
{
  "SimpleBackupConfiguration": {
    "BackupPipelines": [  ## Array of pipelines. Each pipeline compresses files and folders into common output archive folder.
      {
        "Name": "name1",  ## Name of the pipeline.
        "Enabled": "true", ## Should pipeline be executed.
        "RefreshThreshold": "8.00:00:00", ## If last finished execution of the pipeline is not old enough, execution will be disabled.
        "Sources": [ ## Array of files and folders to compress
          "d:\\Nc",
          ...
        ],
        "Compression": "Adaptive", ## Type of compression used: Adaptive, Minimal, Normal, Best.
        "BackupOutputFolder": "d:\\Backup_E\\Test2", ## Folder in which archive folders will be created.
        "RemoveOldArchive": "true" ## Should old archives be deleted after this pipeline finishes.
      },
      ...
    ],

    "LogMinimumLevel": "Information", ## Logger minimal verbosity level: Verbose, Debug, Information, Warning, Error, Fatal.
    "TestRun": "false" ## Should data be changed.
  }
}
```

# How to use
* Build
* Configure using appsetting.json
* dotnet run SimpleBackup.dll OR SimpleBackup.exe