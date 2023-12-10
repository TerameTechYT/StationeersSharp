using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;

namespace InstantPlants;

[BepInPlugin(Utilities.Plugin.GUID, Utilities.Plugin.NAME, Utilities.Plugin.VERSION)]
[BepInProcess("rocketstation.exe")]
public class InstantPlants : BaseUnityPlugin
{
    public static InstantPlants Instance { get; private set; }
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
    public static T GetFieldValue<T>(Traverse traverse, string fieldName)
    {
        return traverse.Field<T>(fieldName).Value;
    }

    public struct Plugin
    {
        public const string GUID = "instantplants";
        public const string NAME = "InstantPlants";
        public const string VERSION = "1.0";
    }
}