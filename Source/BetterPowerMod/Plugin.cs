
using Assets.Scripts;
using Assets.Scripts.UI;
using BepInEx;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace BetterPowerMod;

[BepInPlugin(Data.Guid, Data.Name, Data.Version)]
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
        this.LoadConfiguration();

        this.Logger.LogInfo(Data.Name + " successfully loaded!");
        Instance = this;
        HarmonyInstance = new Harmony(Data.Guid);
        HarmonyInstance.PatchAll();
        this.Logger.LogInfo(Data.Name + " successfully patched!");

        // Thx jixxed for awesome code :)
        SceneManager.sceneLoaded += (scene, _) => {
            if (scene.name == "Base") {
                this.OnBaseLoaded().Forget();
            }
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

    public async UniTask OnBaseLoaded() {
        // Wait until game has loaded into main menu
        await UniTask.WaitUntil(() => { return MainMenu.Instance.IsVisible; });
        // Check version after main menu is visible
        await this.CheckVersion();
    }

    public async UniTask CheckVersion() {
        UnityWebRequest webRequest = await UnityWebRequest.Get(new Uri(Data.GitVersion)).SendWebRequest();
        this.Logger.LogInfo("Awaiting send web request...");

        string currentVersion = webRequest.downloadHandler.text.Trim();
        this.Logger.LogInfo("Await complete!");

        if (webRequest.result == UnityWebRequest.Result.Success) {
            this.Logger.LogInfo($"Latest version is {currentVersion}. Installed {Data.Version}");
            ConsoleWindow.Print($"[{Data.Name}]: v{Data.Version} is installed.");

            if (Data.Version == currentVersion) {
                return;
            }

            this.Logger.LogInfo("User does not have latest version, printing to console.");
            ConsoleWindow.PrintAction($"[{Data.Name}]: New version v{currentVersion} is available");
        }
        else {
            this.Logger.LogError(
                $"Failed to request latest version. Result: {webRequest.result} Error: '\"{webRequest.error}\""
            );
            ConsoleWindow.PrintError($"[{Data.Name}]: Failed to request latest version! Check log for more info.");
        }

        webRequest.Dispose();
    }
}

internal struct Data {
    public const string Guid = "bettepowermod";
    public const string Name = "BetterPowerMod";
    public const string Version = "1.0.7";
    public const string WorkshopHandle = "3234906754";
    public const string GitRaw = "https://raw.githubusercontent.com/TerameTechYT/StationeersSharp/development/Source/";
    public const string GitVersion = GitRaw + Name + "/VERSION";

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
