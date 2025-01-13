#region

using TMPro;

#endregion

namespace BetterWasteTank;

[HarmonyPatch]
public static class PatchFunctions {
    private static readonly Dictionary<MethodInfo, bool> _patches =
        typeof(PatchFunctions).GetMethods().ToDictionary(info => info, key => false);

    [UsedImplicitly]
    [HarmonyPatch(typeof(Suit), nameof(Suit.Awake))]
    [HarmonyPostfix]
    public static void SuitAwake(ref Suit __instance) {
        // recalculate max waste pressure
        if (__instance == null)
            return;

        try {
            __instance.wasteMaxPressure = Functions.GetWasteMaxPressure(__instance).ToFloat();
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogError(ex);
            }
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(Suit), nameof(Suit.OnAtmosphericTick))]
    [HarmonyPostfix]
    public static void SuitOnAtmosphericTick(ref Suit __instance) {
        // recalculate max waste pressure
        if (__instance == null)
            return;

        try {
            __instance.wasteMaxPressure = Functions.GetWasteMaxPressure(__instance).ToFloat();
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogError(ex);
            }
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(StatusUpdates), nameof(StatusUpdates.IsWasteCritical))]
    [HarmonyPrefix]
    public static bool StatusUpdatesIsWasteCritical(ref bool __result, ref Suit ____suit) {
        if (____suit == null)
            return true;

        try {
            __result = Functions.IsWasteCritical(____suit);
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
    [HarmonyPatch(typeof(StatusUpdates), nameof(StatusUpdates.IsWasteCaution))]
    [HarmonyPrefix]
    public static bool StatusUpdatesIsWasteCaution(ref bool __result, ref Suit ____suit) {
        if (____suit == null)
            return true;

        try {
            __result = Functions.IsWasteCaution(____suit);
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
    [HarmonyPatch(typeof(StatusUpdates), "HandleIconUpdates")]
    [HarmonyPrefix]
    public static void StatusUpdatesHandleIconUpdates(ref TMP_Text ___TextWaste, ref Human ____human) {
        if (___TextWaste == null || ____human == null)
            return;

        try {
            Functions.UpdateIcons(ref ___TextWaste, ref ____human);
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogError(ex);
            }
        }
    }
}