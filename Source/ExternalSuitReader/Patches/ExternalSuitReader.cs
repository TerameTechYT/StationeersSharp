using System;
using Assets.Scripts.Objects.Clothing;
using Assets.Scripts.Objects.Motherboards;
using HarmonyLib;
using JetBrains.Annotations;
using static Assets.Scripts.Atmospherics.Chemistry;

namespace ExternalSuitReader.Patches;

[HarmonyPatch]
public static class ExternalSuitReaderPatches
{
    [UsedImplicitly]
    [HarmonyPatch(typeof(AdvancedSuit), nameof(AdvancedSuit.CanLogicRead))]
    [HarmonyPrefix]
    public static bool AdvancedSuitCanLogicRead(AdvancedSuit __instance, ref bool __result, LogicType logicType)
    {
        var gasType = GetGasTypeFromLogicType(logicType, "Output");
        if (gasType != GasType.Undefined)
        {
            __result = true;
            return false;
        }

        return true;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(AdvancedSuit), nameof(AdvancedSuit.GetLogicValue))]
    [HarmonyPrefix]
    public static bool AdvancedSuitGetLogicValue(AdvancedSuit __instance, ref double __result, LogicType logicType)
    {
        var gasType = GetGasTypeFromLogicType(logicType, "Output");
        if (__instance.HasAtmosphere && __instance.HasReadableAtmosphere && gasType != GasType.Undefined)
        {
            __result = Convert.ToDouble(__instance.WorldAtmosphere.GetGasTypeRatio(gasType));
            return false;
        }

        return true;
    }

    public static GasType GetGasTypeFromLogicType(LogicType logicType, string endingString)
    {
        var logicTypeAsString = Enum.GetName(typeof(LogicType), logicType) ?? "";

        if (string.IsNullOrEmpty(logicTypeAsString) || !logicTypeAsString.StartsWith("Ratio") ||
            !logicTypeAsString.EndsWith(endingString)) return GasType.Undefined;

        var gasTypeString = logicTypeAsString.Replace("Ratio", "").Replace(endingString, "");

        return Enum.TryParse(gasTypeString, out GasType gasType) ? gasType : GasType.Undefined;
    }
}