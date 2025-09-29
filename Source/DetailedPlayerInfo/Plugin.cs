#region

#endregion

namespace DetailedPlayerInfo;

public class Plugin : Mod<Plugin> {
    public static Plugin? Instance { get; private set; }

    public override bool UseLogger => true;
    public override bool UseConfig => true;
    public override bool UseHarmony => true;

    public override ModInfo Data => new ModInfo() {
        Name = "DetailedPlayerInfo",
        Guid = "detailedplayerinfo",
        Version = new Version(2, 0, 0, 341),
        WorkshopId = 3071950159ul,
        GameType = GameType.Client,
    };

    public Plugin() : base() => Plugin.Instance = this;

    public override void OnLoaded(List<GameObject> prefabs) => base.OnLoaded(prefabs);

    public override void OnStart() {}

    public override UniTask OnBaseLoaded(SceneLoadArgs args) {
        try {
            var type = Type.GetType("PlantsnNutritionRebalance.Scripts.MaxHydrationStoragePatch/PlayerStateWindowPatches, stationeers-PlantsnNutritionRebalance");

            this.Harmony.Patch(type.GetMethod("PlayerStateWindowPatch", BindingFlags.Public | BindingFlags.Static),
                new HarmonyMethod(typeof(PatchFunctions).GetMethod("PNNPatch", BindingFlags.Public | BindingFlags.Static)));
        } catch (Exception ex) {
            this.LogException(ex);
        }

        return UniTask.CompletedTask;
    }

    public override void OnConfigLoad() {
        ConfigData.preferredPressureUnit = Config.Bind(new ConfigDefinition("Units", "Preferred Pressure Unit"),
                             PressureUnit.Pascal,
                             new ConfigDescription("Will change most things to use this unit of measurement."));

        ConfigData.preferredTemperatureUnit = Config.Bind(new ConfigDefinition("Units", "Preferred Temperature Unit"),
                TemperatureUnit.Celcius,
                new ConfigDescription("Will change most things to use this unit of measurement."));

        ConfigData.preferredVolumeUnit = Config.Bind(new ConfigDefinition("Units", "Preferred Volume Unit"),
                VolumeUnit.Liter,
                new ConfigDescription("Will change most things to use this unit of measurement."));

        ConfigData.preferredVelocityUnit = Config.Bind(new ConfigDefinition("Units", "Preferred Velocity Unit"),
                VelocityUnit.Meters,
                new ConfigDescription("Will change most things to use this unit of measurement."));

        ConfigData.customFramerate = Config.Bind(new ConfigDefinition("Configurables", "CustomFramerate"),
                true,
                new ConfigDescription("Should the framerate text only display FPS."));

        ConfigData.changeFontSize = Config.Bind(new ConfigDefinition("Configurables", "ChangeFontSize"),
                true,
                new ConfigDescription("Should the font size be changed."));

        ConfigData.extraInfoPower = Config.Bind(new ConfigDefinition("Configurables", "ExtraInfoPower"),
                true,
                new ConfigDescription("Should a extra text label be placed next to the status like waste tank status."));

        ConfigData.extraInfoFilter = Config.Bind(new ConfigDefinition("Configurables", "ExtraInfoFilter"),
                true,
                new ConfigDescription("Should a extra text label be placed next to the status like waste tank status."));

        ConfigData.numberPrecision = Config.Bind(new ConfigDefinition("Configurables", "NumberPrecision"),
                2,
                new ConfigDescription("How many decimal points should be displayed on numbers.",
                new AcceptableValueRange<int>(1, 4)));

        ConfigData.fontSize = Config.Bind(new ConfigDefinition("Configurables", "FontSize"),
                21,
                new ConfigDescription("What font size should the labels be changed to.",
                new AcceptableValueRange<int>(14, 28)));
    }
}

internal struct ConfigData {
    //
    public static ConfigEntry<PressureUnit>? preferredPressureUnit;
    public static PressureUnit PreferredPressureUnit => preferredPressureUnit?.Value ?? PressureUnit.Pascal;

    //
    public static ConfigEntry<TemperatureUnit>? preferredTemperatureUnit;
    public static TemperatureUnit PreferredTemperatureUnit => preferredTemperatureUnit?.Value ?? TemperatureUnit.Celcius;

    //
    public static ConfigEntry<VolumeUnit>? preferredVolumeUnit;
    public static VolumeUnit PreferredVolumeUnit => preferredVolumeUnit?.Value ?? VolumeUnit.Liter;

    //
    public static ConfigEntry<VelocityUnit>? preferredVelocityUnit;
    public static VelocityUnit PreferredVelocityUnit => preferredVelocityUnit?.Value ?? VelocityUnit.Meters;

    //
    public static ConfigEntry<bool>? customFramerate;
    public static bool CustomFramerate => customFramerate?.Value ?? false;

    //
    public static ConfigEntry<bool>? changeFontSize;
    public static bool ChangeFontSize => changeFontSize?.Value ?? false;

    //
    public static ConfigEntry<int>? fontSize;
    public static int FontSize => ChangeFontSize ? (fontSize?.Value ?? 21) : 21;

    //
    public static ConfigEntry<bool>? extraInfoPower;
    public static bool ExtraInfoPower => extraInfoPower?.Value ?? false;

    //
    public static ConfigEntry<bool>? extraInfoFilter;
    public static bool ExtraInfoFilter => extraInfoFilter?.Value ?? false;

    //
    public static ConfigEntry<int>? numberPrecision;
    public static int NumberPrecision => numberPrecision?.Value ?? 0;

    public const string ExternalTemperatureUnit =
            "GameCanvas/PanelStatusInfo/PanelExternalNavigation/PanelExternal/PanelTemp/ValueTemp/TextUnitTemp";

    public const string InternalTemperatureUnit =
            "GameCanvas/PanelStatusInfo/PanelVerticalGroup/Internals/PanelInternal/PanelTemp/ValueTemp/TextUnitTemp";

    public const string ExternalPressureUnit =
            "GameCanvas/PanelStatusInfo/PanelExternalNavigation/PanelExternal/PanelPressure/TextUnitPressure";

    public const string InternalPressureUnit =
            "GameCanvas/PanelStatusInfo/PanelVerticalGroup/Internals/PanelInternal/PanelPressure/TextUnitPressure";

    public const string JetpackPressureUnit =
    "GameCanvas/PanelStatusInfo/PanelVerticalGroup/PanelJetpack/PanelPressureDelta/TextUnitPressure";

    public const string NavagationVelocityUnit =
            "GameCanvas/PanelStatusInfo/PanelExternalNavigation/PanelExternal/PanelNavigation/PanelVelocity/ValueVelocity/TextUnitVelocity";

    public const string WasteTextPanel =
            "GameCanvas/StatusIcons/Waste/Panel";

    public const string BatteryStatus =
            "GameCanvas/StatusIcons/Power";

    public const string FilterStatus =
            "GameCanvas/StatusIcons/Filter";
}