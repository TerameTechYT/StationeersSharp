﻿// ReSharper disable InconsistentNaming

#pragma warning disable CA2243

using Assets.Scripts;
using Assets.Scripts.UI;
using BepInEx;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections;
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
            if (scene.name.Equals("base", StringComparison.OrdinalIgnoreCase))
            {
                // I do startcoroutine and it nullrefs?
                // but this works?? wtf???
                CheckVersion().ToUniTask().Forget();
            }
        };
    }

    public IEnumerator CheckVersion()
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(Data.GitVersion);
        Logger.LogInfo("Awaiting send web request...");
        yield return webRequest.SendWebRequest();

        string currentVersion = webRequest.downloadHandler.text.Trim();
        Logger.LogInfo("Await complete!");

        while (!MainMenu.Instance.MainMenuCanvas.gameObject.activeInHierarchy)
        {
            yield return null;
        }

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            Logger.LogInfo($"Latest version is {currentVersion}. Installed {Data.Version}");
            ConsoleWindow.Print($"[{Data.Name}]: v{Data.Version} is installed.");

            if (Data.Version == currentVersion)
            {
                yield break;
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
    public const string Version = "1.0.2";
    public const string WorkshopHandle = "3234906754";
    public const string GitRaw = "https://raw.githubusercontent.com/TerameTechYT/RocketMods/development/Source/";
    public const string GitVersion = GitRaw + Name + "/VERSION";

    public const float FiveKilowatts = 5000f;
    public const float OneHundredKilowatts = 100000f;
}