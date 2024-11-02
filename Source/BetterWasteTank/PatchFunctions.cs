#region

using Assets.Scripts.Objects.Clothing;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.UI;
using HarmonyLib;
using JetBrains.Annotations;
using TMPro;

#endregion

namespace BetterWasteTank;

[HarmonyPatch]
public static class PatchFunctions {
    [UsedImplicitly]
    [HarmonyPatch(typeof(Suit), nameof(Suit.Awake))]
    [HarmonyPostfix]
    public static void SuitAwake(ref Suit __instance) {
        // recalculate max waste pressure
        if (__instance != null)
            __instance.wasteMaxPressure = Functions.GetWasteMaxPressure(__instance);
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(Suit), nameof(Suit.OnAtmosphericTick))]
    [HarmonyPostfix]
    public static void SuitOnAtmosphericTick(ref Suit __instance) {
        // recalculate max waste pressure
        if (__instance != null)
            __instance.wasteMaxPressure = Functions.GetWasteMaxPressure(__instance);
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(StatusUpdates), nameof(StatusUpdates.IsWasteCritical))]
    [HarmonyPrefix]
    public static bool StatusUpdatesIsWasteCritical(ref bool __result, ref Suit ____suit) {
        __result = Functions.IsWasteCritical(____suit);

        return false;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(StatusUpdates), nameof(StatusUpdates.IsWasteCaution))]
    [HarmonyPrefix]
    public static bool StatusUpdatesIsWasteCaution(ref bool __result, ref Suit ____suit) {
        __result = Functions.IsWasteCaution(____suit);

        return false;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(StatusUpdates), "HandleIconUpdates")]
    [HarmonyPrefix]
    public static void StatusUpdatesHandleIconUpdates(ref TMP_Text ___TextWaste, ref Human ____human) {
        Functions.UpdateIcons(ref ___TextWaste, ref ____human);
    }
}