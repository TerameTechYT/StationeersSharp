#region

#endregion

namespace LoulanRevived;

[HarmonyPatch]
public static class PatchFunctions {
		private static readonly Dictionary<MethodInfo, bool> _patches = typeof(PatchFunctions).GetMethods().ToDictionary(info => info, key => false);

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
						MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

						if (!_patches[currentMethod]) {
								_patches[currentMethod] = true;

								Plugin.Instance.LogError($"Exception in method: {currentMethod.Name}! Please press {Utilities.GetConsoleKeyCode()} and run 'slib report'!");
								Plugin.Instance.LogException(ex);
						}
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
						MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

						if (!_patches[currentMethod]) {
								_patches[currentMethod] = true;

								Plugin.Instance.LogError($"Exception in method: {currentMethod.Name}! Please press {Utilities.GetConsoleKeyCode()} and run 'slib report'!");
								Plugin.Instance.LogError(ex);
						}
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
						MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

						if (!_patches[currentMethod]) {
								_patches[currentMethod] = true;

								Plugin.Instance.LogError($"Exception in method: {currentMethod.Name}! Please press {Utilities.GetConsoleKeyCode()} and run 'slib report'!");
								Plugin.Instance.LogException(ex);
						}
				}
				return false;
		}*/
}