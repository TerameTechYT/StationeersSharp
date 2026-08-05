#region

using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Items;
using HarmonyLib;
using StationeersLibrary;
using UnityEngine;

#endregion

namespace BetterAdvancedTablet;

[HarmonyPatch]
public static class PatchFunctions {
    [HarmonyPatch(typeof(AdvancedTablet), nameof(AdvancedTablet.DeserializeSave))]
    [HarmonyPostfix]
    public static void AdvancedTabletDeserializeSavePostfix(ref AdvancedTablet __instance, ThingSaveData savedData) => Traverse.Create(__instance).Method("GetCartridge").GetValue();

    [HarmonyPatch(typeof(AdvancedTablet), "InteractWith")]
    [HarmonyPostfix]
    public static void AdvancedTabletInteractWithPostfix(ref AdvancedTablet __instance, Interactable interactable, Interaction interaction, bool doAction) {
        if (__instance == null || interactable == null || !doAction || interactable.Action == InteractableType.Activate) {
            return;
        }
        try {
            if (__instance.CartridgeSlots.Any((slot) => slot.Contains<Cartridge>()) && !__instance.CartridgeSlots[__instance.Mode].Contains<Cartridge>()) {
                __instance.InteractWith(interactable, interaction, doAction);
            }
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }

    [HarmonyPatch(typeof(Item), nameof(Item.OnUsePrimary))]
    [HarmonyPrefix]
    public static bool ItemOnUsePrimaryPrefix(ref Item __instance, Vector3 targetLocation, Quaternion targetRotation, ulong steamId, bool authoringMode) {
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

    /*[HarmonyPatch(typeof(AtmosAnalyser), "GetScannedAtmosphere")]
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
    }*/
}