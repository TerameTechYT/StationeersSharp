#region

using ExtendedGases.Elements;
using static Assets.Scripts.Atmospherics.AtmosphereHelper;
using static Assets.Scripts.Atmospherics.Chemistry;

#endregion


namespace ExtendedGases.Patches;

/*[HarmonyPatch]
public static class ChemistryPatcher {
    [HarmonyPatch(typeof(Chemistry), nameof(Chemistry.SpecificHeat)), HarmonyPrefix]
    public static bool ChemistrySpecificHeatPrefix(GasType gasType, ref double __result) {
        if (ElementManager.TryGet(gasType, out Element element)) {
            __result = element.SpecificHeat;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(Chemistry), nameof(Chemistry.MolarVolumeLiquid)), HarmonyPrefix]
    public static bool ChemistrySpecificHeatPrefix(GasType gasType, ref VolumeLitres __result) {
        if (ElementManager.TryGet(gasType, out Element element)) {
            __result = new VolumeLitres(element.MolarVolume);
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(Chemistry), nameof(Chemistry.MolarVolumeLiquid)), HarmonyPrefix]
    public static bool ChemistrySpecificHeatPrefix(GasType gasType, ref MatterState __result) {
        if (ElementManager.TryGet(gasType, out Element element)) {
            __result = gasType == element.GasType ? AtmosphereHelper.MatterState.Gas : AtmosphereHelper.MatterState.Liquid;
            return false;
        }
        return true;
    }
}*/