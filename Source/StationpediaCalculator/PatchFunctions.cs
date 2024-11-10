#region

using Assets.Scripts.UI;
using HarmonyLib;
using JetBrains.Annotations;
using System.Collections.Generic;

#endregion

namespace StationpediaCalculator;

[HarmonyPatch]
public static class PatchFunctions {
    [UsedImplicitly]
    [HarmonyPatch(typeof(Stationpedia), "ForceSearch")]
    [HarmonyPostfix]
    public static void StationpediaForceSearchref(ref Stationpedia __instance, string searchText) => Functions.CalculateSearch(ref Data.CalculatorItem, searchText);

    [UsedImplicitly]
    [HarmonyPatch(typeof(Stationpedia), "AddSearchInsertsToPool")]
    [HarmonyPostfix]
    public static void StationpediaAddSearchInsertsToPool(ref Stationpedia __instance, int numToAdd) {
        Traverse traverse = Traverse.Create(Stationpedia.Instance);
        List<SPDAListItem> list = traverse.Field("_SPDASearchInserts").GetValue<List<SPDAListItem>>();

        Functions.CreateCalculator(ref list);
    }
}