#pragma warning disable CA1707

using HarmonyLib;
using JetBrains.Annotations;

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

    //[UsedImplicitly]
    //[HarmonyPatch(typeof(TileSystem), "GenerateRandomIncident")]
    //[HarmonyPrefix]
    //public static bool TileSystemGenerateRandomIncident(ref TileSystem __instance, TileData tileData, bool onTileEnter = false)
    //{
    //    return Functions.GenerateRandomIncident(ref __instance, tileData, onTileEnter);
    //}
}