// ReSharper disable InconsistentNaming

#pragma warning disable CA1707

using Assets.Scripts.Atmospherics;
using Assets.Scripts.Objects.Clothing;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.UI;
using HarmonyLib;
using JetBrains.Annotations;
using TMPro;

namespace BetterWasteTank;

[HarmonyPatch]
public static class PatchFunctions
{
    [UsedImplicitly]
    [HarmonyPatch(typeof(StatusUpdates), nameof(StatusUpdates.IsWasteCritical))]
    [HarmonyPostfix]
    public static void StatusUpdatesIsWasteCritical(ref bool __result, ref Suit ____suit)
    {
        __result = Functions.IsWasteCritical(____suit);
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(StatusUpdates), nameof(StatusUpdates.IsWasteCaution))]
    [HarmonyPostfix]
    public static void StatusUpdatesIsWasteCaution(ref bool __result, ref Suit ____suit)
    {
        __result = Functions.IsWasteCaution(____suit);
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(StatusUpdates), "HandleIconUpdates")]
    [HarmonyPostfix]
    public static void StatusUpdatesHandleIconUpdates(ref TMP_Text ___TextWaste, ref Human ____human)
    {
        Functions.UpdateIcons(ref ___TextWaste, ref ____human);
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(InternalAtmosphereConditioner), "AirConditioning")]
    [HarmonyPrefix]
    public static bool InternalAtmosphereConditionerAirConditioning(InternalAtmosphereConditioner __instance,
        ref float __result, ref Atmosphere selectedAtmosphere)
    {
        if (__instance == null || __instance.Thing is not Suit || selectedAtmosphere == null) return true;

        if (!__instance.OnOff || __instance.Battery == null || __instance.WasteTank == null ||
            __instance.Battery.IsEmpty)
            return true;

        if (__instance.WasteTank.InternalAtmosphere.PressureGasses >= __instance.WasteMaxPressure) return true;

        __result = Functions.SuitAirConditioner(ref __instance, ref selectedAtmosphere);
        return false;
    }
}