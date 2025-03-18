#region

using static BepInEx.BepInDependency;
using MainMenuUI = Assets.Scripts.UI.MainMenu;

#endregion

namespace BetterCodeEditor;

[BepInPlugin(Data.ModGuid, Data.ModName, Data.ModVersion)]
// https://steamcommunity.com/sharedfiles/filedetails/?id=3265272725
[BepInIncompatibility("awa.shark.plugin.MoreLinesCodeMod")]
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
        Data.codeEditorLines = Config.Bind("Configurables",
            "Code Editor Lines",
            InputSourceCode.MAX_LINES,
            "Number of lines in the code editor.");

        Data.codeEditorLineLength = Config.Bind("Configurables",
            "Code Editor Line Length",
            InputSourceCode.LINE_LENGTH_LIMIT,
            "The length of the code editor lines");
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
    public const string ModGuid = "bettercodeeditor";
    public const string ModName = "BetterCodeEditor";
    public const string ModVersion = "1.1.0";
    public const ulong ModHandle = 0;

    public static ConfigEntry<int> codeEditorLines;
    public static int CodeEditorLines => codeEditorLines?.Value ?? InputSourceCode.MAX_LINES;

    public static ConfigEntry<int> codeEditorLineLength;
    public static int CodeEditorLineLength => codeEditorLineLength?.Value ?? InputSourceCode.LINE_LENGTH_LIMIT;

    public static int BytesPerLine => InputSourceCode.MAX_FILE_SIZE / InputSourceCode.MAX_LINES;
    public static int MaxFileSize => BytesPerLine * CodeEditorLines;
}