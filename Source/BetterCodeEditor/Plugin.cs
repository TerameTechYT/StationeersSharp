﻿#region

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
using StationeersMods.Interface;

#endregion

namespace BetterCodeEditor;

[StationeersMod(Data.ModGuid, Data.ModName, Data.ModVersion)]
public class Plugin : ModBehaviour {
    public static Plugin Instance {
        get; private set;
    }

    public static Harmony HarmonyInstance {
        get; private set;
    }

    public override void OnLoaded(ContentHandler contentHandler) {
        base.OnLoaded(contentHandler);

        if (Chainloader.PluginInfos.TryGetValue(Data.ModGuid, out _))
            throw new Data.AlreadyLoadedException($"Mod {Data.ModName} ({Data.ModGuid}) - {Data.ModVersion} has already been loaded!");
        //awa.shark.plugin.MoreLinesCodeMod
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
        Data.CodeEditorLines = Config.Bind("Configurables",
            "Code Editor Lines",
            InputSourceCode.MAX_LINES,
            "Number of lines in the code editor.");

        Data.CodeEditorLineLength = Config.Bind("Configurables",
            "Code Editor Line Length",
            InputSourceCode.LINE_LENGTH_LIMIT,
            "The length of the code editor lines");
    }

    public async UniTask OnBaseLoaded() {
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
    public const string ModGuid = "bettercodeeditor";
    public const string ModName = "BetterCodeEditor";
    public const string ModVersion = "1.0.0";
    public const ulong ModHandle = 0;

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

    public static ConfigEntry<int> CodeEditorLines;
    public static ConfigEntry<int> CodeEditorLineLength;

    public static int BytesPerLine => InputSourceCode.MAX_FILE_SIZE / InputSourceCode.MAX_LINES;
    public static int MaxFileSize => BytesPerLine * (CodeEditorLines?.Value ?? InputSourceCode.MAX_LINES);
    public static int MaxLines => CodeEditorLines?.Value ?? InputSourceCode.MAX_LINES;
    public static int MaxLineLength => CodeEditorLineLength?.Value ?? InputSourceCode.LINE_LENGTH_LIMIT;
}