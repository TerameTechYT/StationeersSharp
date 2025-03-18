#region

using static BepInEx.BepInDependency;
using MainMenuUI = Assets.Scripts.UI.MainMenu;

#endregion

namespace BetterStartScreen;

[BepInPlugin(Data.ModGuid, Data.ModName, Data.ModVersion)]
[BepInDependency(Constants.STATIONEERS_LIBRARY_GUID, DependencyFlags.HardDependency)]
[BepInProcess(Constants.CLIENT_EXECUTABLE_NAME)]
[BepInProcess(Constants.HEADLESS_EXECUTABLE_NAME)]
public class Plugin : BaseUnityPlugin {
    public static Plugin Instance {
        get; private set;
    }

    public static Harmony HarmonyInstance {
        get; private set;
    }

    public static ManualLogSource LoggerInstance {
        get => Instance.Logger;
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
    public const string ModGuid = "betterstartscreen";
    public const string ModName = "BetterStartScreen";
    public const string ModVersion = "1.0.0";
    public const ulong ModHandle = 0;
}