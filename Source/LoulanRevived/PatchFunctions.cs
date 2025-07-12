#region

#endregion

namespace LoulanRevived;

[HarmonyPatch]
public static class PatchFunctions {

    /*[UsedImplicitly]
    [DoHarmonyPatch(typeof(WorldManager), "LoadGameDataAsync")]
    [HarmonyPostfix]
    public static void WorldManagerLoadGameDataAsync(ref WorldManager __instance) {
            if (!Data.SpawnWrecks || __instance == null) {
                    return;
            }

            try {
                    Functions.LoadIncidents(ref __instance);
            }
            catch (Exception ex) {
                    Utilities.ExceptionReporter(Plugin.Instance, ref ex);
            }
    }

    [UsedImplicitly]
    [DoHarmonyPatch(typeof(WorkshopMenu), "GenerateRandomIncident")]
    [HarmonyPrefix]
    public static bool TileSystemDelayIncident(ref TileSystem __instance, TileData tileData, bool onTileEnter = false)
    {
            if (!Data.SpawnWrecks || __instance == null || tileData == null)
                    return false;

            try {
                    Functions.GenerateRandomIncident(ref __instance, tileData, onTileEnter);
            }
            catch (Exception ex) {
                    Utilities.ExceptionReporter(Plugin.Instance, ref ex);
            }

            return false;
    }

    [UsedImplicitly]
    [DoHarmonyPatch(typeof(TileSystem), "DelayIncident")]
    [HarmonyPrefix]
    public static bool TileSystemDelayIncident(ref TileSystem __instance, ref IEnumerator __result,
            TileData tileData, Incident incident, int delay, WorldManager.TerrainFeatureIncident relatedValues) {
            if (!Data.SpawnWrecks || __instance == null || tileData == null || incident == null || relatedValues == null) {
                    return false;
            }

            try {
                    __result = Functions.DelayIncidentAsync(__instance, tileData, incident, delay, relatedValues).ToCoroutine();
                    return false;
            }
            catch (Exception ex) {
                    Utilities.ExceptionReporter(Plugin.Instance, ref ex);
            }
            return false;
    }*/
}