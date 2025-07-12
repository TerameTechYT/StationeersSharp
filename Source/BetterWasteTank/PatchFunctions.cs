#region

#endregion

namespace BetterWasteTank;

[HarmonyPatch]
public static class PatchFunctions {


    [UsedImplicitly]
    [HarmonyPatch(typeof(Suit), nameof(Suit.Awake))]
    [HarmonyPostfix]
    public static void SuitAwake(ref Suit __instance) {
        // recalculate max waste pressure
        if (__instance == null) {
            return;
        }

        try {
            __instance.wasteMaxPressure = Functions.GetCanisterMax(__instance.WasteTank);
        }
        catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(Thing), nameof(Thing.OnChildEnterInventory))]
    [HarmonyPostfix]
    public static void SuitOnAtmosphericTick(ref Thing __instance, DynamicThing newChild) {
        // only recalculate max waste pressure if a tank has entered
        if (__instance == null || __instance is not Suit suit || newChild is not GasCanister) {
            return;
        }

        try {
            suit.wasteMaxPressure = Functions.GetCanisterMax(suit.WasteTank);
        }
        catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(StatusUpdates), "HandleIconUpdates")]
    [HarmonyPostfix]
    public static void StatusUpdatesIsWasteCritical(ref StatusUpdates __instance, ref Suit ____suit) {
        if (__instance == null || ____suit == null || ____suit.ParentEntity == null) {
            return;
        }

        try {
            __instance.TextWaste.text = $"{Mathf.FloorToInt(Functions.GetCanisterFullRatio(____suit.WasteTank) * 100f)}%";
            __instance.TexAirTank.text = $"{Mathf.FloorToInt(Functions.GetCanisterMoles(____suit.AirTank, ____suit.ParentEntity.SpeciesClass) / ConfigData.AirTankMolesCaution * 100f)}%";
        }
        catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }

    // alarm patches
    [UsedImplicitly]
    [HarmonyPatch(typeof(StatusUpdates), nameof(StatusUpdates.IsWasteCritical))]
    [HarmonyPrefix]
    public static bool StatusUpdatesIsWasteCritical(ref bool __result, ref Suit ____suit) {
        if (____suit == null || ____suit.ParentEntity == null) {
            __result = false;
            return false;
        }

        try {
            __result = Functions.IsWasteCritical(ref ____suit);
        }
        catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }

        return false;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(StatusUpdates), nameof(StatusUpdates.IsWasteCaution))]
    [HarmonyPrefix]
    public static bool StatusUpdatesIsWasteCaution(ref bool __result, ref Suit ____suit) {
        if (____suit == null || ____suit.ParentEntity == null) {
            __result = false;
            return false;
        }

        try {
            __result = Functions.IsWasteCaution(ref ____suit);
        }
        catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }

        return false;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(StatusUpdates), nameof(StatusUpdates.IsAirTankCritical))]
    [HarmonyPrefix]
    public static bool StatusUpdatesIsAirTankCritical(ref bool __result, ref Suit ____suit) {
        if (____suit == null || ____suit.ParentEntity == null) {
            __result = false;
            return false;
        }

        try {
            __result = Functions.IsAirCritical(ref ____suit, ____suit.ParentEntity.SpeciesClass);
        }
        catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }

        return false;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(StatusUpdates), nameof(StatusUpdates.IsAirTankCaution))]
    [HarmonyPrefix]
    public static bool StatusUpdatesIsAirTankCaution(ref bool __result, ref Suit ____suit) {
        if (____suit == null || ____suit.ParentEntity == null) {
            __result = false;
            return false;
        }

        try {
            __result = Functions.IsAirCaution(ref ____suit, ____suit.ParentEntity.SpeciesClass);
        }
        catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }

        return false;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(StatusUpdates), "GetPercentageString")]
    [HarmonyPrefix]
    public static bool StatusUpdatesGetPercentageString(ref string __result, float val) {
        try {
            __result = $"{val.ToStringRounded()}%";
        }
        catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }

        return false;
    }
}