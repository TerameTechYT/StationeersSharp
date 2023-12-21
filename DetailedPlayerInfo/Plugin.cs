using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;

namespace DetailedPlayerInfo;

[BepInPlugin(Utilities.Plugin.GUID, Utilities.Plugin.NAME, Utilities.Plugin.VERSION)]
[BepInProcess("rocketstation.exe")]
public class DetailedPlayerInfo : BaseUnityPlugin
{
    public static DetailedPlayerInfo Instance { get; private set; }
    public static Harmony HarmonyInstance { get; private set; }

    [UsedImplicitly]
    public void Awake()
    {
        Logger.LogInfo(Utilities.Plugin.NAME + " successfully loaded!");
        Instance = this;
        HarmonyInstance = new Harmony(Utilities.Plugin.GUID);
        HarmonyInstance.PatchAll();

        Logger.LogInfo(Utilities.Plugin.NAME + " successfully patched!");
    }
}

public class Utilities
{
    public struct Plugin
    {
        public const string GUID = "detailedplayerinfo";
        public const string NAME = "DetailedPlayerInfo";
        public const string VERSION = "1.2";
    }
}