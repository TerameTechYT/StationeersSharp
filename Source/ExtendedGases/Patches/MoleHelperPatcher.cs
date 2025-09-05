#region

using ExtendedGases.Elements;
using static Assets.Scripts.Atmospherics.Chemistry;

#endregion


namespace ExtendedGases.Patches;

[HarmonyPatch]
public static class MoleHelperPatcher {
    [HarmonyPatch(typeof(MoleHelper), "EvaporationCoefficientA"), HarmonyPrefix]
    public static bool MoleHelperEvaporationCoefficientAPrefix(GasType gasType, ref double __result) {
        if (ElementManager.TryGet(gasType, out Element element)) {
            __result = element.EvaporationCoefficientA;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(MoleHelper), "EvaporationCoefficientB"), HarmonyPrefix]
    public static bool MoleHelperEvaporationCoefficientBPrefix(GasType gasType, ref double __result) {
        if (ElementManager.TryGet(gasType, out Element element)) {
            __result = element.EvaporationCoefficientB;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(MoleHelper), nameof(MoleHelper.CanEvaporate)), HarmonyPrefix]
    public static bool MoleHelperCanEvaporatePrefix(GasType gasType, ref bool __result) {
        if (ElementManager.TryGet(gasType, out Element element)) {
            __result = gasType == element.LiquidType;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(MoleHelper), nameof(MoleHelper.CanCondense)), HarmonyPrefix]
    public static bool MoleHelperCanCondensePrefix(GasType gasType, ref bool __result) {
        if (ElementManager.TryGet(gasType, out Element element)) {
            __result = gasType == element.GasType;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(MoleHelper), nameof(MoleHelper.EvaporationType)), HarmonyPrefix]
    public static bool MoleHelperEvaporationTypePrefix(GasType gasType, ref GasType __result) {
        if (ElementManager.TryGet(gasType, out  Element element)) {
            __result = element.GasType;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(MoleHelper), nameof(MoleHelper.CondensationType)), HarmonyPrefix]
    public static bool MoleHelperCondensationTypePrefix(GasType gasType, ref GasType __result) {
        if (ElementManager.TryGet(gasType, out Element element)) {
            __result = element.LiquidType;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(MoleHelper), nameof(MoleHelper.FreezeType)), HarmonyPrefix]
    public static bool MoleHelperFreezeTypePrefix(GasType gasType, ref GasType __result) {
        if (ElementManager.TryGet(gasType, out Element element)) {
            __result = element.LiquidType;
            return false;
        }
        return true;
    }
}
