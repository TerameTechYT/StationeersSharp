#region

using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using StationeersLibrary;
using StationeersLibrary.Args;

using StationeersLibrary.Modding;
using System.Reflection;

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
        Version = new Version(2, 0, 0, 562),
        WorkshopId = 3071950159ul,
        GameType = GameType.Client,
    };

    public Plugin() : base() => Plugin.Instance = this;

    public override void OnStart() { }

    public override UniTask OnBaseLoaded(SceneLoadArgs args) {
        try {
            Type type = Type.GetType("PlantsnNutritionRebalance.Scripts.MaxHydrationStoragePatch/PlayerStateWindowPatches, stationeers-PlantsnNutritionRebalance");
            if (type != null) {
                this.Harmony.Patch(type.GetMethod("PlayerStateWindowPatch", BindingFlags.Public | BindingFlags.Static),
                    new HarmonyMethod(typeof(PatchFunctions).GetMethod("PNNPatch", BindingFlags.Public | BindingFlags.Static)));
            }
        } catch (Exception ex) {
            this.LogException(ex);
        }

        return UniTask.CompletedTask;
    }

    public override void OnConfigLoad() {

        ConfigData.preferredPressureUnit = this.RegisterConfig(new ConfigData<PressureUnit>(
            PressureUnit.Pascal,
            "Units", "Preferred Pressure Unit",
            "Will change most things to use this unit of measurement."
        ));

        ConfigData.preferredTemperatureUnit = this.RegisterConfig(new ConfigData<TemperatureUnit>(
            TemperatureUnit.Celcius,
            "Units", "Preferred Temperature Unit",
            "Will change most things to use this unit of measurement."
        ));

        ConfigData.preferredVolumeUnit = this.RegisterConfig(new ConfigData<VolumeUnit>(
            VolumeUnit.Liter,
            "Units", "Preferred Volume Unit",
            "Will change most things to use this unit of measurement."
        ));

        ConfigData.preferredVelocityUnit = this.RegisterConfig(new ConfigData<VelocityUnit>(
            VelocityUnit.Meters,
            "Units", "Preferred Velocity Unit",
            "Will change most things to use this unit of measurement."
        ));

        ConfigData.customFramerate = this.RegisterConfig(new ConfigData<bool>(
            true,
            "Configurables", "CustomFramerate",
            "Should the framerate text only display FPS."
        ));

        ConfigData.changeFontSize = this.RegisterConfig(new ConfigData<bool>(
            true,
            "Configurables", "ChangeFontSize",
            "Should the font size be changed."  
        ));

        ConfigData.extraInfoPower = this.RegisterConfig(new ConfigData<bool>(
            true,
            "Configurables", "ExtraInfoPower",
            "Should a extra text label be placed next to the status like waste tank status."
        ));

        ConfigData.extraInfoFilter = this.RegisterConfig(new ConfigData<bool>(
            true,
            "Configurables", "ExtraInfoFilter",
            "Should a extra text label be placed next to the status like waste tank status."
        ));

        ConfigData.alwaysDisplaySanitation = this.RegisterConfig(new ConfigData<bool>(
            false,
            "Configurables", "AlwaysDisplaySanitation",
            "Should the sanitation status always be displayed."
        ));

        ConfigData.alwaysDisplayCognition = this.RegisterConfig(new ConfigData<bool>(
            false,
            "Configurables", "AlwaysDisplayCognition",
            "Should the cognition status always be displayed."
        ));

        ConfigData.alwaysDisplayHealth = this.RegisterConfig(new ConfigData<bool>(
            false,
            "Configurables", "AlwaysDisplayHealth",
            "Should the health status always be displayed."
        ));

        ConfigData.alwaysDisplayBodyHealth = this.RegisterConfig(new ConfigData<bool>(
            false,
            "Configurables", "AlwaysDisplayBodyHealth",
            "Should the body health status display/mannequin always be displayed."
        ));

        ConfigData.alwaysDisplayToxin = this.RegisterConfig(new ConfigData<bool>(
            false,
            "Configurables", "AlwaysDisplayToxin",
            "Should the toxin status always be displayed."
        ));

        ConfigData.numberPrecision = this.RegisterConfig(new ConfigData<int>(
            2,
            1,
            4,
            "Configurables", "NumberPrecision",
            "How many decimal points should be displayed on numbers."
        ));

        ConfigData.fontSize = this.RegisterConfig(new ConfigData<int>(
            21,
            14,
            28,
            "Configurables", "FontSize",
            "What font size should the labels be changed to."
        ));
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
    public static ConfigEntry<bool>? alwaysDisplayHealth;
    public static bool AlwaysDisplayHealth => alwaysDisplayHealth?.Value ?? false;

    //
    public static ConfigEntry<bool>? alwaysDisplayBodyHealth;
    public static bool AlwaysDisplayBodyHealth => alwaysDisplayBodyHealth?.Value ?? false;

    //
    public static ConfigEntry<bool>? alwaysDisplayCognition;
    public static bool AlwaysDisplayCognition => alwaysDisplayCognition?.Value ?? false;

    //
    public static ConfigEntry<bool>? alwaysDisplaySanitation;
    public static bool AlwaysDisplaySanitation => alwaysDisplaySanitation?.Value ?? false;

    //
    public static ConfigEntry<bool>? alwaysDisplayToxin;
    public static bool AlwaysDisplayToxin => alwaysDisplayToxin?.Value ?? false;

    //
    public static ConfigEntry<int>? numberPrecision;
    public static int NumberPrecision => numberPrecision?.Value ?? 0;

    //
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