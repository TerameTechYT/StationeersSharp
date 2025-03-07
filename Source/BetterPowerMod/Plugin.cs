#region

using MainMenuUI = Assets.Scripts.UI.MainMenu;

#endregion

namespace BetterPowerMod;

[BepInPlugin(Data.ModGuid, Data.ModName, Data.ModVersion)]
[BepInProcess(Constants.CLIENT_EXECUTABLE_NAME)]

[BepInProcess(Constants.HEADLESS_EXECUTABLE_NAME)]
public class Plugin : BaseUnityPlugin {
    public static Plugin Instance {
        get; private set;
    }

    public static Harmony HarmonyInstance {
        get; private set;
    }

    [UsedImplicitly]
    public void Awake() {
        if (Utilities.IsLoaded(Data.ModGuid)) {
            throw new AlreadyLoadedException(Data.ModName, Data.ModGuid, Data.ModVersion);
        }

        this.LoadConfiguration();

        Plugin.Instance = this;
        Plugin.HarmonyInstance = new Harmony(Data.ModGuid);
        Plugin.HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

        // Thx jixxed for awesome code :)
        SceneManager.sceneLoaded += (scene, _) => {
            if (scene.name == Constants.BASE_SCENE_NAME) {
                OnBaseLoaded().Forget();
            }
        };
    }

    public void LoadConfiguration() {
        Data.enableSolarPanel = Config.Bind(new ConfigDefinition("Configurables", "Solar Panel Patches"),
            true,
            new ConfigDescription("Should the max power output be set to the worlds Solar Irradiance"));
        Data.enableWindTurbine = Config.Bind(new ConfigDefinition("Configurables", "Wind Turbine Patches"),
            true,
            new ConfigDescription("Should the max power output be set higher based on the atmospheric pressure"));

        Data.enableTurbine = Config.Bind(new ConfigDefinition("Configurables", "Wall Turbine Patches"),
            true,
            new ConfigDescription("Should the max power output be multipled by 10"));

        Data.enableStirling = Config.Bind(new ConfigDefinition("Configurables", "Stirling Patches"),
            true,
            new ConfigDescription($"Should the max power output be changed to Stirling Energy Output"));

        Data.stirlingEnergy = Config.Bind(new ConfigDefinition("Configurables", "Stirling Energy Output"),
            Constants.TWENTY_KILOWATTS,
            new ConfigDescription("The max power output of the Stirling Engine",
            new AcceptableValueRange<float>(Constants.EIGHT_KILOWATTS, Constants.TWENTY_FIVE_KILOWATTS)));

        Data.enableFasterCharging = Config.Bind(new ConfigDefinition("Configurables", "Charging Patches"),
            true,
            new ConfigDescription("Should the max input power of (Area Power Controller, Small and Large Battery Charger, Omni Power Transmitter) be set to Fast Charge Rate"));

        Data.fastChargeRate = Config.Bind(new ConfigDefinition("Configurables", "Fast Charging Charging Rate"),
            Constants.TWO_POINT_FIVE_KILOWATTS,
            new ConfigDescription("The max input power of the (Area Power Controller, Small and Large Battery Charger, Omni Power Transmitter)",
            new AcceptableValueRange<float>(1f, Constants.FIVE_KILOWATTS)));
    }

    public async UniTask OnBaseLoaded() {
        // Wait until game has loaded into main menu
        await UniTask.WaitUntil(() => MainMenuUI.Instance.IsVisible);

        // Print version after main menu is visible
        LogInfo($"{Data.ModVersion} is installed.");

        Utilities.SetModVersion(Data.ModHandle, Data.ModVersion);
    }

    public static void LogException(Exception ex) => Log($"[{ex.Source} - {ex.StackTrace}]: {ex.Message}", Severity.Error);
    public static void LogError(string message) => Log(message, Severity.Error);
    public static void LogWarning(string message) => Log(message, Severity.Warning);
    public static void LogInfo(string message) => Log(message, Severity.Info);

#if DEBUG
    public static void LogDebug(string message) => Log(message, Severity.Debug);
#else
    public static void LogDebug(string message) {
    }
#endif

    private static void Log(string message, Severity severity) {
        string newMessage = $"[{Data.ModName}]: {message}";

        switch (severity) {
            case Severity.Error: {
                ConsoleWindow.PrintError(newMessage);
                break;
            }
            case Severity.Warning: {
                ConsoleWindow.PrintAction(newMessage);
                break;
            }
            case Severity.Info: {
                ConsoleWindow.Print(newMessage);
                break;
            }
            default:
            case Severity.Debug: {
                Debug.Log(newMessage);
            }
            break;
        }
    }
}

internal struct Data {
    // Mod Data
    public const string ModGuid = "betterpowermod";
    public const string ModName = "BetterPowerMod";
    public const string ModVersion = "1.2.0";
    public const ulong ModHandle = 3234916147;

    public static List<string> IgnoredSolarPanelPrefabs => [
        "StructureSolarPanelFlat", "StructureSolarPanel45",
        "StructureSolarPanelFlatReinforced", "StructureSolarPanel45Reinforced"
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

    public static ConfigEntry<float> fastChargeRate;
    public static float FastChargeRate => fastChargeRate?.Value ?? Constants.TWO_POINT_FIVE_KILOWATTS;
}