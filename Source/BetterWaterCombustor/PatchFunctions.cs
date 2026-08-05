#region

using Assets.Scripts.Atmospherics;
using Assets.Scripts.Objects.Pipes;
using HarmonyLib;
using StationeersLibrary;
using StationeersLibrary.Modding;
using System.Reflection;
using System.Reflection.Emit;

#endregion

namespace BetterWaterCombustor;

[HarmonyPatch]
public static class PatchFunctions {
    // https://store.steampowered.com/news/app/544550/view/521990850610203005
    [HarmonyPatchVersionMaximum("0.2.6182.26959")]
    [HarmonyPatch(typeof(Atmosphere), "CombustForWater")]
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

    /*// https://store.steampowered.com/news/app/544550/view/491593545667313734
    [HarmonyPatchVersionMinimum("0.2.6217.27046")]
    [HarmonyPatch("CombustorMachine", "OnAtmosphericTick")]
    [HarmonyPrefix]
    public static bool CombustorMachineOnAtmosphericTick(ref CombustorMachine __instance) {
        if (__instance == null) {
            return false;
        }

        try {
            Functions.OnAtmosphericTick(ref __instance);
            return false;
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }

        return true;
    }*/
}