#region

using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using MainMenuUI = Assets.Scripts.UI.MainMenu;

#endregion

namespace BetterPowerMod;

[BepInPlugin(Data.ModGuid, Data.ModName, Data.ModVersion)]
[BepInProcess(Data.ExecutableName)]
[BepInProcess(Data.DSExecutableName)]
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
        SceneManager.sceneLoaded += (scene, _) => {
            if (scene.name == "Base")
                OnBaseLoaded().Forget();
        };
    }

    public void LoadConfiguration() {
        Data.enableSolarPanel = Config.Bind("Configurables",
            "Solar Panel Patches",
            true,
            "Should the max power output be set to the worlds Solar Irradiance");
        Data.enableWindTurbine = Config.Bind("Configurables",
            "Wind Turbine Patches",
            true,
            "Should the max power output be set higher based on the atmospheric pressure");

        Data.enableTurbine = Config.Bind("Configurables",
            "Wall Turbine Patches",
            true,
            "Should the max power output be multipled by 10");

        Data.enableStirling = Config.Bind("Configurables",
            "Stirling Patches",
            true,
            $"Should the max power output be changed to Stirling Energy Output");

        Data.stirlingEnergy = Config.Bind("Configurables",
            "Stirling Energy Output",
            Data.TwentyKilowatts,
            $"The max power output of the Stirling Engine");

        Data.enableFasterCharging = Config.Bind("Configurables",
            "Charging Patches",
            true,
            $"Should the max input power of (Area Power Controller, Small and Large Battery Charger, Omni Power Transmitter) be set to Fast Charge Rate");

        Data.fastChargeRate = Config.Bind("Configurables",
            "Fast Charging Charging Rate",
            Data.TwoAndAHalfKilowatts,
            $"The max input power of the (Area Power Controller, Small and Large Battery Charger, Omni Power Transmitter)");
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
    public const string ModGuid = "betterpowermod";
    public const string ModName = "BetterPowerMod";
    public const string ModVersion = "1.1.7";
    public const ulong ModHandle = 3234916147;

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

    public static List<string> IgnoredPrefabs => [
        "StructureSolarPanelFlat", "StructureSolarPanel45",
        "StructureSolarPanelFlatReinforced", "StructureSolarPanel45Reinforced"
    ];

    public const float OneKilowatt = 1000f;
    public const float FiveKilowatts = OneKilowatt * 5f;
    public const float TwoAndAHalfKilowatts = OneKilowatt * 2.5f;
    public const float TwentyKilowatts = OneKilowatt * 20f;
    public const float FiftyKilowatts = OneKilowatt * 50f;
    public const float OneHundredKilowatts = OneKilowatt * 100f;

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
    public static float StirlingEnergy => stirlingEnergy?.Value ?? TwentyKilowatts;

    //
    public static ConfigEntry<bool> enableFasterCharging;
    public static bool EnableFasterCharging => enableWindTurbine?.Value ?? false;

    public static ConfigEntry<float> fastChargeRate;
    public static float FastChargeRate => fastChargeRate?.Value ?? TwoAndAHalfKilowatts;
}