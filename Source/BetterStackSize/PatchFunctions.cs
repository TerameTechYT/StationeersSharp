#region

using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Pipes;
using HarmonyLib;
using ThingImport;

#endregion

namespace BetterStackSize;

[HarmonyPatch]
public static class PatchFunctions {
    private static Ore? _reagentMix;

    [HarmonyPatch(typeof(ThingImporter), "RegisterThing")]
    [HarmonyPostfix]
    public static void ThingImporterRegisterThing(ref ThingImporter __instance, Thing thing) {
        Plugin.Instance?.ProcessThing(thing);
    }

    [HarmonyPatch(typeof(FurnaceBase), nameof(FurnaceBase.Awake))]
    [HarmonyPostfix]
    public static void FurnaceBaseAwake(ref FurnaceBase __instance) {
        _reagentMix ??= Prefab.AllPrefabs.Find((prefab) => prefab != null && prefab.PrefabName == "ItemReagentMix") as Ore;

        __instance.SlagPrefab = _reagentMix;
    }
}