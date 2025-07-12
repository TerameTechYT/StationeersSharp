#region

#endregion

namespace StationpediaCalculator;

[HarmonyPatch]
public static class PatchFunctions {


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
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
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
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }
}