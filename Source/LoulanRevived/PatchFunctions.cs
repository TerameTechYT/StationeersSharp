#pragma warning disable CA1707

using Assets.Scripts;
using HarmonyLib;
using JetBrains.Annotations;
using System.Collections;

namespace LoulanRevived;

[HarmonyPatch]
public static class PatchFunctions
{
    [UsedImplicitly]
    [HarmonyPatch(typeof(WorldManager), "LoadGameDataAsync")]
    [HarmonyPostfix]
    public static void WorldManagerLoadGameDataAsync(ref WorldManager __instance)
    {
        if (Data.SpawnWrecks != null && Data.SpawnWrecks.Value)
        {
            Functions.LoadIncidents(ref __instance);
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(TileSystem), "DelayIncident")]
    [HarmonyPrefix]
    public static bool TileSystemDelayIncident(ref TileSystem __instance, ref IEnumerator __result,
        TileData tileData, Incident incident, int delay, WorldManager.TerrainFeatureIncident relatedValues)
    {
        if (Data.SpawnWrecks != null && Data.SpawnWrecks.Value)
        {
            __result = Functions.DelayIncident(tileData, incident, delay, relatedValues);
            return false;
        }

        return true;
    }
}