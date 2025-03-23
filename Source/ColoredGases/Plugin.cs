#region

using MainMenuUI = Assets.Scripts.UI.MainMenu;

#endregion

namespace ColoredGases;

[BepInPlugin(Data.ModGuid, Data.ModName, Data.ModVersion)]
// https://steamcommunity.com/sharedfiles/filedetails/?id=3021353906
[BepInIncompatibility("com.ihatetn931.ColoredGasses")]
[BepInProcess(Constants.CLIENT_EXECUTABLE_NAME)]
[BepInProcess(Constants.HEADLESS_EXECUTABLE_NAME)]
public class Plugin : BaseUnityPlugin {
    public static Plugin Instance {
        get; private set;
    }

    public static Harmony HarmonyInstance {
        get; private set;
    }

    public static ManualLogSource LoggerInstance => Plugin.Instance.Logger;

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
        Data.enableAirVisualizer = Config.Bind(
            new ConfigDefinition("Configurables", "Enable Colored Air Visualier"),
            true,
            new ConfigDescription("Enable or disable custom colored air visualizers.")
        );

        Data.enableFogVisualizer = Config.Bind(
            new ConfigDefinition("Configurables", "Enable Colored Fog Visualier"),
            true,
            new ConfigDescription("Enable or disable custom colored fog visualizers.")
        );
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
                Plugin.LoggerInstance?.LogError(message);
                ConsoleWindow.PrintError(newMessage);
                break;
            }
            case Severity.Warning: {
                Plugin.LoggerInstance?.LogWarning(message);
                ConsoleWindow.PrintAction(newMessage);
                break;
            }
            case Severity.Info: {
                Plugin.LoggerInstance?.LogInfo(message);
                ConsoleWindow.Print(newMessage);
                break;
            }
            default:
            case Severity.Debug: {
                Plugin.LoggerInstance?.LogDebug(message);
                ConsoleWindow.Print(newMessage, color: ConsoleColor.Gray, aged: false);
            }
            break;
        }
    }
}

internal struct Data {
    // Mod Data
    public const string ModGuid = "coloredgases";
    public const string ModName = "ColoredGases";
    public const string ModVersion = "1.1.0";
    public const ulong ModHandle = 0;

    // config
    public static ConfigEntry<bool> enableAirVisualizer;
    public static bool EnableAirVisualizer => enableAirVisualizer?.Value ?? false;

    public static ConfigEntry<bool> enableFogVisualizer;
    public static bool EnableFogVisualizer => enableFogVisualizer?.Value ?? false;
}