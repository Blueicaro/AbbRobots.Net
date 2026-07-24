using System;
using System.Dynamic;
using System.Text.Json.Serialization;

namespace AbbRobots.Net.WebServices.Models;


public enum BackupStatus
{
    Pending,
    Executing,
    Completed,
    Failed
}

public class BackupProgressEventArgs : EventArgs
{
    public required string BackupName { get; init; }
    public BackupStatus Status { get; init; }
    public int ProgressPercentage { get; init; } // 0 - 100%
    public string CurrentStep { get; init; } = string.Empty; // ej: "Archiving RAPID modules", "Compressing..."
    public string? ErrorMessage { get; init; }
}
public class BackupStateResponseModel
{
    [JsonPropertyName("state")]
    public List<BackupStateModel> State { get; set; } = new();
}
public class BackupResult
{
    public required string BackupName { get; init; }
    public bool Success { get; init; }
    public string? BackupPath { get; init; }
    public TimeSpan Duration { get; init; }
    public string? ErrorDetails { get; init; }
}


public class ControllerStateChangeEventArgs:EventArgs
{
    public string CtrlState{get;set;}=string.Empty;
    public string Mode{get;set;}=string.Empty;
}
public class ControllerModel
{
    public string ControllerName{get;set;} = string.Empty;
    public string RobotWareVersion{get;set;} = string.Empty;
    public string CtrlState{get;set;}=string.Empty;
}

public class BackupStateChangeEventArgs:EventArgs
{
    public string state{get;set;}=string.Empty;
    public string code{get;set;}=string.Empty;
}
public class BackupStateModel
{
    [JsonPropertyName("backup-state")]
    public string backupstate{get;set;}=string.Empty;
}

public class CreateBackupRequestModel
{
    [JsonPropertyName("backup-name")]
    public string BackupName {get;set;}=string.Empty;
    [JsonPropertyName("backup-path")]
    public string? BackupPath {get;set;}
}