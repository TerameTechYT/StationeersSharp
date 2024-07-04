

using Assets.Scripts;
using Assets.Scripts.UI;
using BepInEx;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace DetailedPlayerInfo;

[BepInPlugin(Data.Guid, Data.Name, Data.Version)]
[BepInDependency("PlantsnNutrition", BepInDependency.DependencyFlags.SoftDependency)]
[BepInProcess("rocketstation.exe")]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance { get; private set; }
    public static Harmony HarmonyInstance { get; private set; }

    [UsedImplicitly]
    public void Awake()
    {
        LoadConfiguration();

        Logger.LogInfo(Data.Name + " successfully loaded!");
        Instance = this;
        HarmonyInstance = new Harmony(Data.Guid);
        HarmonyInstance.PatchAll();
        Logger.LogInfo(Data.Name + " successfully patched!");


        // Thx jixxed for awesome code :)
        SceneManager.sceneLoaded += (scene, sceneMode) =>
        {
            if (scene.name == "Base")
            {
                OnBaseLoaded().Forget();
            }
        };
    }

    public void LoadConfiguration()
    {
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


    public async UniTask OnBaseLoaded()
    {
        // Wait until game has loaded into main menu
        await UniTask.WaitUntil(() => { return MainMenu.Instance.IsVisible; });
        // Check version after main menu is visible
        await CheckVersion();
    }

    public async UniTask CheckVersion()
    {
        UnityWebRequest webRequest = await UnityWebRequest.Get(new Uri(Data.GitVersion)).SendWebRequest();
        Logger.LogInfo("Awaiting send web request...");

        string currentVersion = webRequest.downloadHandler.text.Trim();
        Logger.LogInfo("Await complete!");

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            Logger.LogInfo($"Latest version is {currentVersion}. Installed {Data.Version}");
            ConsoleWindow.Print($"[{Data.Name}]: v{Data.Version} is installed.");

            if (Data.Version == currentVersion)
            {
                return;
            }

            Logger.LogInfo("User does not have latest version, printing to console.");
            ConsoleWindow.PrintAction($"[{Data.Name}]: New version v{currentVersion} is available");
        }
        else
        {
            Logger.LogError(
                $"Failed to request latest version. Result: {webRequest.result} Error: '\"{webRequest.error}\""
            );
            ConsoleWindow.PrintError($"[{Data.Name}]: Failed to request latest version! Check log for more info.");
        }

        webRequest.Dispose();
    }
}

internal struct Data
{
    public const string Guid = "detailedplayerinfo";
    public const string Name = "DetailedPlayerInfo";

    public const string Version = "1.5.4";
    public const string WorkshopHandle = "3071950159";
    public const string GitRaw = "https://raw.githubusercontent.com/TerameTechYT/RocketMods/development/Source/";
    public const string GitVersion = GitRaw + Name + "/VERSION";

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
    public const float TemperatureOne = 274.15f;
    public const float TemperatureTwenty = 293.15f;
    public const float TemperatureThirty = 303.15f;
    public const float TemperatureFifty = 323.15f;

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