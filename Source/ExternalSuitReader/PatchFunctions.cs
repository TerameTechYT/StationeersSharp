#region

using Assets.Scripts.Objects.Clothing;
using Assets.Scripts.Objects.Motherboards;
using HarmonyLib;
using JetBrains.Annotations;

#endregion

namespace ExternalSuitReader;

[HarmonyPatch]
public static class PatchFunctions {
    [UsedImplicitly]
    [HarmonyPatch(typeof(AdvancedSuit), nameof(AdvancedSuit.CanLogicRead))]
    [HarmonyPrefix]
    public static bool AdvancedSuitCanLogicRead(AdvancedSuit __instance, ref bool __result, LogicType logicType) {
        if (!Functions.CanLogicRead(logicType)) return true;

        __result = true;
        return false;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(AdvancedSuit), nameof(AdvancedSuit.GetLogicValue))]
    [HarmonyPrefix]
    public static bool AdvancedSuitGetLogicValue(AdvancedSuit __instance, ref double __result, LogicType logicType) {
        if (!Functions.CanLogicRead(logicType)) return true;

        __result = Functions.GetLogicValue(__instance, logicType);
        return false;
    }
}