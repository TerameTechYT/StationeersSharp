#region

#endregion

namespace BetterPowerMod;

[HarmonyPatch]
public static class PatchFunctions {
    private static readonly Dictionary<MethodInfo, bool> _patches = typeof(PatchFunctions).GetMethods().ToDictionary(info => info, key => false);

    [UsedImplicitly]
    [HarmonyPatch(typeof(SolarPanel), nameof(SolarPanel.PowerGenerated), MethodType.Getter)]
    [HarmonyPostfix]
    public static void SolarPanelPowerGeneratedGetter(ref SolarPanel __instance, ref float __result) {
        if (!Data.EnableSolarPanel || __instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            __result = Functions.GetPotentialSolarPowerGenerated(__instance);
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please press {Utilities.GetConsoleKeyCode()} and run 'slib report'!");
                Plugin.LogException(ex);
            }
        }
    }

    /*[UsedImplicitly]
    [HarmonyPatch(typeof(Device), nameof(Device.GetPassiveTooltip))]
    [HarmonyPriority(Priority.First)]
    [HarmonyReversePatch]
    public static PassiveTooltip DeviceGetPassiveTooltipReversePatch(Device __instance, Collider hitCollider) => throw new HarmonyReversePatchException();

    [UsedImplicitly]
    [HarmonyPatch(typeof(SolarPanel), nameof(SolarPanel.GetPassiveTooltip))]
    [HarmonyPrefix]
    public static bool SolarPanelGetPassiveTooltip(ref SolarPanel __instance, ref PassiveTooltip __result, Collider hitCollider) {
        if (!Data.EnableSolarPanel || GameManager.IsBatchMode || __instance == null || !__instance.IsStructureCompleted) {
            return true; // exit as server will never be the one rendering tooltips
        }

        try {
            __result = Functions.GetSolarPanelTooltip(__instance, hitCollider);

            return false;
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please press {Utilities.GetConsoleKeyCode()} and run 'slib report'!");
                Plugin.LogException(ex);
            }
        }

        return true;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(Device), nameof(Device.GetPassiveTooltip))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPrefix]
    public static bool DeviceGetPassiveTooltip(ref Device __instance, ref PassiveTooltip __result, Collider hitCollider) {
        if (GameManager.IsBatchMode || !Data.EnableWindTurbine || __instance == null || !__instance.IsStructureCompleted || !Data.WindTurbinePrefabs.Contains(__instance.PrefabName)) {
            return true; // exit as server will never be the one rendering tooltips
        }

        try {
            __result = Functions.GetWindTurbineTooltip(__instance as WindTurbineGenerator, hitCollider);

            return false;
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please press {Utilities.GetConsoleKeyCode()} and run 'slib report'!");
                Plugin.LogException(ex);
            }
        }

        return true;
    }*/

    [UsedImplicitly]
    [HarmonyPatch(typeof(WindTurbineGenerator), "SetTurbineRotationSpeed")]
    [HarmonyPostfix]
    public static void WindTurbineGeneratorSetTurbineRotationSpeed(ref WindTurbineGenerator __instance, float speed, ref Transform ___bladesTransform) {
        if (!Data.EnableWindTurbine || GameManager.IsBatchMode || __instance == null || !__instance.IsStructureCompleted) {
            return; // exit as server will never be the one rendering the turbine (i think)
        }

        try {
            if (speed > 0f) {
                __instance.BaseAnimator?.SetFloat(WindTurbineGenerator.SpeedState, speed);
                ___bladesTransform?.Rotate(__instance is LargeWindTurbineGenerator ? Vector3.forward : Vector3.up, 720f * GameManager.DeltaTime * speed);
            }
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please press {Utilities.GetConsoleKeyCode()} and run 'slib report'!");
                Plugin.LogException(ex);
            }
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(WindTurbineGenerator), nameof(WindTurbineGenerator.GenerationRate), MethodType.Getter)]
    [HarmonyPostfix]
    public static void WindTurbineGeneratorGenerationRateGetter(ref WindTurbineGenerator __instance, ref float __result) {
        if (!Data.EnableWindTurbine || __instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            __result = Functions.GetPotentialWindPowerGenerated(__instance);
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please press {Utilities.GetConsoleKeyCode()} and run 'slib report'!");
                Plugin.LogException(ex);
            }
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(TurbineGenerator), nameof(TurbineGenerator.GetGeneratedPower))]
    [HarmonyPostfix]
    public static void TurbineGeneratorGetGeneratedPower(ref TurbineGenerator __instance, ref float __result) {
        if (!Data.EnableTurbine || __instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            __result *= Data.TurbineMultiplier;
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please press {Utilities.GetConsoleKeyCode()} and run 'slib report'!");
                Plugin.LogException(ex);
            }
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(StirlingEngine), nameof(StirlingEngine.MaxPower), MethodType.Getter)]
    [HarmonyPostfix]
    public static void StirlingEngineMaxPowerGetter(ref StirlingEngine __instance, ref MoleEnergy __result) {
        if (!Data.EnableStirling || __instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            __result = new MoleEnergy(Data.StirlingEnergy);
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please press {Utilities.GetConsoleKeyCode()} and run 'slib report'!");
                Plugin.LogException(ex);
            }
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(PowerTransmitterOmni), nameof(PowerTransmitterOmni.GetUsedPower))]
    [HarmonyPostfix]
    public static void PowerTransmitterOmniGetUsedPower(ref PowerTransmitterOmni __instance, ref float ____maximumPowerUsage) {
        if (!Data.EnableFasterCharging || __instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            ____maximumPowerUsage = Data.FastChargeRate;
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please press {Utilities.GetConsoleKeyCode()} and run 'slib report'!");
                Plugin.LogException(ex);
            }
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(AreaPowerControl), nameof(AreaPowerControl.GetUsedPower))]
    [HarmonyPostfix]
    public static void AreaPowerControlGetUsedPower(ref AreaPowerControl __instance) {
        if (!Data.EnableFasterCharging || __instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            __instance.BatteryChargeRate = Data.FastChargeRate;
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please press {Utilities.GetConsoleKeyCode()} and run 'slib report'!");
                Plugin.LogException(ex);
            }
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(BatteryCellCharger), nameof(BatteryCellCharger.GetUsedPower))]
    [HarmonyPostfix]
    public static void BatteryCellChargerGetUsedPower(ref BatteryCellCharger __instance) {
        if (!Data.EnableFasterCharging || __instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            __instance.BatteryChargeRate = __instance.PrefabName == Data.BatteryChargerSmall ? Data.FastChargeRate / 2f : Data.FastChargeRate;
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please press {Utilities.GetConsoleKeyCode()} and run 'slib report'!");
                Plugin.LogException(ex);
            }
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(WallLightBattery), nameof(WallLightBattery.GetUsedPower))]
    [HarmonyPostfix]
    public static void WallLightBatteryGetUsedPower(ref WallLightBattery __instance) {
        if (!Data.EnableFasterCharging || __instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            __instance.BatteryChargeRate = Data.FastChargeRate / 2f;
        }
        catch (Exception ex) {
            MethodInfo currentMethod = (MethodInfo) MethodBase.GetCurrentMethod();

            if (!_patches[currentMethod]) {
                _patches[currentMethod] = true;

                Plugin.LogError($"Exception in method: {currentMethod.Name}! Please press {Utilities.GetConsoleKeyCode()} and run 'slib report'!");
                Plugin.LogException(ex);
            }
        }
    }
}