#region

#endregion

namespace LoulanRevived;

public class Plugin : Mod {
    public static Plugin Instance { get; private set; }

    public override bool UseConfig => true;
    public override bool UseHarmony => true;


    public override ModInfo Data => new ModInfo() {
        Name = "LoulanRevived",
        Guid = "loulanrevived",
        Version = new Version(1, 2, 0),
        WorkshopId = 3255025164ul,
        GameType = GameType.Client,
    };

    public Plugin() => Plugin.Instance = this;

    public override void OnAwake() { }
}

internal struct ConfigData {
    //
    //public static ConfigEntry<bool> spawnWrecks;
    //public static bool SpawnWrecks => spawnWrecks?.Value ?? false;
}