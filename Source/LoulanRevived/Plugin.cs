#region

using MainMenuUI = Assets.Scripts.UI.MainMenu;

#endregion

namespace LoulanRevived;

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

    public void LoadConfiguration() => Data.spawnWrecks = Config.Bind(new ConfigDefinition("Configurables", "Wrecks"),
            false,
            new ConfigDescription("Should mod attempt to spawn wrecks (warning, may be unstable!)"));

    public static async UniTask OnBaseLoaded() {
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
                ConsoleWindow.Print(newMessage, color: ConsoleColor.Gray, aged: false);
            }
            break;
        }
    }
}

internal struct Data {
    // Mod Data
    public const string ModGuid = "loulanrevived";
    public const string ModName = "LoulanRevived";
    public const string ModVersion = "1.0.0";
    public const ulong ModHandle = 3255025164;

    // Config
    public static ConfigEntry<bool> spawnWrecks;
    public static bool SpawnWrecks => spawnWrecks?.Value ?? false;
}