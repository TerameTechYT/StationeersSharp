#region

using ExtendedGases.Elements;
using static Assets.Scripts.Atmospherics.AtmosphereHelper;
using static Assets.Scripts.Atmospherics.Chemistry;

#endregion


namespace ExtendedGases.Patches;

[HarmonyPatch]
public static class MolePatcher {
    [HarmonyPatch(typeof(Mole), nameof(Mole.FreezingTemperature)), HarmonyPrefix]
    public static bool MoleFreezingTemperaturePrefix(GasType gasType, ref TemperatureKelvin __result) {
        if (ElementManager.TryGet(gasType, out Element element)) {
            __result = new TemperatureKelvin(element.FreezingPoint);
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(Mole), nameof(Mole.BoilingPoint)), HarmonyPrefix]
    public static bool MoleBoilingPointPrefix(GasType gasType, ref TemperatureKelvin __result) {
        if (ElementManager.TryGet(gasType, out Element element)) {
            __result = new TemperatureKelvin(element.BoilingPoint);
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(Mole), nameof(Mole.MaxLiquidTemperature)), HarmonyPrefix]
    public static bool MoleMaxLiquidTemperaturePrefix(GasType gasType, ref TemperatureKelvin __result) {
        if (ElementManager.TryGet(gasType, out Element element)) {
            __result = new TemperatureKelvin(element.FreezingPoint);
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(Mole), nameof(Mole.MinimumLiquidPressureAtMaxTemperature)), HarmonyPrefix]
    public static bool MoleMinimumLiquidPressureAtMaxTemperaturePrefix(GasType gasType, ref PressurekPa __result) {
        if (ElementManager.TryGet(gasType, out Element element)) {
            __result = new PressurekPa(element.CriticalPressure);
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(Mole), nameof(Mole.MinLiquidPressure)), HarmonyPrefix]
    public static bool MoleMinLiquidPressurePrefix(GasType gasType, ref PressurekPa __result) {
        if (ElementManager.TryGet(gasType, out Element element)) {
            __result = new PressurekPa(element.TriplePointPressure);
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(Mole), nameof(Mole.MolarVolume)), HarmonyPrefix]
    public static bool MoleMolarVolumePrefix(GasType gasType, ref VolumeLitres __result) {
        if (ElementManager.TryGet(gasType, out Element element)) {
            __result = new VolumeLitres(element.MolarVolume);
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(Mole), nameof(Mole.MolarVolume)), HarmonyPrefix]
    public static bool MoleMolarVolumePrefix(GasType gasType, ref double __result) {
        if (ElementManager.TryGet(gasType, out Element element)) {
            __result = element.MolarMass;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(Mole), nameof(Mole.LatentHeatOfVaporization)), HarmonyPrefix]
    public static bool MoleLatentHeatOfVaporizationPrefix(GasType gasType, ref double __result) {
        if (ElementManager.TryGet(gasType, out Element element)) {
            __result = element.LatentHeat;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(Mole), nameof(Mole.GetSpecificHeat)), HarmonyPrefix]
    public static bool MoleGetSpecificHeatPrefix(GasType gasType, ref double __result) {
        if (ElementManager.TryGet(gasType, out Element element)) {
            __result = element.SpecificHeat;
            return false;
        }
        return true;
    }

    /*[HarmonyPatch(typeof(Mole), nameof(Mole.HeatCapacityRatio)), HarmonyPrefix]
    public static bool MoleHeatCapacityRatioPrefix(GasType gasType, ref double __result) {
        if (ElementManager.TryGet(gasType, out Element element)) {
            __result = element;
            return false;
        }
        return true;
    }*/

    [HarmonyPatch(typeof(Mole), nameof(Mole.ThermalEfficiency)), HarmonyPrefix]
    public static bool MoleThermalEfficiencyPrefix(GasType gasType, ref double __result) {
        if (ElementManager.TryGet(gasType, out Element element)) {
            __result = element.ThermalEfficiency;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(Mole), nameof(Mole.MatterState)), HarmonyPrefix]
    public static bool MoleMatterStatePrefix(GasType gasType, ref MatterState __result) {
        if (ElementManager.TryGet(gasType, out Element element)) {
            __result = gasType == element.GasType ? AtmosphereHelper.MatterState.Gas : AtmosphereHelper.MatterState.Liquid;
            return false;
        }
        return true;
    }
}
