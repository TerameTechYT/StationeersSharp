#region

#endregion

namespace BetterAdvancedTablet;

public class Plugin : Mod {
    public static Plugin Instance {
        get; private set;
    }

    public override bool UseConfig => true;
    public override bool UseHarmony => true;


    public override ModInfo Data => new ModInfo() {
        Name = "BetterAdvancedTablet",
        Guid = "betteradvancedtablet",
        Version = new Version(1, 1, 0),
        WorkshopId = 3523321721ul,
        GameType = GameType.Both,
    };

    public Plugin() => Plugin.Instance = this;

    public override void OnLoadConfiguration() => ConfigData.additionalTabletSlots = this.Config.Bind(
                    new ConfigDefinition("Configurables", "Additonal Tablet Slots"),
                    2,
                    new ConfigDescription("How many additional cartridge slots do you want to add to the advanced tablet?",
                    new AcceptableValueRange<int>(0, 6))
            );

    public override void OnAwake() { }
}

internal struct ConfigData {
    //
    public const string AdvancedTabletPrefabName = "ItemAdvancedTablet";

    /*public const string NextCartridge = "Next Cartridge";
    public const string PrevCartridge = "Previous Cartridge";*/

    // Config
    public static ConfigEntry<int> additionalTabletSlots;
    public static int AdditionalTabletSlots = additionalTabletSlots?.Value ?? 2;
}