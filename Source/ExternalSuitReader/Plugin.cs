﻿#region

using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Objects.Motherboards;
using Assets.Scripts.UI;
using BepInEx;
using BepInEx.Bootstrap;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

#endregion

namespace ExternalSuitReader;

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
        await UniTask.WaitUntil(() => MainMenu.Instance.IsVisible);

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
    public const string ModGuid = "externalsuitreader";
    public const string ModName = "ExternalSuitReader";
    public const string ModVersion = "1.4.0";
    public const ulong ModHandle = 3071985478;

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

    public static readonly Dictionary<LogicType, Chemistry.GasType> LogicPairs = new() {
        { LogicType.RatioOxygenOutput, Chemistry.GasType.Oxygen },
        { LogicType.RatioLiquidOxygenOutput, Chemistry.GasType.LiquidOxygen },
        { LogicType.RatioNitrogenOutput, Chemistry.GasType.Nitrogen },
        { LogicType.RatioLiquidNitrogenOutput, Chemistry.GasType.LiquidNitrogen },
        { LogicType.RatioCarbonDioxideOutput, Chemistry.GasType.CarbonDioxide },
        { LogicType.RatioLiquidCarbonDioxideOutput, Chemistry.GasType.LiquidCarbonDioxide },
        { LogicType.RatioVolatilesOutput, Chemistry.GasType.Volatiles },
        { LogicType.RatioLiquidVolatilesOutput, Chemistry.GasType.LiquidVolatiles },
        { LogicType.RatioPollutantOutput, Chemistry.GasType.Pollutant },
        { LogicType.RatioLiquidPollutantOutput, Chemistry.GasType.LiquidPollutant },
        { LogicType.RatioNitrousOxideOutput, Chemistry.GasType.NitrousOxide },
        { LogicType.RatioLiquidNitrousOxideOutput, Chemistry.GasType.LiquidNitrousOxide },
        { LogicType.RatioSteamOutput, Chemistry.GasType.Steam },
        { LogicType.RatioWaterOutput, Chemistry.GasType.Water },
        { LogicType.RatioWaterOutput2, Chemistry.GasType.PollutedWater }
    };
}