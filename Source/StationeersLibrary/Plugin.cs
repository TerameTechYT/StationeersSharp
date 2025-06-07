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
        GameType = GameType.Client | GameType.Server,
    };

    public Plugin() => Plugin.Instance = this;

    public override void OnLoadConfiguration() {
        ConfigData.debugMode = this.Config.Bind(
            new ConfigDefinition("Debug", "Enable Debugging Mode"),
            false,
            new ConfigDescription("Should StationeersLibrary mods enable debug mode? enables extra logging for debugging, may fill log files.")
        );
    }

    public override void OnAwake() { }
}

internal struct ConfigData {
    //
    public static ConfigEntry<bool> debugMode;
    public static bool DebugMode => debugMode?.Value ?? false;
}