namespace AbbRobots.Net.WebServices.Models;

public record  RapidResource(
    string Name,
    string? Title = null,
    string? Type = null,
    string? Url = null
);

public record TasksResource(
    string Name,
    string? Type= null,
    string? TaskState=null,
    string? Excstate =null,
    string? Active = null,
    bool? MotionTask = null
);