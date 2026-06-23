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
    string Default = "",
    string Invert = "",
    string SafeLevel="",
    string FiltAct="",
    string FiltPas=""
);

// 💡
public record CrossConnectionItem(
    string Name,
    string Result,
    string Actor1,
    string Operator1 = "",
    string InvertActor1="",
    string Actor2 = "",
    string Operator2 = "",
    string InvertActor2="",
    string Actor3 = "",
    string Operator3 = "",
    string InvertActor3="",
    string Actor4 = "",
    string Operator4="",
    string InvertActor4="",
    string Actor5="",   
    string InvertActor5=""
);
  
   