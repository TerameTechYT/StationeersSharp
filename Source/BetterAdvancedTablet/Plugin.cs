#region

using Assets.Scripts.Objects;
using BepInEx.Configuration;
using StationeersLibrary;
using StationeersLibrary.Modding;

#endregion

namespace BetterAdvancedTablet;

public class Plugin : Mod<Plugin> {
    public static Plugin? Instance {
        get; private set;
    }

    public override bool UseLogger => true;
    public override bool UseConfig => true;
    public override bool UseHarmony => true;

    public override ModInfo Data => new ModInfo() {
        Name = "BetterAdvancedTablet",
        Guid = "betteradvancedtablet",
        Version = new Version(1, 3, 0, 566),
        WorkshopId = 3523321721ul,
        GameType = GameType.Both,
    };

    public Plugin() : base() => Plugin.Instance = this;

    public override void OnStart() {
        Prefab.OnPrefabsLoaded += Functions.PrefabsLoaded;
    }

    public override void OnConfigLoad() {
        ConfigData.additionalTabletSlots = this.RegisterConfig(new ConfigData<int>(
            2,
            "Configurables", "Additonal Tablet Slots",
            "How many additional cartridge slots do you want to add to the advanced tablet?",
            new AcceptableValueRange<int>(0, 6)
        ));

        ConfigData.preventInventoryScrollWithTablet = this.RegisterConfig(new ConfigData<bool>(
            false,
            "Configurables", "Prevent Inventory Scroll With Tablets",
            "If true, the inventory menu will not scroll when tablet is in main hand and on."
        ));
    }
}

internal struct ConfigData {
    //
    public const string AdvancedTabletPrefabName = "ItemAdvancedTablet";

    /*public const string NextCartridge = "Next Cartridge";
    public const string PrevCartridge = "Previous Cartridge";*/

    // Config
    public static ConfigEntry<int>? additionalTabletSlots;
    public static int AdditionalTabletSlots => additionalTabletSlots?.Value ?? 2;

    public static ConfigEntry<bool>? preventInventoryScrollWithTablet;
    public static bool PreventInventoryScrollWithTablet => preventInventoryScrollWithTablet?.Value ?? false;
}