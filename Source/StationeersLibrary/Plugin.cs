#region

#endregion

namespace StationeersLibrary;

public class Plugin : Mod {
    public static Plugin Instance { get; private set; }

    public override bool UseConfig => true;
    public override bool UseHarmony => true;


    public override ModInfo Data => new ModInfo() {
        Name = "StationeersLibrary",
        Guid = "stationeerslibrary",
        Version = new Version(1, 2, 0),
        WorkshopId = 3389894703ul,
        GameType = GameType.Both,
    };


    public Plugin() => Plugin.Instance = this;

    public override void OnAwake() { }
}

internal struct ConfigData {
    //
    public static ConfigEntry<bool> debugMode;
    public static bool DebugMode => debugMode?.Value ?? false;
}