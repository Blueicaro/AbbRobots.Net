using System.Dynamic;
using System.Runtime.Versioning;
using System.Text.Json.Serialization;

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
    string? Title = null,
    string? TaskState = null,
    string? Excstate = null,
    string? Active = null,
    string? MotionTask = null,
    string? Url = null
);



public record ModuleResource(
    string? Title = null,
    string? ChangeCount = null,
    string? ModuleText = null,
    string? ModuleLength = null,
    string? FilePath = null
);

public record ModulesResource(
    string Name,
    string? Type,
    string? Title
);

// --- Modelos internos de deserialización ---
#region ModulesResponseModel
internal record ModulesResponseModel
{
    [JsonPropertyName("state")]
    public  List <ModuleItemModel>?State {get;init;}
    public List<ModuleItemModel> Items=>State??[];
}

//   "_type": "rap-module-info-li",
//             "_title": "T_ROB1/Sistema",
//             "name": "Sistema",
//             "type": "SysMod"

internal record ModuleItemModel
{
    [JsonPropertyName("name")]
    public string? Name {get; set;}
    [JsonPropertyName("Type")]
    public string? Type{get; set;}
    [JsonPropertyName("_title")]
    public string? Title{get;set;}

    public string ResolvedName => Name?? Title?? string.Empty;
}


#endregion

#region  TaskResponseModel
internal record TasksResponseModel
{
    [JsonPropertyName("_embedded")]
    public TasksEmbeddedModel? Embedded { get; init; }
}

internal record TasksEmbeddedModel
{
    [JsonPropertyName("resources")]
    public List<TaskResourceModel>? Resources { get; init; }

    public List<TaskResourceModel> Items => Resources ?? [];
}

internal record TaskResourceModel
{
    [JsonPropertyName("_links")]
    public TaskLinksModel? Links { get; init; }

    [JsonPropertyName("_type")]
    public string? ResourceType { get; init; }

    [JsonPropertyName("_title")]
    public string? Title { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("taskstate")]
    public string? TaskState { get; init; }

    [JsonPropertyName("excstate")]
    public string? ExcState { get; init; }

    [JsonPropertyName("active")]
    public string? Active { get; init; }

    [JsonPropertyName("motiontask")]
    public string? MotionTask { get; init; }

    public string ResolvedName => Name ?? Title ?? Links?.Self?.Href ?? string.Empty;
}

internal record TaskLinksModel
{
    [JsonPropertyName("self")]
    public LinkModel? Self { get; init; }
}

internal record LinkModel
{
    [JsonPropertyName("href")]
    public string? Href { get; init; }
}
#endregion