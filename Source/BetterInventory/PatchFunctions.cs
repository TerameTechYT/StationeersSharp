namespace BetterInventory;

[HarmonyPatch]
public static class PatchFunctions {
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
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
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
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }
}