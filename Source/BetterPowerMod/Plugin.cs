#region

#endregion

namespace BetterPowerMod;

public class Plugin : Mod {
    public static Plugin Instance { get; private set; }

    public override bool UseConfig => true;
    public override bool UseHarmony => true;
    protected override LaunchPadBooster.Mod InternalMod => new(this.ModGuid, this.ModVersionString);

    public override ModInfo Data => new ModInfo() {
        Name = "BetterPowerMod",
        Guid = "betterpowermod",
        Version = new Version(1, 4, 0),
        WorkshopId = 3234916147ul,
        GameType = GameType.Both,
    };

    public Plugin() => Plugin.Instance = this;

    public override void OnLoadConfiguration() {
        ConfigData.enableSolarPanel = Config.Bind(
                new ConfigDefinition("Configurables", "Solar Panel Patches"),
                true,
                new ConfigDescription("Should the max power output be set to the worlds Solar Irradiance"
        ));

        ConfigData.enableWindTurbine = Config.Bind(
                new ConfigDefinition("Configurables", "Wind Turbine Patches"),
                true,
                new ConfigDescription("Should the max power output be set higher based on the atmospheric pressure")
        );

        ConfigData.enableTurbine = Config.Bind(
                new ConfigDefinition("Configurables", "Wall Turbine Patches"),
                true,
                new ConfigDescription("Should the max power output be multipled by 10")
         );

        ConfigData.enableStirling = Config.Bind(
                new ConfigDefinition("Configurables", "Stirling Patches"),
                true,
                new ConfigDescription($"Should the max power output be changed to Stirling Energy Output")
        );

        ConfigData.stirlingEnergy = Config.Bind(
                new ConfigDefinition("Configurables", "Stirling Energy Output"),
                Constants.TWENTY_KILOWATTS,
                new ConfigDescription("The max power output of the Stirling Engine",
                new AcceptableValueRange<float>(Constants.EIGHT_KILOWATTS, Constants.TWENTY_FIVE_KILOWATTS)
        ));

        ConfigData.enableFasterCharging = Config.Bind(
                new ConfigDefinition("Configurables", "Charging Patches"),
                true,
                new ConfigDescription("Should the max input power of (Area Power Controller, Small and Large Battery Charger, Omni Power Transmitter) be set to Fast Charge Rate")
        );

        ConfigData.fastChargeRate = Config.Bind(
                new ConfigDefinition("Configurables", "Fast Charging Charging Rate"),
                Constants.TWO_POINT_FIVE_KILOWATTS,
                new ConfigDescription("The max input power of the (Area Power Controller, Small and Large Battery Charger, Omni Power Transmitter)",
                new AcceptableValueRange<float>(1f, Constants.FIVE_KILOWATTS)
        ));

        ConfigData.turbineMultiplier = Config.Bind(
                new ConfigDefinition("Configurables", "Turbine Power Multiplier"),
                10f,
                new ConfigDescription("The power output on the Turbine Generator (not wind turbine, the one that looks like a wall)",
                new AcceptableValueRange<float>(1f, 25f)
        ));
    }

    public override void OnAwake() { }
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
    public static ConfigEntry<bool> enableSolarPanel;
    public static bool EnableSolarPanel => enableSolarPanel?.Value ?? false;

    //
    public static ConfigEntry<bool> enableWindTurbine;
    public static bool EnableWindTurbine => enableWindTurbine?.Value ?? false;

    //
    public static ConfigEntry<bool> enableTurbine;
    public static bool EnableTurbine => enableTurbine?.Value ?? false;

    //
    public static ConfigEntry<bool> enableStirling;
    public static bool EnableStirling => enableStirling?.Value ?? false;

    public static ConfigEntry<float> stirlingEnergy;
    public static float StirlingEnergy => stirlingEnergy?.Value ?? Constants.TWENTY_KILOWATTS;

    //
    public static ConfigEntry<bool> enableFasterCharging;
    public static bool EnableFasterCharging => enableWindTurbine?.Value ?? false;

    //
    public static ConfigEntry<float> fastChargeRate;
    public static float FastChargeRate => fastChargeRate?.Value ?? Constants.TWO_POINT_FIVE_KILOWATTS;

    //
    public static ConfigEntry<float> turbineMultiplier;
    public static float TurbineMultiplier => turbineMultiplier?.Value ?? 10f;
}