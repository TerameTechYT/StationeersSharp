using Assets.Scripts;
using Assets.Scripts.UI;
using BepInEx;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace SolarSystem;

[BepInPlugin(Data.Guid, Data.Name, Data.Version)]
[BepInProcess("rocketstation.exe")]
public class Plugin : BaseUnityPlugin {
    public static Plugin Instance {
        get; private set;
    }
    public static Harmony HarmonyInstance {
        get; private set;
    }

    [UsedImplicitly]
    public void Awake() {
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
    public const string Guid = "solarsystem";
    public const string Name = "SolarSystem";
    public const string Version = "1.0.1";
    public const string WorkshopHandle = "";
    public const string GitRaw = "https://raw.githubusercontent.com/TerameTechYT/RocketMods/development/Source/";
    public const string GitVersion = GitRaw + Name + "/VERSION";

    // All these planets will be added!!
    public static readonly List<string> WorldOrder =
    [
        "Space",
        "Mercury",
        "Venus",
        "Earth",
        "Moon",

        "Mars",
        "Phobos",
        "Deimos",

        "Ceres",

        "Jupiter",
        "Io",
        "Europa",
        "Ganymede",
        "Callisto",

        "Saturn",
        "Mimas",
        "Enceladus",
        "Tethys",
        "Dione",
        "Rhea",
        "Titan",

        "Uranus",
        "Miranda",
        "Ariel",
        "Umbriel",
        "Titania",
        "Oberon",

        "Neptune",
        "Triton",

        "Pluto",
        "Haumea",
        "Makemake",
        "Quaoar",
        "Orcus",
        "Eris",
        "Gonggong",
        "Sedna",

        "Vulcan",
    ];
}