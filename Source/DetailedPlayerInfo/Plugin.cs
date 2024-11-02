#region

using System;
using Assets.Scripts;
using Assets.Scripts.UI;
using BepInEx;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

#endregion

namespace DetailedPlayerInfo;

[BepInPlugin(Data.ModGuid, Data.ModName, Data.ModVersion)]
[BepInProcess("rocketstation.exe")]
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
        SceneManager.sceneLoaded += (scene, sceneMode) => {
            if (scene.name == "Base")
                OnBaseLoaded().Forget();
        };
    }

    public void LoadConfiguration() {
        Data.KelvinMode = Config.Bind("Keybinds",
            "Kelvin Mode",
            KeyCode.K,
            "Keybind that when pressed, changes the status temperatures to kelvin instead of celcius.");

        Data.CustomFramerate = Config.Bind("Configurables",
            "CustomFramerate",
            true,
            "Should the framerate text only display FPS.");

        Data.ChangeFontSize = Config.Bind("Configurables",
            "ChangeFontSize",
            true,
            "Should the font size be changed.");

        Data.ExtraInfoPower = Config.Bind("Configurables",
            "ExtraInfoPower",
            true,
            "Should a extra text label be placed next to the status like waste tank status.");

        Data.ExtraInfoFilter = Config.Bind("Configurables",
            "ExtraInfoFilter",
            true,
            "Should a extra text label be placed next to the status like waste tank status.");

        Data.NumberPrecision = Config.Bind("Configurables",
            "NumberPrecision",
            2,
            "How many decimal points should be displayed on numbers.");

        Data.FontSize = Config.Bind("Configurables",
            "FontSize",
            21,
            "What font size should the labels be changed to.");
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
    public const string ModGuid = "detailedplayerinfo";
    public const string ModName = "DetailedPlayerInfo";
    public const string ModVersion = "1.5.7";
    public const string ModHandle = "3071950159";

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

    //Keycode
    public static ConfigEntry<KeyCode> KelvinMode;

    //Bools
    public static ConfigEntry<bool> CustomFramerate;
    public static ConfigEntry<bool> ChangeFontSize;

    public static ConfigEntry<bool> ExtraInfoPower;
    public static ConfigEntry<bool> ExtraInfoFilter;

    //Ints/Floats
    public static ConfigEntry<int> NumberPrecision;
    public static ConfigEntry<int> FontSize;

    public const float TemperatureZero = 273.15f;
    public const float TemperatureOne = TemperatureZero + 1f;
    public const float TemperatureTwenty = TemperatureZero + 20f;
    public const float TemperatureThirty = TemperatureZero + 30f;
    public const float TemperatureFifty = TemperatureZero + 50f;

    public const float TemperatureMinimumSafe = TemperatureZero;
    public const float TemperatureMaximumSafe = TemperatureFifty;

    public const float TemperatureMinimum = 1f;
    public const float TemperatureMaximum = 80000f;

    public const float PressureAtmosphere = 101.325f;

    public const float PressureMinimumSafe = 273.15f;
    public const float PressureMaximumSafe = 607.94995f;

    public const float PressureMinimum = 0f;
    public const float PressureMaximum = 1000000f;

    public const string ExternalTemperatureUnit =
        "GameCanvas/PanelStatusInfo/PanelExternalNavigation/PanelExternal/PanelTemp/ValueTemp/TextUnitTemp";

    public const string InternalTemperatureUnit =
        "GameCanvas/PanelStatusInfo/PanelVerticalGroup/Internals/PanelInternal/PanelTemp/ValueTemp/TextUnitTemp";

    public const string WasteTextPanel =
        "GameCanvas/StatusIcons/Waste/Panel";

    public const string BatteryStatus =
        "GameCanvas/StatusIcons/Power";

    public const string FilterStatus =
        "GameCanvas/StatusIcons/Filter";
}