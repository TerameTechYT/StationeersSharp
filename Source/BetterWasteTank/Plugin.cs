#region

using Assets.Scripts;
using Assets.Scripts.UI;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using UnityEngine.SceneManagement;

#endregion

namespace BetterWasteTank;

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

    public void LoadConfiguration() {
        Data.WasteCriticalRatio = this.Config.Bind("Configurables",
            "Waste Critical Ratio",
            0.975f,
            "(0.0 to 1.0) Ratio when \"Waste Tank Critical!\" alarm goes off.");

        Data.WasteCautionRatio = this.Config.Bind("Configurables",
            "Waste Caution Ratio",
            0.75f,
            "(0.0 to 1.0) Ratio when \"Waste Tank Caution\" alarm goes off.");
    }

    public static async UniTask OnBaseLoaded() {
        // Wait until game has loaded into main menu
        await UniTask.WaitUntil(() => MainMenu.Instance.IsVisible);

        // Print version after main menu is visible
        LogInfo($"v{Data.ModVersion} is installed.");
    }


    public static void LogError(string message) => Log(message, Data.Severity.Error);

    public static void LogWarning(string message) => Log(message, Data.Severity.Warning);

    public static void LogInfo(string message) => Log(message, Data.Severity.Info);

    private static void Log(string message, Data.Severity severity) {
        string newMessage = $"[{Data.ModName}]: {message}";

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
    public const string ModVersion = "1.3.6";
    public const string ModHandle = "3071913936";

    // Log Data
    public enum Severity {
        Error,
        Warning,
        Info
    }

    public class AlreadyLoadedException : Exception {
        public AlreadyLoadedException(string message) : base(message) {
        }

        public AlreadyLoadedException(string message, Exception innerException) : base(message, innerException) {
        }

        public AlreadyLoadedException() {
        }
    }

    public static ConfigEntry<float> WasteCriticalRatio;
    public static ConfigEntry<float> WasteCautionRatio;
}