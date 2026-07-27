namespace AbbRobots.Net.WebServices.Models;

public record RapidResource(
    string Name,
    string? Title = null,
    string? Type = null,
    string? Url = null
);

public record TasksResource(
    string Name,
    string? Type = null,
    string? TaskState = null,
    string? Excstate = null,
    string? Active = null,
    string? MotionTask = null,
    string? Url = null
);
/// <summary>
/// Represents a module
///  {
///            "_type": "rap-module-info-li",
///            "_title": "T_ROB1/Calib_t_pinzaC",
///            "name": "Calib_t_pinzaC",
///           "type": "ProgMod"
/// },
/// </summary>
/// <param name="name">Name of module</param>
/// <param name="title">Path to the module</param>
/// <param name="type">Type of module. ProgMod, SysMod</param>
public record ModuleResource(
    string Name,
    string? Title = null,
    string? Type = null
);
/// <summary>
/// Represent the answer of a GetModule
///  "_type": "rap-module-text",
///            "_title": "moduletext",
///            "change-count": " 20769 ",
///            "file-path": "/TEMP/pusres.307523",
///            "module-length": "1368"
/// </summary>
/// <param name="ChangeCount"></param>
/// <param name="ModuleText">Contents the module, if the module is small</param>
/// <param name="ModuleLength"></param>
/// <param name="FilePath">If the file is too long, here is where is stored</param>
public record TextModuleResource(
    string? Title=null,
    string? ChangeCount = null,
    string? ModuleText = null,
    string? ModuleLength = null,
    string? FilePath = null
);
