﻿
using Assets.Scripts;
using Assets.Scripts.UI;
using BepInEx;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace ActualSolarIrradiance;

[BepInPlugin(Data.Guid, Data.Name, Data.Version)]
[BepInProcess("rocketstation.exe")]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance { get; private set; }
    public static Harmony HarmonyInstance { get; private set; }

    [UsedImplicitly]
    public void Awake()
    {
        Logger.LogInfo(Data.Name + " successfully loaded!");
        Instance = this;
        HarmonyInstance = new Harmony(Data.Guid);
        HarmonyInstance.PatchAll();
        Logger.LogInfo(Data.Name + " successfully patched!");

        // Thx jixxed for awesome code :)
        SceneManager.sceneLoaded += (scene, _) =>
        {
            if (scene.name == "Base")
            {
                OnBaseLoaded().Forget();
            }
        };
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
    public const string Guid = "actualsolarirradiance";
    public const string Name = "ActualSolarIrradiance";
    public const string Version = "1.0.3";
    public const string WorkshopHandle = "3234906754";
    public const string GitRaw = "https://raw.githubusercontent.com/TerameTechYT/RocketMods/development/Source/";
    public const string GitVersion = GitRaw + Name + "/VERSION";

    public const float OneKilowatt = 1000f;
    public const float FiveKilowatts = OneKilowatt * 5f;
    public const float TwentyKilowatts = OneKilowatt * 20f;
    public const float FiftyKilowatts = OneKilowatt * 50f;
    public const float OneHundredKilowatts = OneKilowatt * 100f;
}
