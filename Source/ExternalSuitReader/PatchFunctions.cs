#region

using Assets.Scripts.Objects.Clothing;
using Assets.Scripts.Objects.Motherboards;
using HarmonyLib;
using JetBrains.Annotations;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace ExternalSuitReader;

[HarmonyPatch]
public static class PatchFunctions {
    private static readonly Dictionary<MethodInfo, bool> _patches =
        typeof(PatchFunctions).GetMethods().ToDictionary(info => info, key => false);

    [UsedImplicitly]
    [HarmonyPatch(typeof(AdvancedSuit), nameof(AdvancedSuit.CanLogicRead))]
    [HarmonyPrefix]
    public static bool AdvancedSuitCanLogicRead(AdvancedSuit __instance, ref bool __result, LogicType logicType) {
        if (__instance == null)
            return true;

        try {
            __result = Functions.CanLogicRead(logicType);
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogError(ex);
            }
        }

        return false;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(AdvancedSuit), nameof(AdvancedSuit.GetLogicValue))]
    [HarmonyPrefix]
    public static bool AdvancedSuitGetLogicValue(AdvancedSuit __instance, ref double __result, LogicType logicType) {
        if (__instance == null)
            return true;

        try {
            if (!Functions.CanLogicRead(logicType))
                return true;

            __result = Functions.GetLogicValue(__instance, logicType);
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogError(ex);
            }
        }

        return false;
    }
}