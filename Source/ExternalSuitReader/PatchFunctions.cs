// ReSharper disable InconsistentNaming

#pragma warning disable CA1305
#pragma warning disable CA1707
#pragma warning disable IDE0060

using Assets.Scripts.Objects.Clothing;
using Assets.Scripts.Objects.Motherboards;
using HarmonyLib;
using JetBrains.Annotations;

namespace ExternalSuitReader;

[HarmonyPatch]
public static class PatchFunctions
{
    [UsedImplicitly]
    [HarmonyPatch(typeof(AdvancedSuit), nameof(AdvancedSuit.CanLogicRead))]
    [HarmonyPrefix]
    public static bool AdvancedSuitCanLogicRead(AdvancedSuit __instance, ref bool __result, LogicType logicType)
    {
        if (!Functions.CanLogicRead(logicType)) return true;

        __result = true;
        return false;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(AdvancedSuit), nameof(AdvancedSuit.GetLogicValue))]
    [HarmonyPrefix]
    public static bool AdvancedSuitGetLogicValue(AdvancedSuit __instance, ref double __result, LogicType logicType)
    {
        if (!Functions.CanLogicRead(logicType)) return true;

        __result = Functions.GetLogicValue(__instance,
            Functions.GetGasTypeFromLogicType(logicType, "Output"));
        return false;
    }
}