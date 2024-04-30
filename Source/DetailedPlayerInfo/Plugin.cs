// ReSharper disable InconsistentNaming

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
        UnityWebRequest webRequest = UnityWebRequest.Get(new Uri(Data.GitVersion));
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
    public const string Guid = "detailedplayerinfo";
    public const string Name = "DetailedPlayerInfo";

    public const string Version = "1.5.0";
    public const string WorkshopHandle = "3071950159";
    public const string GitRaw = "https://raw.githubusercontent.com/TerameTechYT/RocketMods/development/Source/";
    public const string GitVersion = GitRaw + Name + "/VERSION";

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
}