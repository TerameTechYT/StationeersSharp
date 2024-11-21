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

namespace BetterPowerMod;

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
        Data.EnableSolarPanel = this.Config.Bind("Configurables",
            "Solar Panel Patches",
            true,
            "Should the max power output be set to the worlds Solar Irradiance");
        Data.EnableWindTurbine = this.Config.Bind("Configurables",
            "Wind Turbine Patches",
            true,
            "Should the max power output be set higher based on the atmospheric pressure");

        Data.EnableTurbine = this.Config.Bind("Configurables",
            "Wall Turbine Patches",
            true,
            "Should the max power output be multipled by 10");

        Data.EnableStirling = this.Config.Bind("Configurables",
            "Stirling Patches",
            true,
            $"Should the max power output be set to {Data.TwentyKilowatts} like the gas fuel generator");

        Data.EnableFasterCharging = this.Config.Bind("Configurables",
            "Charging Patches",
            true,
            $"Should the max input power of (Area Power Controller, Small and Large Battery Charger, Omni Power Transmitter) be set to {Data.TwoAndAHalfKilowatts}");
    }

    public static async UniTask OnBaseLoaded() {
        // Wait until game has loaded into main menu
        await UniTask.WaitUntil(() => MainMenu.Instance.IsVisible);

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
    public const string ModGuid = "betterpowermod";
    public const string ModName = "BetterPowerMod";
    public const string ModVersion = "1.1.0";
    public const ulong ModHandle = 3234916147;

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