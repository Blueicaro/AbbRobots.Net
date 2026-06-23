namespace AbbRobots.Net.Models;
namespace AbbRobots.Net.Models;

// 
public record SignalItem(
    string Name,
    string SignalType,
    string Device,
    string DeviceMap,
    string Label = "",
    string Category = "",
    string Access = "",
    string DefaultValue = "",
    string Invert = ""
);

// 💡
public record CrossConnectionItem(
    string Name,
    string Result,
    string Actor1,
    string Operator1 = "",
    string Actor2 = "",
    string Operator2 = "",
    string Actor3 = ""
);
  
   