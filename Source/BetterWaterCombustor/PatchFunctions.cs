#region

using Assets.Scripts.Atmospherics;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

#endregion

namespace BetterWaterCombustor;

[HarmonyPatch]
public static class PatchFunctions {
    [UsedImplicitly]
    [HarmonyPatch(typeof(Atmosphere), nameof(Atmosphere.CombustForWater))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> AtmosphereCombustForWaterTranspiler(IEnumerable<CodeInstruction> instructions) {
        foreach (CodeInstruction instruction in instructions) {
            // replace Water enum value (32), with steam enum value (1024)
            if (instruction.Is(OpCodes.Ldc_I4_S, (byte) Chemistry.GasType.Water)) {
                instruction.opcode = OpCodes.Ldc_I4;
                instruction.operand = (int) Chemistry.GasType.Steam;
            }

            // replace call to add to Water with call to add to Steam
            Type type = typeof(GasMixture);
            FieldInfo water = type.GetField(nameof(GasMixture.Water));
            FieldInfo steam = type.GetField(nameof(GasMixture.Steam));

            if (instruction.LoadsField(water, true)) {
                instruction.operand = steam;
            }
        }

        return instructions;
    }
}