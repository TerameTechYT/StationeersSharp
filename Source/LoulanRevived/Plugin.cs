#region

#endregion

namespace LoulanRevived;

public class Plugin : Mod {
    public static Plugin? Instance { get; private set; }

    public override bool UseLogger => true;
    public override bool UseConfig => true;
    public override bool UseHarmony => true;

    public override ModInfo Data => new ModInfo() {
        Name = "LoulanRevived",
        Guid = "loulanrevived",
        Version = new Version(1, 2, 0, 93),
        WorkshopId = 3255025164ul,
        GameType = GameType.Client,
    };

    public Plugin() => Plugin.Instance = this;

    public override void OnStart() {}
}

internal struct ConfigData {
    //
    //public static ConfigEntry<bool> spawnWrecks;
    //public static bool SpawnWrecks => spawnWrecks?.DefaultValue ?? false;
}