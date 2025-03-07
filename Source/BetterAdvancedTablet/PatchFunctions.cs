#region

#endregion

namespace BetterAdvancedTablet;

[HarmonyPatch]
public static class PatchFunctions {
    private static readonly Dictionary<MethodInfo, bool> _patches = typeof(PatchFunctions).GetMethods().ToDictionary(info => info, key => false);

    [UsedImplicitly]
    [HarmonyPatch(typeof(Prefab), nameof(Prefab.LoadAll))]
    [HarmonyPrefix]
    public static bool PrefabLoadAll() {
        try {
            AdvancedTablet tabletPrefab = WorldManager.Instance.SourcePrefabs.Find((thing) => thing is AdvancedTablet) as AdvancedTablet;
            tabletPrefab.AllowSelfUse = true;

            Slot template = tabletPrefab.Slots.Find((slot) => slot.Type == Slot.Class.Cartridge);
            for (int i = 0; i < Data.AdditionalTabletSlots; i++) {
                tabletPrefab.Slots.Add(Functions.CloneSlot(template));
            }
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogException(ex);
            }
        }

        return true;
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
            Functions.ToNextCartridge(ref advancedTablet);
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogException(ex);
            }
        }

        return true;
    }

    /*[UsedImplicitly]
    [HarmonyPatch(typeof(AtmosAnalyser), "GetScannedAtmosphere")]
    [HarmonyPrefix]
    public static bool AtmosAnalyserGetScannedAtmosphere(AtmosAnalyser __instance, ref Atmosphere __result, ref string ____selectedText) {
        if (__instance == null) {
            return true;
        }

        try {
            __result = Functions.GetScannedAtmosphere(ref __instance, ref ____selectedText);
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogException(ex);
            }
        }

        return false;
    }*/

    /*[UsedImplicitly]
    [HarmonyPatch(typeof(KeyManager), nameof(KeyManager.SetupKeyBindings))]
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

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please Press F3 and type 'log' and report it to github.");
                Plugin.LogException(ex);
            }
        }
    }*/
}