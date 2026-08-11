#region

using Assets.Scripts.UI;
using HarmonyLib;

#endregion

namespace StationeersLibrary;

public static class Patches {
    public static event Action<string> OnMainMenuCurrentPageChanged;

    [HarmonyPatch(typeof(MainMenu), nameof(MainMenu.CurrentPageChanged))]
    [HarmonyPostfix]
    public static void MainMenuCurrentPageChangedPostfix(ref MainMenu __instance, string page) {
        Patches.OnMainMenuCurrentPageChanged?.Invoke(page);
    }
}
