#region

#endregion

namespace BetterWaterCombustor;

[HarmonyPatch]
public static class PatchFunctions {
		private static readonly Dictionary<MethodInfo, bool> _patches = typeof(PatchFunctions).GetMethods().ToDictionary(info => info, key => false);

		[UsedImplicitly]
		[HarmonyPatch(typeof(Atmosphere), nameof(Atmosphere.CombustForWater))]
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> AtmosphereCombustForWaterTranspiler(IEnumerable<CodeInstruction> instructions) {
				try {
						Type type = typeof(GasMixture);
						FieldInfo water = type.GetField(nameof(GasMixture.Water));
						FieldInfo steam = type.GetField(nameof(GasMixture.Steam));

						List<CodeInstruction> newInstructions = [.. instructions];
						foreach (CodeInstruction instruction in newInstructions) {
								// replace Water enum value (32), with steam enum value (1024)
								if (instruction.Is(OpCodes.Ldc_I4_S, (byte) Chemistry.GasType.Water)) {
										instruction.opcode = OpCodes.Ldc_I4;
										instruction.operand = (int) Chemistry.GasType.Steam;
								}

								// replace call to add to Water with call to add to Steam
								if (instruction.LoadsField(water, true)) {
										instruction.operand = steam;
								}
						}

						return newInstructions.AsEnumerable();
				}
				catch (Exception ex) {
						MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

						if (!_patches[currentMethod]) {
								_patches[currentMethod] = true;

								Plugin.Instance.LogError($"Exception in method: {currentMethod.Name}! Please press {Utilities.GetConsoleKeyCode()} and run 'slib report'!");
								Plugin.Instance.LogException(ex);
						}
				}

				return instructions;
		}
}