#region

using BepInEx.Configuration;
using StationeersLibrary;

using StationeersLibrary.Modding;

#endregion

namespace BetterPowerMod;

public class Plugin : Mod<Plugin> {
    public static Plugin? Instance { get; private set; }

    public override bool UseLogger => true;
    public override bool UseConfig => true;
    public override bool UseHarmony => true;

    public override ModInfo Data => new ModInfo() {
        Name = "BetterPowerMod",
        Guid = "betterpowermod",
        Version = new Version(1, 6, 0, 566),
        WorkshopId = 3234916147ul,
        GameType = GameType.Both,
    };

    public Plugin() : base() => Plugin.Instance = this;
    public override void OnStart() { }

    public override void OnConfigLoad() {
        ConfigData.enableSolarPanel = this.RegisterConfig(new ConfigData<bool>(
            true,
            "Configurables", "Solar Panel Patches",
            "Should the max power output be set to the worlds Solar Irradiance"
        ));

        ConfigData.enableWindTurbine = this.RegisterConfig(new ConfigData<bool>(
            true,
            "Configurables", "Wind Turbine Patches",
            "Should the max power output be set higher based on the atmospheric pressure"
        ));

        ConfigData.enableTurbine = this.RegisterConfig(new ConfigData<bool>(
            true,
            "Configurables", "Wall Turbine Patches",
            "Should the max power output be multipled by 10"
        ));

        ConfigData.enableStirling = this.RegisterConfig(new ConfigData<bool>(
            true,
            "Configurables", "Stirling Patches",
            "Should the max power output be changed to Stirling Energy Output"
        ));

        ConfigData.stirlingEnergy = this.RegisterConfig(new ConfigData<float>(
            Constants.TWENTY_KILOWATTS,
            "Configurables", "Stirling Energy Output",
            "The max power output of the Stirling Engine",
            new AcceptableValueRange<float>(Constants.EIGHT_KILOWATTS, Constants.TWENTY_FIVE_KILOWATTS)
        ));

        ConfigData.enableFasterCharging = this.RegisterConfig(new ConfigData<bool>(
            true,
            "Configurables", "Charging Patches",
            "Should the max input power of (Area Power Controller, Small and Large Battery Charger, Omni Power Transmitter) be set to Fast Charge Rate"
        ));

        ConfigData.fastChargeRate = this.RegisterConfig(new ConfigData<float>(
            Constants.TWO_POINT_FIVE_KILOWATTS,
            "Configurables", "Fast Charging Charging Rate",
            "The max input power of the (Area Power Controller, Small and Large Battery Charger, Omni Power Transmitter)",
            new AcceptableValueRange<float>(1f, Constants.FIVE_KILOWATTS)
        ));

        ConfigData.turbineMultiplier = this.RegisterConfig(new ConfigData<float>(
            10f,
            "Configurables", "Turbine Power Multiplier",
            "(THIS OBJECT WAS REMOVED FROM THE GAME, LEFT FOR COMPATIBILITY) The power output on the Turbine Generator (not wind turbine, the one that looks like a wall)",
            new AcceptableValueRange<float>(1f, 25f)
        ));
    }
}

internal struct ConfigData {
    //
    public const string BatteryChargerSmall = "StructureBatteryChargerSmall";

    //
    public static List<string> FlatSolarPanelPrefabs => [
        "StructureSolarPanelFlat", "StructureSolarPanel45", "StructureSolarPanelFlatReinforced", "StructureSolarPanel45Reinforced"
    ];

    //
    public static List<string> WindTurbinePrefabs => [
            "StructureUprightWindTurbine", "StructureWindTurbine",
    ];

    //
    public static ConfigEntry<bool>? enableSolarPanel;
    public static bool EnableSolarPanel => enableSolarPanel?.Value ?? false;

    //
    public static ConfigEntry<bool>? enableWindTurbine;
    public static bool EnableWindTurbine => enableWindTurbine?.Value ?? false;

    //
    public static ConfigEntry<bool>? enableTurbine;
    public static bool EnableTurbine => enableTurbine?.Value ?? false;

    //
    public static ConfigEntry<bool>? enableStirling;
    public static bool EnableStirling => enableStirling?.Value ?? false;

    public static ConfigEntry<float>? stirlingEnergy;
    public static float StirlingEnergy => stirlingEnergy?.Value ?? Constants.TWENTY_KILOWATTS;

    //
    public static ConfigEntry<bool>? enableFasterCharging;
    public static bool EnableFasterCharging => enableWindTurbine?.Value ?? false;

    //
    public static ConfigEntry<float>? fastChargeRate;
    public static float FastChargeRate => fastChargeRate?.Value ?? Constants.TWO_POINT_FIVE_KILOWATTS;

    //
    public static ConfigEntry<float>? turbineMultiplier;
    public static float TurbineMultiplier => turbineMultiplier?.Value ?? 10f;
}