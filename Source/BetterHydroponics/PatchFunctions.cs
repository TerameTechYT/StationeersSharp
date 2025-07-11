namespace BetterHydroponics;

[HarmonyPatch]
public static class PatchFunctions {
		private static readonly Dictionary<MethodInfo, bool> _patches = typeof(PatchFunctions).GetMethods().ToDictionary(info => info, key => false);

		[UsedImplicitly]
		[HarmonyPatch(typeof(HydroponicsTrayDevice), nameof(HydroponicsTrayDevice.CanLogicRead), [typeof(LogicSlotType), typeof(int)])]
		[HarmonyPostfix]
		public static void HydroponicsTrayDeviceCanLogicRead(ref HydroponicsTrayDevice __instance, ref bool __result, LogicSlotType logicSlotType, int slotId) {
				if (__instance == null) {
						return;
				}

				try {
						__result = __result || Functions.CanLogicRead(logicSlotType);
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
		[HarmonyPatch(typeof(HydroponicsTrayDevice), nameof(HydroponicsTrayDevice.GetLogicValue), [typeof(LogicSlotType), typeof(int)])]
		[HarmonyPostfix]
		public static void HydroponicsTrayDeviceGetLogicValue(ref HydroponicsTrayDevice __instance, ref double __result, LogicSlotType logicSlotType, int slotId) {
				if (__instance == null || __instance.Plant == null) {
						return;
				}

				try {
						if (!Functions.CanLogicRead(logicSlotType)) {
								return;
						}

						__result = Functions.GetLogicValue(__instance.Plant, logicSlotType, slotId);
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
		[HarmonyPatch(typeof(HydroponicsAutomated), nameof(HydroponicsAutomated.CanLogicRead), [typeof(LogicSlotType), typeof(int)])]
		[HarmonyPostfix]
		public static void HydroponicsAutomatedCanLogicRead(ref HydroponicsAutomated __instance, ref bool __result, LogicSlotType logicSlotType, int slotId) {
				if (__instance == null) {
						return;
				}

				try {
						__result = __result || Functions.CanLogicRead(logicSlotType);
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
		[HarmonyPatch(typeof(HydroponicsAutomated), nameof(HydroponicsAutomated.GetLogicValue), [typeof(LogicSlotType), typeof(int)])]
		[HarmonyPostfix]
		public static void HydroponicsAutomatedGetLogicValue(ref HydroponicsAutomated __instance, ref double __result, LogicSlotType logicSlotType, int slotId) {
				if (__instance == null || __instance.Plant == null) {
						return;
				}

				try {
						if (!Functions.CanLogicRead(logicSlotType)) {
								return;
						}

						__result = Functions.GetLogicValue(__instance.Plant, logicSlotType, slotId);
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
		[HarmonyPatch(typeof(HydroponicsStation), nameof(HydroponicsStation.CanLogicRead), [typeof(LogicSlotType), typeof(int)])]
		[HarmonyPostfix]
		public static void HydroponicsStationCanLogicRead(ref HydroponicsStation __instance, ref bool __result, LogicSlotType logicSlotType, int slotId) {
				if (__instance == null) {
						return;
				}

				try {
						__result = __result || Functions.CanLogicRead(logicSlotType);
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
		[HarmonyPatch(typeof(HydroponicsStation), nameof(HydroponicsStation.GetLogicValue), [typeof(LogicSlotType), typeof(int)])]
		[HarmonyPostfix]
		public static void HydroponicsStationGetLogicValue(ref HydroponicsStation __instance, ref double __result, LogicSlotType logicSlotType, int slotId) {
				if (__instance == null || __instance.Plant == null) {
						return;
				}

				try {
						if (!Functions.CanLogicRead(logicSlotType)) {
								return;
						}

						__result = Functions.GetLogicValue(__instance.Plant(slotId), logicSlotType, slotId);
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

		/*[UsedImplicitly]
		[DoHarmonyPatch(typeof(Device), nameof(Device.CanLogicRead), [typeof(LogicType)])]
		[HarmonyPriority(Priority.Last)]
		[HarmonyPostfix]
		public static void DeviceCanLogicRead(ref Device __instance, ref bool __result, LogicType logicType) {
				if (__instance == null) {
						return;
				}

				if (__instance is not HydroponicsTrayDevice || __instance is not HydroponicsAutomated) {
						return;
				}

				try {
						Plugin.Instance.LogDebug($"Device.CanLogicRead({logicType})");
						__result = __result || Functions.CanLogicRead(logicType);
				}
				catch (Exception ex) {
						MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

						if (!_patches[currentMethod]) {
								_patches[currentMethod] = true;

								Plugin.Instance.LogError($"Exception in method: {currentMethod.Name}! Please press {Utilities.GetConsoleKeyCode()} and run 'slib report'!");
								Plugin.Instance.LogException(ex);
						}
				}
		}*/

		/*[UsedImplicitly]
		[DoHarmonyPatch(typeof(Device), nameof(Device.GetLogicValue), [typeof(LogicType)])]
		[HarmonyPriority(Priority.Last)]
		[HarmonyPostfix]
		public static void DeviceGetLogicValue(ref Device __instance, ref double __result, LogicType logicType) {
				if (__instance == null) {
						return;
				}

				try {
						if (!Functions.CanLogicRead(logicType)) {
								return;
						}

						if (__instance is HydroponicsTrayDevice hydroponicsDevice) {
								Plugin.Instance.LogDebug($"HydroponicsTrayDevice.GetLogicValue({logicType})");
								__result = Functions.GetLogicValue(hydroponicsDevice.Plant, logicType);
						}

						if (__instance is HydroponicsAutomated hydroponicsAutomated) {
								Plugin.Instance.LogDebug($"HydroponicsAutomated.GetLogicValue({logicType})");
								__result = Functions.GetLogicValue(hydroponicsAutomated.Plant, logicType);
						}
				}
				catch (Exception ex) {
						MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

						if (!_patches[currentMethod]) {
								_patches[currentMethod] = true;

								Plugin.Instance.LogError($"Exception in method: {currentMethod.Name}! Please press {Utilities.GetConsoleKeyCode()} and run 'slib report'!");
								Plugin.Instance.LogException(ex);
						}
				}
		}*/
}