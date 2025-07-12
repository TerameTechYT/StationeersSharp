#region

#endregion

namespace StationpediaCalculator;

public class Plugin : Mod {
    public static Plugin Instance { get; private set; }

    public override bool UseConfig => true;
    public override bool UseHarmony => true;


    public override ModInfo Data => new ModInfo() {
        Name = "StationpediaCalculator",
        Guid = "stationpediacalculator",
        Version = new Version(1, 5, 0, 3),
        WorkshopId = 3305312105ul,
        GameType = GameType.Client,
    };

    public Plugin() => Plugin.Instance = this;

    public override void OnAwake() { }
}

internal struct ConfigData {
    public static SPDAListItem CalculatorItem;
}


