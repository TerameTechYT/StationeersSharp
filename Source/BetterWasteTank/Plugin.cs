﻿#region

using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using MainMenuUI = Assets.Scripts.UI.MainMenu;

#endregion

namespace BetterWasteTank;

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
        Data.wasteCriticalRatio = Config.Bind("Configurables",
            "Waste Critical Ratio",
            0.975,
            "(0.0 to 1.0) Ratio when \"Waste Tank Critical!\" alarm goes off.");

        Data.wasteCautionRatio = Config.Bind("Configurables",
            "Waste Caution Ratio",
            0.75,
            "(0.0 to 1.0) Ratio when \"Waste Tank Caution\" alarm goes off.");
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
    public const string ModGuid = "betterwastetank";
    public const string ModName = "BetterWasteTank";
    public const string ModVersion = "1.4.1";
    public const ulong ModHandle = 3071913936;

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

    public static ConfigEntry<double> wasteCriticalRatio;
    public static double WasteCriticalRatio => wasteCriticalRatio?.Value ?? 0.75;

    public static ConfigEntry<double> wasteCautionRatio;
    public static double WasteCautionRatio => wasteCautionRatio?.Value ?? 0.975;
}