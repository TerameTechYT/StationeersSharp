#region

using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using MainMenuUI = Assets.Scripts.UI.MainMenu;

#endregion

namespace DetailedPlayerInfo;

[BepInPlugin(Data.ModGuid, Data.ModName, Data.ModVersion)]
[BepInProcess(Data.ExecutableName)]
public class Plugin : BaseUnityPlugin {
    public static Plugin Instance {
        get; private set;
    }

    public static Harmony HarmonyInstance {
        get; private set;
    }

    [UsedImplicitly]
    public void Awake() {
        if (Chainloader.PluginInfos.TryGetValue(Data.ModGuid, out _))
            throw new Data.AlreadyLoadedException($"Mod {Data.ModName} ({Data.ModGuid}) - {Data.ModVersion} has already been loaded!");

        LoadConfiguration();

        Instance = this;
        HarmonyInstance = new Harmony(Data.ModGuid);
        HarmonyInstance.PatchAll();

        // Thx jixxed for awesome code :)
        SceneManager.sceneLoaded += (scene, sceneMode) => {
            if (scene.name == "Base")
                OnBaseLoaded().Forget();
        };
    }

    public void LoadConfiguration() {
        Data.kelvinMode = Config.Bind("Keybinds",
            "Kelvin Mode",
            KeyCode.K,
            "Keybind that when pressed, changes the status temperatures to kelvin instead of celcius.");

        Data.customFramerate = Config.Bind("Configurables",
            "CustomFramerate",
            true,
            "Should the framerate text only display FPS.");

        Data.changeFontSize = Config.Bind("Configurables",
            "ChangeFontSize",
            true,
            "Should the font size be changed.");

        Data.extraInfoPower = Config.Bind("Configurables",
            "ExtraInfoPower",
            true,
            "Should a extra text label be placed next to the status like waste tank status.");

        Data.extraInfoFilter = Config.Bind("Configurables",
            "ExtraInfoFilter",
            true,
            "Should a extra text label be placed next to the status like waste tank status.");

        Data.numberPrecision = Config.Bind("Configurables",
            "NumberPrecision",
            2,
            "How many decimal points should be displayed on numbers.");

        Data.fontSize = Config.Bind("Configurables",
            "FontSize",
            21,
            "What font size should the labels be changed to.");
    }


    public async UniTask OnBaseLoaded() {
        // Wait until game has loaded into main menu
        await UniTask.WaitUntil(() => MainMenuUI.Instance.IsVisible);

        // Print version after main menu is visible
        LogInfo("is installed.");

        SetModVersion();
    }

    private void SetModVersion() {
        ModData mod = WorkshopMenu.ModsConfig.Mods.Find((mod) => mod.GetAboutData().WorkshopHandle == Data.ModHandle);
        if (mod == null) {
            return;
        }

        ModAbout aboutData = mod.GetAboutData();
        aboutData.Version = Data.ModVersion;

        Traverse.Create(mod).Field("_modAboutData").SetValue(aboutData);
    }

    public static void LogError(Exception ex) => Log($"[{ex.Source} - {ex.StackTrace}]: {ex.Message}", Data.Severity.Error);
    public static void LogError(string message) => Log(message, Data.Severity.Error);
    public static void LogWarning(string message) => Log(message, Data.Severity.Warning);
    public static void LogInfo(string message) => Log(message, Data.Severity.Info);

    private static void Log(string message, Data.Severity severity) {
        string newMessage = $"[{Data.ModName} - v{Data.ModVersion}]: {message}";

        switch (severity) {
            case Data.Severity.Error: {
                ConsoleWindow.PrintError(newMessage);
                break;
            }
            case Data.Severity.Warning: {
                ConsoleWindow.PrintAction(newMessage);
                break;
            }
            case Data.Severity.Info:
            default: {
                ConsoleWindow.Print(newMessage);
                break;
            }
        }
    }
}

internal struct Data {
    // Mod Data
    public const string ModGuid = "detailedplayerinfo";
    public const string ModName = "DetailedPlayerInfo";
    public const string ModVersion = "1.6.3";
    public const ulong ModHandle = 3071950159;

    // Game Data
    public const string ExecutableName = "rocketstation.exe";
    public const string DSExecutableName = "rocketstation_DedicatedServer.exe";

    // Log Data
    internal enum Severity {
        Error,
        Warning,
        Info
    }

    public sealed class AlreadyLoadedException : Exception {
        public AlreadyLoadedException(string message) : base(message) {
        }

        public AlreadyLoadedException(string message, Exception innerException) : base(message, innerException) {
        }

        public AlreadyLoadedException() {
        }
    }

    // Config
    public static ConfigEntry<KeyCode> kelvinMode;
    public static KeyCode KelvinMode => kelvinMode?.Value ?? KeyCode.K;

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

    public const float TemperatureZero = 273.15f;
    public const float TemperatureOne = TemperatureZero + 1f;
    public const float TemperatureTwenty = TemperatureZero + 20f;
    public const float TemperatureThirty = TemperatureZero + 30f;
    public const float TemperatureFifty = TemperatureZero + 50f;

    public const float TemperatureMinimumSafe = TemperatureZero;
    public const float TemperatureMaximumSafe = TemperatureFifty;

    public const float TemperatureMinimum = 1f;
    public const float TemperatureMaximum = 80000f;

    public const float PressureAtmosphere = 101.325f;

    public const float PressureMinimumSafe = 273.15f;
    public const float PressureMaximumSafe = 607.94995f;

    public const float PressureMinimum = 0f;
    public const float PressureMaximum = 1000000f;

    public const string ExternalTemperatureUnit =
        "GameCanvas/PanelStatusInfo/PanelExternalNavigation/PanelExternal/PanelTemp/ValueTemp/TextUnitTemp";

    public const string InternalTemperatureUnit =
        "GameCanvas/PanelStatusInfo/PanelVerticalGroup/Internals/PanelInternal/PanelTemp/ValueTemp/TextUnitTemp";

    public const string WasteTextPanel =
        "GameCanvas/StatusIcons/Waste/Panel";

    public const string BatteryStatus =
        "GameCanvas/StatusIcons/Power";

    public const string FilterStatus =
        "GameCanvas/StatusIcons/Filter";
}