#region

#endregion

namespace StationeersLibrary;

public enum DependencyType {
    /// <summary>
    /// There is no dependency.
    /// </summary>
    None,

    /// <summary>
    /// This dependency is optional.
    /// </summary>
    Soft,

    /// <summary>
    /// This dependency is required.
    /// </summary>
    Hard,
}

public enum GameType {
    Client,
    Server,
    Both,
}


[Flags]
public enum SystemType {
    None = 0,
    Windows = 1,
    Linux = 2,
    Macintosh = 4,
    Any = 8,
}

public enum GameBranch {
    None,
    Previous,
    Beta,
    DevBranch,
    PreRocket,
    PreTerrain,
}

public enum EnergyUnit {
    Joule,
    FootPound,
    Calorie,
}

public enum PressureUnit {
    Pascal,
    PSI,
    Bar,
    Torr,
}

public enum PascalUnits {
    MicroPascal,
    MilliPascal,
    Pascal,
    KiloPascal,
    MegaPascal,
    GigaPascal,
}

public enum TemperatureUnit {
    Kelvin,
    Celcius,
    Fahrenheit,
}

public enum VelocityUnit {
    Meters,
    Miles,
    Knot,
}
public enum VolumeUnit {
    Liter,
    USGallon,
    ImperialGallon,
}