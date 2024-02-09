using System.Collections;
using Assets.Scripts;
using Assets.Scripts.UI;
using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine.Networking;

namespace ExternalSuitReader;

[BepInPlugin(Utilities.Plugin.Guid, Utilities.Plugin.Name, Utilities.Plugin.Version)]
[BepInProcess("rocketstation.exe")]
public class ExternalSuitReader : BaseUnityPlugin
{
    public static ExternalSuitReader Instance { get; private set; }
    public static Harmony HarmonyInstance { get; private set; }

    [UsedImplicitly]
    public void Awake()
    {
        Logger.LogInfo(Utilities.Plugin.Name + " successfully loaded!");
        Instance = this;
        HarmonyInstance = new Harmony(Utilities.Plugin.Guid);
        HarmonyInstance.PatchAll();

        Logger.LogInfo(Utilities.Plugin.Name + " successfully patched!");

        CheckVersion();
    }

    private IEnumerator CheckVersion()
    {
        var webRequest = UnityWebRequest.Get(Utilities.Plugin.GitVersion);
        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success) yield break;

        var data = webRequest.downloadHandler.text.Trim();
        if (data != Utilities.Plugin.Version)
        {
            if (MainMenu.Instance.MainMenuCanvas.isActiveAndEnabled)
                ConsoleWindow.PrintAction(
                    "New version of " + Utilities.Plugin.Name + " v" + Utilities.Plugin.Version + " is available!",
                    false);
            else
                yield return null;
        }
    }
}

internal class Utilities
{
    internal struct Plugin
    {
        public const string Guid = "externalsuitreader";
        public const string Name = "ExternalSuitReader";
        public const string Version = "1.1";
        public const string WorkshopHandle = "3071985478";
        public const string GitRaw = "https://raw.githubusercontent.com/TerameTechYT/RocketMods/development/";
        public const string GitVersion = GitRaw + Name + "/VERSION";
    }
}