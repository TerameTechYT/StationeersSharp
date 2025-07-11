namespace BetterInventory;

[HarmonyPatch]
public static class PatchFunctions {
    private static readonly Dictionary<MethodInfo, bool> _patches = typeof(PatchFunctions).GetMethods().ToDictionary(info => info, key => false);

    [HarmonyPatch(typeof(InventoryManager), nameof(InventoryManager.SmartStow))]
    [HarmonyPrefix]
    public static void InventoryManagerSmartStow(Slot selectedSlot) {
        if (selectedSlot == null || selectedSlot.IsEmpty()) {
            return;
        }

        try {
            Functions.CustomSmartStow(ref selectedSlot);
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.Instance.LogError($"Exception in method: {currentMethod.Name}! Please press {Utilities.GetConsoleKeyCode()} and run 'slib report'!");
                Plugin.Instance.LogException(ex);
            }
        }
    }

    [HarmonyPatch(typeof(KeyManager), nameof(KeyManager.SetupKeyBindings))]
    [HarmonyPostfix]
    public static void KeyManagerSetupKeyBindings(ref Dictionary<string, ControlsGroup> ____controlsGroupLookup) {
        if (____controlsGroupLookup == null) {
            return;
        }

        try {
            foreach (KeyItem key in Data.ControlKeys) {
                ____controlsGroupLookup[key.Name] = Data.ControlsGroup;
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
    }
}