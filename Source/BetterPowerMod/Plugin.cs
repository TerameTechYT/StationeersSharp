#region

using System;
using Assets.Scripts;
using Assets.Scripts.UI;
using BepInEx;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

#endregion

namespace BetterPowerMod;

[BepInPlugin(Data.ModGuid, Data.ModName, Data.ModVersion)]
[BepInProcess("rocketstation.exe")]
[BepInProcess("rocketstation_DedicatedServer.exe")]
public class Plugin : BaseUnityPlugin {
    public static Plugin Instance { get; private set; }

    public static Harmony HarmonyInstance { get; private set; }

    [UsedImplicitly]
    public void Awake() {
        //if (Harmony.HasAnyPatches(Data.ModGuid))
        //    throw new Exception($"Mod {Data.ModName} ({Data.ModGuid}) - {Data.ModVersion} has already been loaded!");

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
        Data.EnableSolarPanel = Config.Bind("Configurables",
            "Solar Panel Patches",
            true,
            "Should the max power output be set to the worlds Solar Irradiance");
        Data.EnableWindTurbine = Config.Bind("Configurables",
            "Wind Turbine Patches",
            true,
            "Should the max power output be set higher based on the atmospheric pressure");
        ;
        Data.EnableTurbine = Config.Bind("Configurables",
            "Wall Turbine Patches",
            true,
            "Should the max power output be multipled by 10");
        ;
        Data.EnableStirling = Config.Bind("Configurables",
            "Stirling Patches",
            true,
            $"Should the max power output be set to {Data.TwentyKilowatts} like the gas fuel generator");
        ;
        Data.EnableFasterCharging = Config.Bind("Configurables",
            "Charging Patches",
            true,
            $"Should the max input power of (Area Power Controller, Small and Large Battery Charger, Omni Power Transmitter) be set to {Data.TwoAndAHalfKilowatts}");
        ;
    }

    public static async UniTask OnBaseLoaded() {
        // Wait until game has loaded into main menu
        await UniTask.WaitUntil(() => { return MainMenu.Instance.IsVisible; });
        // Check version after main menu is visible
        await CheckVersion();
    }

    public static async UniTask CheckVersion() {
        if (!Data.VersionCheck) {
            LogWarning("Version check has been disabled.");

            return;
        }

        try {
            var webRequest = await UnityWebRequest.Get(new Uri(Data.GitVersion)).SendWebRequest();
            var currentVersion = webRequest.downloadHandler.text.Trim();

            if (webRequest.result == UnityWebRequest.Result.Success) {
                LogInfo($"v{Data.ModVersion} is installed.");

                if (Data.ModVersion == currentVersion)
                    return;

                LogWarning($"New version v{currentVersion} is available!");
            }

            webRequest.Dispose();
        }
        catch (Exception e) {
            LogError($"Failed to request latest version! {e.StackTrace}: {e.Message}");
        }
    }


    public static void LogError(string message) {
        Log(message, Data.Severity.Error);
    }

    public static void LogWarning(string message) {
        Log(message, Data.Severity.Warning);
    }

    public static void LogInfo(string message) {
        Log(message, Data.Severity.Info);
    }

    private static void Log(string message, Data.Severity severity) {
        var newMessage = $"[{Data.ModName}]: {message}";

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
    public const string ModGuid = "betterpowermod";
    public const string ModName = "BetterPowerMod";
    public const string ModVersion = "1.0.7";
    public const string ModHandle = "3234906754";

    // Log Data
    public enum Severity {
        Error,
        Warning,
        Info
    }

    // Version Check Data
    public const string GitContent = "https://raw.githubusercontent.com/";
    public const string GitAuthor = "TerameTechYT";
    public const string GitName = "StationeersSharp";
    public const string GitBranch = "development";
    public const string GitSourceFolder = "Source";
    public const string GitVersionFile = "VERSION";

    public const string GitVersion =
        $"{GitContent}/{GitAuthor}/{GitName}/{GitBranch}/{GitSourceFolder}/{ModName}/{GitVersionFile}";

    public const bool VersionCheck = false;

    public const float OneKilowatt = 1000f;
    public const float FiveKilowatts = OneKilowatt * 5f;
    public const float TwoAndAHalfKilowatts = OneKilowatt * 2.5f;
    public const float TwentyKilowatts = OneKilowatt * 20f;
    public const float FiftyKilowatts = OneKilowatt * 50f;
    public const float OneHundredKilowatts = OneKilowatt * 100f;

    public static ConfigEntry<bool> EnableSolarPanel;
    public static ConfigEntry<bool> EnableWindTurbine;
    public static ConfigEntry<bool> EnableTurbine;
    public static ConfigEntry<bool> EnableStirling;
    public static ConfigEntry<bool> EnableFasterCharging;
}