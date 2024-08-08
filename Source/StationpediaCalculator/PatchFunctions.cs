using Assets.Scripts.UI;
using HarmonyLib;
using JetBrains.Annotations;
using System.Collections.Generic;

namespace StationpediaCalculator;

[HarmonyPatch]
public static class PatchFunctions {

    [UsedImplicitly]
    [HarmonyPatch(typeof(Stationpedia), "ForceSearch")]
    [HarmonyPostfix]
    public static void StationpediaForceSearch(ref Stationpedia __instance, string searchText) {
        if (__instance != null && Data.CalculatorItem != null && !string.IsNullOrEmpty(searchText)) {
            Functions.CalculateSearch(ref __instance, searchText);
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(Stationpedia), "AddSearchInsertsToPool")]
    [HarmonyPostfix]
    public static void StationpediaAddSearchInsertsToPool(ref Stationpedia __instance, int numToAdd) {
        if (__instance != null) {
            Data.CalculatorItem = UnityEngine.Object.Instantiate(__instance.ListInsertPrefab, __instance.SearchContents);
            var traverse = Traverse.Create(__instance);
            var list = traverse.Field("_SPDASearchInserts").GetValue<List<SPDAListItem>>();
            list.Add(Data.CalculatorItem);

            Data.CalculatorItem.gameObject.SetActive(false);
        }
    }
}