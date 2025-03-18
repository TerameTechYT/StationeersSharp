#region

using MainMenuUI = Assets.Scripts.UI.MainMenu;

#endregion

namespace StationeersLibrary;

[BepInPlugin(Data.ModGuid, Data.ModName, Data.ModVersion)]
[BepInProcess(Constants.CLIENT_EXECUTABLE_NAME)]
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

        // Thx jixxed for awesome code :)
        SceneManager.sceneLoaded += (scene, _) => {
            if (scene.name == Constants.BASE_SCENE_NAME) {
                this.OnBaseLoaded().Forget();
            }
        };
    }

    public void OnDestroy() => StationeersLog.LogStream?.Dispose();

    public void LoadConfiguration() {

    }

    public async UniTask OnBaseLoaded() {
        // Wait until game has loaded into main menu
        await UniTask.WaitUntil(() => MainMenuUI.Instance.IsVisible);

        // Print version after main menu is visible
        LogInfo($"{Data.ModVersion} is installed.");

        Utilities.SetModVersion(Data.ModHandle, Data.ModVersion);
    }

    public static void LogException(Exception ex) => StationeersLog.LogException(Data.ModName, ex);
    public static void LogError(string message) => StationeersLog.LogError(Data.ModName, message);
    public static void LogWarning(string message) => StationeersLog.LogWarning(Data.ModName, message);
    public static void LogInfo(string message) => StationeersLog.LogInfo(Data.ModName, message);
    public static void LogDebug(string message) => StationeersLog.LogDebug(Data.ModName, message);
}

internal struct Data {
    // Mod Data
    public const string ModGuid = "stationeerslibrary";
    public const string ModName = "StationeersLibrary";
    public const string ModVersion = "1.0.0";
    public const ulong ModHandle = 3389894703;
}