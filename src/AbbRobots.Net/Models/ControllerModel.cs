using System;
using System.Dynamic;
using System.Text.Json.Serialization;

namespace AbbRobots.Net.Models;




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