#region

#endregion

using Assets.Scripts.Objects.Items;

namespace BetterAdvancedTablet;

[HarmonyPatch]
public static class PatchFunctions {
    [UsedImplicitly]
    [HarmonyPatch(typeof(AdvancedTablet), nameof(AdvancedTablet.DeserializeSave))]
    [HarmonyPostfix]
    public static void AdvancedTabletDeserializeSave(ref AdvancedTablet __instance, ThingSaveData savedData) => Traverse.Create(__instance).Method("GetCartridge").GetValue();

    [UsedImplicitly]
    [HarmonyPatch(typeof(AdvancedTablet), nameof(AdvancedTablet.InteractWith))]
    [HarmonyPostfix]
    public static void AdvancedTabletInteractWith(ref AdvancedTablet __instance, ref Thing.DelayedActionInstance __result, ref int ___currentCartSlot, Interactable interactable, Interaction interaction, bool doAction) {
        if (__instance == null || !doAction) {
            return;
        }

        if (interactable.Action != InteractableType.Button1 && interactable.Action != InteractableType.Button2) {
            return;
        }

        try {
            if (__instance.CartridgeSlots[__instance.Mode % __instance.CartridgeSlots.Count].IsEmpty()) {
                __result = __instance.InteractWith(interactable, interaction, doAction);
            }
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(Item), nameof(Item.OnUsePrimary))]
    [HarmonyPrefix]
    public static bool ItemOnUsePrimary(Item __instance, Vector3 targetLocation, Quaternion targetRotation, ulong steamId, bool authoringMode) {
        if (__instance == null || __instance is not AdvancedTablet advancedTablet) {
            return true;
        }

        if (!advancedTablet.OnOff || !advancedTablet.Powered || !advancedTablet.CartridgeSlots.Any((slot) => slot.Contains<Cartridge>())) {
            return true;
        }

        try {
            Functions.UsePrimary(ref advancedTablet);
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }

        return true;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(AtmosAnalyser), "GetScannedAtmosphere")]
    [HarmonyPrefix]
    public static bool AtmosAnalyserGetScannedAtmosphere(ref AtmosAnalyser __instance, ref Atmosphere __result, ref string ____selectedText) {
        if (__instance == null) {
            return true;
        }

        try {
            __result = Functions.GetScannedAtmosphere(ref __instance, ref ____selectedText);
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }

        return false;
    }
}