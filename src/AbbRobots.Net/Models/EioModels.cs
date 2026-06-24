namespace AbbRobots.Net.Models;



public record SignalItem(
    string Name,
    string SignalType,
    string Device,
    string DeviceMap,
    string Label = "",
    string Category = "",
    string Access = "",
    string DefaultValue = "",
    string Invert = "",
    string SafeLevel = "",
    string FiltAct = "",
    string FiltPas = "",
    string EncType = "",
    string MaxBitVal = "",
    string MaxLog = "",
    string MaxPhys = "",
    string MaxPhysLimit = "",
    string MinBitVal = "",
    string MinLog = "",
    string MinPhys = "",
    string MinPhysLimit = "",
    string Size = ""
);

// 💡
public record CrossConnectionItem(
    string Name,
    string Result,
    string Actor1,
    string Operator1 = "",
    string InvertActor1 = "",
    string Actor2 = "",
    string Operator2 = "",
    string InvertActor2 = "",
    string Actor3 = "",
    string Operator3 = "",
    string InvertActor3 = "",
    string Actor4 = "",
    string Operator4 = "",
    string InvertActor4 = "",
    string Actor5 = "",
    string InvertActor5 = ""
);

public record NetWorkItem(
  string Address,
  string GateWay,
  string IndicationLable,
  string Simulated,
  string SubnetMast
);

public record AccessLeveItem(
    string LocalAuto,
    string LocalManual,
    string Nombre,
    string Rapid,
    string RemoteAuto,
    string RemoteManual
);

public record SystemOutputItem(
    string Signal,
    string Status,
    string Arg2,
    string Arg3,
    string Arg4,
    string Arg5,
    string Arg6,
    string Arg7,
    string Arg8
);

public record SystemInputItem(
    string Action,
    string Signal,
    string Arg1,
    string Arg2,
    string Arg3,
    string Arg4,
    string Arg5,
    string Arg6,
    string Arg7,
    string Arg8,
    string Arg9
);
