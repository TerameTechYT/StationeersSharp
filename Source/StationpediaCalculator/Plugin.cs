#region

using System;
using Assets.Scripts;
using Assets.Scripts.UI;
using BepInEx;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

#endregion

namespace StationpediaCalculator;

[BepInPlugin(Data.ModGuid, Data.ModName, Data.ModVersion)]
[BepInProcess("rocketstation.exe")]
public class Plugin : BaseUnityPlugin {
    public static Plugin Instance { get; private set; }

    public static Harmony HarmonyInstance { get; private set; }

    [UsedImplicitly]
    public void Awake() {
        //if (Harmony.HasAnyPatches(Data.ModGuid))
        //    throw new Exception($"Mod {Data.ModName} ({Data.ModGuid}) - {Data.ModVersion} has already been loaded!");

        Instance = this;
        HarmonyInstance = new Harmony(Data.ModGuid);
        HarmonyInstance.PatchAll();

        // Thx jixxed for awesome code :)
        SceneManager.sceneLoaded += (scene, _) => {
            if (scene.name == "Base")
                OnBaseLoaded().Forget();
        };
    }

    public async UniTask OnBaseLoaded() {
        // Wait until game has loaded into main menu
        await UniTask.WaitUntil(() => { return MainMenu.Instance.IsVisible; });
        // Check version after main menu is visible
        await CheckVersion();
    }

    public async UniTask CheckVersion() {
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


    public void LogError(string message) {
        Log(message, Data.Severity.Error);
    }

    public void LogWarning(string message) {
        Log(message, Data.Severity.Warning);
    }

    public void LogInfo(string message) {
        Log(message, Data.Severity.Info);
    }

    private void Log(string message, Data.Severity severity) {
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
    public const string ModGuid = "stationpediacalculator";
    public const string ModName = "StationpediaCalculator";
    public const string ModVersion = "1.0.4";
    public const string ModHandle = "3305312105";

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


    public static SPDAListItem CalculatorItem;
}