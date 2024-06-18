// ReSharper disable InconsistentNaming

#pragma warning disable CA1707

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
    [HarmonyPatch(typeof(Suit), nameof(Suit.Awake))]
    [HarmonyPostfix]
    public static void SuitAwake(ref Suit __instance)
    {
        __instance.WasteMaxPressure = Functions.GetWasteMaxPressure(__instance);
    }


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
}