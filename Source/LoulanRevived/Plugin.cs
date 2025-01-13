#region

using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using MainMenuUI = Assets.Scripts.UI.MainMenu;

#endregion

namespace LoulanRevived;

[BepInPlugin(Data.ModGuid, Data.ModName, Data.ModVersion)]
[BepInProcess("rocketstation.exe")]
[BepInProcess("rocketstation_DedicatedServer.exe")]
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

        this.LoadConfiguration();

        Instance = this;
        HarmonyInstance = new Harmony(Data.ModGuid);
        HarmonyInstance.PatchAll();

        // Thx jixxed for awesome code :)
        SceneManager.sceneLoaded += (scene, _) => {
            if (scene.name == "Base")
                OnBaseLoaded().Forget();
        };
    }

    public void LoadConfiguration() => Data.spawnWrecks = Config.Bind(new ConfigDefinition("Configurables", "Wrecks"),
            false,
            new ConfigDescription("Should mod attempt to spawn wrecks (warning, may be unstable)"));

    public static async UniTask OnBaseLoaded() {
        // Wait until game has loaded into main menu
        await UniTask.WaitUntil(() => MainMenuUI.Instance.IsVisible);

        // Print version after main menu is visible
        LogInfo("is installed.");

        SetModVersion();
    }

    private static void SetModVersion() {
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
    public const string ModGuid = "loulanrevived";
    public const string ModName = "LoulanRevived";
    public const string ModVersion = "1.0.0";
    public const ulong ModHandle = 3255025164;

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
    public static ConfigEntry<bool> spawnWrecks;
    public static bool SpawnWrecks => spawnWrecks?.Value ?? false;
}