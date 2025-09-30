#region

using Assets.Scripts.Atmospherics;
using HarmonyLib;
using StationeersLibrary;
using System.Reflection;
using System.Reflection.Emit;

#endregion

namespace BetterWaterCombustor;

[HarmonyPatch]
public static class PatchFunctions {
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
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }

        return instructions;
    }
}