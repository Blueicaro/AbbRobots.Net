using System;

namespace AbbRobots.Net.Models;

/// <summary>
/// It represents the general status of the OmniCore controller.
/// </summary>
public class ControllerModel
{
    public string ControllerName{get;set;} = string.Empty;
    public string RobotWareVersion{get;set;} = string.Empty;
    public string CtrlState{get;set;}=string.Empty;
}

public class ControllerStateChangeEventArgs:EventArgs
{
    public string CtrlState{get;set;}=string.Empty;
    public string Mode{get;set;}=string.Empty;
}