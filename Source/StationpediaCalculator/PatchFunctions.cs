#region

#endregion

namespace StationpediaCalculator;

[HarmonyPatch]
public static class PatchFunctions {
    private static readonly Dictionary<MethodInfo, bool> _patches = typeof(PatchFunctions).GetMethods().ToDictionary(info => info, key => false);

    [UsedImplicitly]
    [HarmonyPatch(typeof(Stationpedia), "ForceSearch")]
    [HarmonyPostfix]
    public static void StationpediaForceSearch(ref Stationpedia __instance, string searchText) {
        if (Stationpedia.Instance == null || ConfigData.CalculatorItem == null || string.IsNullOrEmpty(searchText)) {
            return;
        }

        try {
            Functions.CalculateSearch(searchText);
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

    [UsedImplicitly]
    [HarmonyPatch(typeof(Stationpedia), "AddSearchInsertsToPool")]
    [HarmonyPostfix]
    public static void StationpediaAddSearchInsertsToPool(ref Stationpedia __instance, int numToAdd) {
        if (Stationpedia.Instance == null) {
            return;
        }

        try {
            Traverse traverse = Traverse.Create(Stationpedia.Instance);
            List<SPDAListItem> list = traverse.Field("_SPDASearchInserts").GetValue<List<SPDAListItem>>();

            if (list == null) {
                return;
            }

            Functions.CreateCalculator(ref list);
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