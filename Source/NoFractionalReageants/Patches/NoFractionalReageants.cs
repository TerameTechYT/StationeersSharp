using System;
using HarmonyLib;
using JetBrains.Annotations;
using Reagents;

namespace NoFractionalReageants.Patches;

[HarmonyPatch]
public static class NoFractionalReageants
{
    [UsedImplicitly]
    [HarmonyPatch(typeof(ReagentMixture), "GetQuantity")]
    [HarmonyPostfix]
    public static void ReagentMixtureGetQuantity(ref double __result)
    {
        __result = Math.Round(__result, 0);
    }
}