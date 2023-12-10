using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;

namespace NoFractionalReageants;

[BepInPlugin(Utilities.Plugin.GUID, Utilities.Plugin.NAME, Utilities.Plugin.VERSION)]
[BepInProcess("rocketstation.exe")]
public class NoFractionalReageants : BaseUnityPlugin
{
    public static NoFractionalReageants Instance { get; private set; }
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
        public const string GUID = "nofractionalreageants";
        public const string NAME = "NoFractionalReageants";
        public const string VERSION = "1.0";
    }
}