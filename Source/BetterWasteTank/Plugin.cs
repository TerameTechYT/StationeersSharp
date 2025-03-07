#region

using MainMenuUI = Assets.Scripts.UI.MainMenu;

#endregion

namespace BetterWasteTank;

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
        Data.wasteCriticalRatio = Config.Bind(new ConfigDefinition("Configurables", "Waste Critical Ratio"),
            0.975,
            new ConfigDescription("Ratio when \"Waste Tank Critical!\" alarm goes off.", new AcceptableValueRange<double>(0.0, 1.0)));

        Data.wasteCautionRatio = Config.Bind(new ConfigDefinition("Configurables", "Waste Caution Ratio"),
            0.75,
            new ConfigDescription("Ratio when \"Waste Tank Caution\" alarm goes off.", new AcceptableValueRange<double>(0.0, 1.0)));

        /*Data.airCriticalRatio = Config.Bind(new ConfigDefinition("Configurables", "Air Critical Ratio"),
            0.15,
            new ConfigDescription("Ratio when \"Air Tank Critical!\" alarm goes off.", new AcceptableValueRange<double>(0.0, 1.0)));

        Data.airCautionRatio = Config.Bind(new ConfigDefinition("Configurables", "Air Caution Ratio"),
            0.30,
            new ConfigDescription("Ratio when \"Air Tank Caution\" alarm goes off.", new AcceptableValueRange<double>(0.0, 1.0)));*/
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
    public const string ModGuid = "betterwastetank";
    public const string ModName = "BetterWasteTank";
    public const string ModVersion = "1.5.0";
    public const ulong ModHandle = 3071913936;

    // Config Data
    public static ConfigEntry<double> wasteCriticalRatio;
    public static double WasteCriticalRatio => wasteCriticalRatio?.Value ?? 0.75;

    public static ConfigEntry<double> wasteCautionRatio;
    public static double WasteCautionRatio => wasteCautionRatio?.Value ?? 0.975;

    public static ConfigEntry<double> airCautionRatio;
    public static double AirCautionRatio => airCautionRatio?.Value ?? 0.30;

    public static ConfigEntry<double> airCriticalRatio;
    public static double AirCriticalRatio => airCriticalRatio?.Value ?? 0.15;
}