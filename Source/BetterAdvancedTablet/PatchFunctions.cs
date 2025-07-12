#region

#endregion

namespace BetterAdvancedTablet;

[HarmonyPatch]
public static class PatchFunctions {
    [UsedImplicitly]
    [HarmonyPatch(typeof(Prefab), nameof(Prefab.LoadAll))]
    [HarmonyPrefix]
    public static bool PrefabLoadAll() {
        try {
            AdvancedTablet? tabletPrefab = WorldManager.Instance.SourcePrefabs.Find((thing) => thing.PrefabName == ConfigData.AdvancedTabletPrefabName) as AdvancedTablet;
            if (tabletPrefab == null) {
                return true;
            }

            Plugin.Instance.LogDebug($"Found {ConfigData.AdvancedTabletPrefabName} Prefab!");
            tabletPrefab.AllowSelfUse = true;

            Slot template = tabletPrefab.Slots.Find((slot) => slot.Type == Slot.Class.Cartridge);
            for (int i = 0; i < ConfigData.AdditionalTabletSlots; i++) {
                tabletPrefab.Slots.Add(Functions.CloneSlot(template));
            }
            Plugin.Instance.LogDebug($"Added {ConfigData.AdditionalTabletSlots} slots to {ConfigData.AdvancedTabletPrefabName} Prefab");
        }
        catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }

        return true;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(AdvancedTablet), nameof(AdvancedTablet.DeserializeSave))]
    [HarmonyPostfix]
    public static void AdvancedTabletDeserializeSave(ref AdvancedTablet __instance, ThingSaveData savedData) => Traverse.Create(__instance).Method("GetCartridge").GetValue();

    [UsedImplicitly]
    [HarmonyPatch(typeof(AdvancedTablet), nameof(AdvancedTablet.InteractWith))]
    [HarmonyPrefix]
    public static bool AdvancedTabletInteractWith(ref AdvancedTablet __instance, ref Thing.DelayedActionInstance __result, ref int ___currentCartSlot, Interactable interactable, Interaction interaction, bool doAction = true) {
        if (__instance == null || interactable == null || !doAction) {
            return true;
        }

        try {
            switch (interactable.Action) {
                case InteractableType.Button1: {
                    ___currentCartSlot = Functions.GetTabletCartridgeSlot(ref __instance, ___currentCartSlot, true);
                }
                break;
                case InteractableType.Button2: {
                    ___currentCartSlot = Functions.GetTabletCartridgeSlot(ref __instance, ___currentCartSlot, false);
                }
                break;
                default: {
                    Plugin.Instance.LogDebug($"Ignoring action type {interactable.Action}");
                    break;
                }
            }
        }
        catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }

        return true;
    }

    /*[UsedImplicitly]
    [DoHarmonyPatch(typeof(Item), nameof(Item.OnUsePrimary))]
    [HarmonyPrefix]
    public static bool ItemOnUsePrimary(Item __instance, Vector3 targetLocation, Quaternion targetRotation, ulong steamId, bool authoringMode) {
            if (__instance == null || __instance is not AdvancedTablet advancedTablet) {
                    return true;
            }

            if (!advancedTablet.OnOff || !advancedTablet.Powered || !advancedTablet.CartridgeSlots.Any((slot) => slot.Contains<Cartridge>())) {
                    return true;
            }

            try {
                    Functions.ToNextCartridge(ref advancedTablet);
            }
            catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
            }

            return true;
    }*/

    /*[UsedImplicitly]
    [DoHarmonyPatch(typeof(AtmosAnalyser), "GetScannedAtmosphere")]
    [HarmonyPrefix]
    public static bool AtmosAnalyserGetScannedAtmosphere(AtmosAnalyser __instance, ref Atmosphere __result, ref string ____selectedText) {
            if (__instance == null) {
                    return true;
            }

            try {
                    __result = Functions.GetScannedAtmosphere(ref __instance, ref ____selectedText);
            }
            catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
            }

            return false;
    }*/

    /*[UsedImplicitly]
    [DoHarmonyPatch(typeof(KeyManager), nameof(KeyManager.SetupKeyBindings))]
    [HarmonyPostfix]
    public static void KeyManagerSetupKeyBindings(ref Dictionary<string, ControlsGroup> ____controlsGroupLookup) {
            if (____controlsGroupLookup == null) {
                    return;
            }

            try {
                    ControlsGroup group = Functions.RegisterControlsGroup();
                    List<KeyItem> keys = Functions.RegisterKeys();

                    foreach (KeyItem key in keys) {
                            ____controlsGroupLookup[key.Name] = group;
                            KeyManager.KeyItemLookup[key.Name] = key;
                            KeyManager.AllKeys.Add(key);
                    }
            }
            catch (Exception ex) {
                    MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

                    if (!_patches[currentMethod]) {
                            _patches[currentMethod] = true;

                            Plugin.Instance.LogError($"Exception in method: {currentMethod.Name}! Please press {Utilities.GetConsoleKeyCode()} and run 'slib report'!");
                            Plugin.Instance.LogException(ex);
                    }
            }
    }*/
}