#region

using HarmonyLib.Tools;
using static BepInEx.BepInDependency;
using MainMenuUI = Assets.Scripts.UI.MainMenu;

#endregion

namespace DetailedPlayerInfo;

[BepInPlugin(Data.ModGuid, Data.ModName, Data.ModVersion)]
[BepInDependency(Constants.STATIONEERS_LIBRARY_GUID, DependencyFlags.HardDependency)]
[BepInProcess(Constants.CLIENT_EXECUTABLE_NAME)]
public class Plugin : BaseUnityPlugin {
    public static Plugin Instance {
        get; private set;
    }

    public static Harmony HarmonyInstance {
        get; private set;
    }

    public static ManualLogSource LoggerInstance {
        get => Plugin.Instance.Logger;
    }

    [UsedImplicitly]
    public void Awake() {
        Plugin.LogDebug("Mod Started.");
        if (Utilities.IsLoaded(Data.ModGuid)) {
            throw new AlreadyLoadedException(Data.ModName, Data.ModGuid, Data.ModVersion);
        }

        Plugin.LogDebug("Loading configuration.");
        this.LoadConfiguration();

        Plugin.Instance = this;
        HarmonyFileLog.Enabled = Constants.DEBUG_MODE;
        Plugin.HarmonyInstance = new Harmony(Data.ModGuid);

        Plugin.LogDebug($"Harmony patch starting.");
        try {
            Plugin.HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
        catch (HarmonyException ex) {
            Plugin.LogException(ex);
            Plugin.LogError($"Harmony failed to patch! Please press {Utilities.GetConsoleKeyCode()} and run 'slib report'!");
        }
        finally {
            Plugin.LogDebug($"Harmony patch finished.");
        }

        // Thx jixxed for awesome code :)
        SceneManager.sceneLoaded += (scene, _) => {
            if (scene.name == Constants.BASE_SCENE_NAME) {
                this.OnBaseLoaded().Forget();
            }
        };
    }

    public void LoadConfiguration() {
        Data.preferredPressureUnit = Config.Bind(new ConfigDefinition("Units", "Preferred Pressure Unit"),
            PressureUnit.Pascal,
            new ConfigDescription("Will change most things to use this unit of measurement."));

        Data.preferredTemperatureUnit = Config.Bind(new ConfigDefinition("Units", "Preferred Temperature Unit"),
            TemperatureUnit.Celcius,
            new ConfigDescription("Will change most things to use this unit of measurement."));

        Data.preferredVolumeUnit = Config.Bind(new ConfigDefinition("Units", "Preferred Volume Unit"),
            VolumeUnit.Liter,
            new ConfigDescription("Will change most things to use this unit of measurement."));

        Data.preferredVelocityUnit = Config.Bind(new ConfigDefinition("Units", "Preferred Velocity Unit"),
            VelocityUnit.Meters,
            new ConfigDescription("Will change most things to use this unit of measurement."));

        Data.customFramerate = Config.Bind(new ConfigDefinition("Configurables", "CustomFramerate"),
            true,
            new ConfigDescription("Should the framerate text only display FPS."));

        Data.changeFontSize = Config.Bind(new ConfigDefinition("Configurables", "ChangeFontSize"),
            true,
            new ConfigDescription("Should the font size be changed."));

        Data.extraInfoPower = Config.Bind(new ConfigDefinition("Configurables", "ExtraInfoPower"),
            true,
            new ConfigDescription("Should a extra text label be placed next to the status like waste tank status."));

        Data.extraInfoFilter = Config.Bind(new ConfigDefinition("Configurables", "ExtraInfoFilter"),
            true,
            new ConfigDescription("Should a extra text label be placed next to the status like waste tank status."));

        Data.numberPrecision = Config.Bind(new ConfigDefinition("Configurables", "NumberPrecision"),
            2,
            new ConfigDescription("How many decimal points should be displayed on numbers.",
            new AcceptableValueRange<int>(1, 4)));

        Data.fontSize = Config.Bind(new ConfigDefinition("Configurables", "FontSize"),
            21,
            new ConfigDescription("What font size should the labels be changed to.",
            new AcceptableValueRange<int>(14, 28)));
    }


    public async UniTask OnBaseLoaded() {
        // Wait until game has loaded into main menu
        await UniTask.WaitUntil(() => MainMenuUI.Instance.IsVisible);

        // Print version after main menu is visible
        LogInfo($"v{Data.ModVersion} is installed.");

        Utilities.SetModVersion(Data.ModHandle, Data.ModVersion);
    }

    public static void LogException(Exception ex) => Plugin.Log($"[{ex?.Source} - {ex?.StackTrace}]: {ex?.Message}", Severity.Error);
    public static void LogError(string message) => Plugin.Log(message, Severity.Error);
    public static void LogWarning(string message) => Plugin.Log(message, Severity.Warning);
    public static void LogInfo(string message) => Plugin.Log(message, Severity.Info);
    public static void LogDebug(string message) {
        if (Constants.DEBUG_MODE) {
            Plugin.Log(message, Severity.Debug);
        }
    }

    private static void Log(string message, Severity severity) {
        string newMessage = $"[{Data.ModName}]: {message}";

        switch (severity) {
            case Severity.Error: {
                Plugin.LoggerInstance.LogError(message);
                ConsoleWindow.PrintError(newMessage);
                break;
            }
            case Severity.Warning: {
                Plugin.LoggerInstance.LogWarning(message);
                ConsoleWindow.PrintAction(newMessage);
                break;
            }
            case Severity.Info: {
                Plugin.LoggerInstance.LogInfo(message);
                ConsoleWindow.Print(newMessage);
                break;
            }
            default:
            case Severity.Debug: {
                Plugin.LoggerInstance.LogDebug(message);
                ConsoleWindow.Print(newMessage, color: ConsoleColor.Gray, aged: false);
            }
            break;
        }
    }
}

internal struct Data {
    // Mod Data
    public const string ModGuid = "detailedplayerinfo";
    public const string ModName = "DetailedPlayerInfo";
    public const string ModVersion = "1.7.0";
    public const ulong ModHandle = 3071950159;

    // Config
    public static ConfigEntry<PressureUnit> preferredPressureUnit;
    public static PressureUnit PreferredPressureUnit => preferredPressureUnit?.Value ?? PressureUnit.Pascal;

    public static ConfigEntry<TemperatureUnit> preferredTemperatureUnit;
    public static TemperatureUnit PreferredTemperatureUnit => preferredTemperatureUnit?.Value ?? TemperatureUnit.Celcius;

    public static ConfigEntry<VolumeUnit> preferredVolumeUnit;
    public static VolumeUnit PreferredVolumeUnit => preferredVolumeUnit?.Value ?? VolumeUnit.Liter;

    public static ConfigEntry<VelocityUnit> preferredVelocityUnit;
    public static VelocityUnit PreferredVelocityUnit => preferredVelocityUnit?.Value ?? VelocityUnit.Meters;

    public static ConfigEntry<bool> customFramerate;
    public static bool CustomFramerate => customFramerate?.Value ?? false;

    public static ConfigEntry<bool> changeFontSize;
    public static bool ChangeFontSize => changeFontSize?.Value ?? false;

    public static ConfigEntry<int> fontSize;
    public static int FontSize => ChangeFontSize ? (fontSize?.Value ?? 21) : 21;

    public static ConfigEntry<bool> extraInfoPower;
    public static bool ExtraInfoPower => extraInfoPower?.Value ?? false;

    public static ConfigEntry<bool> extraInfoFilter;
    public static bool ExtraInfoFilter => extraInfoFilter?.Value ?? false;

    public static ConfigEntry<int> numberPrecision;
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