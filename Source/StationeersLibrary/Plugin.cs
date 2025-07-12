#region

#endregion

namespace StationeersLibrary;

public class Plugin : Mod {
    public static Plugin? Instance { get; private set; }

    public override bool UseConfig => true;
    public override bool UseHarmony => false;

    public override ModInfo Data => new ModInfo() {
        Name = "StationeersLibrary",
        Guid = "stationeerslibrary",
        Version = new Version(2, 0, 0, 53),
        WorkshopId = 3389894703ul,
        GameType = GameType.Both,
    };

    public Plugin() => Plugin.Instance = this;

    public override void Start() { }
}

internal struct ConfigData { }