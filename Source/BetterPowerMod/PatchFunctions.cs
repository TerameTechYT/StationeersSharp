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
        if (!ConfigData.EnableSolarPanel || __instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            __result = Functions.GetPotentialSolarPowerGenerated(__instance);
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
    [DoHarmonyPatch(typeof(Device), nameof(Device.GetPassiveTooltip))]
    [HarmonyPriority(Priority.First)]
    [HarmonyReversePatch]
    public static PassiveTooltip DeviceGetPassiveTooltipReversePatch(Device __instance, Collider hitCollider) => throw new HarmonyReversePatchException();

    [UsedImplicitly]
    [DoHarmonyPatch(typeof(SolarPanel), nameof(SolarPanel.GetPassiveTooltip))]
    [HarmonyPrefix]
    public static bool SolarPanelGetPassiveTooltip(ref SolarPanel __instance, ref PassiveTooltip __result, Collider hitCollider) {
        if (!ConfigData.EnableSolarPanel || GameManager.IsBatchMode || __instance == null || !__instance.IsStructureCompleted) {
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

                Plugin.Instance.LogError($"Exception in method: {currentMethod.Name}! Please press {Utilities.GetConsoleKeyCode()} and run 'slib report'!");
                Plugin.Instance.LogException(ex);
            }
        }

        return true;
    }

    [UsedImplicitly]
    [DoHarmonyPatch(typeof(Device), nameof(Device.GetPassiveTooltip))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPrefix]
    public static bool DeviceGetPassiveTooltip(ref Device __instance, ref PassiveTooltip __result, Collider hitCollider) {
        if (GameManager.IsBatchMode || !ConfigData.EnableWindTurbine || __instance == null || !__instance.IsStructureCompleted || !ConfigData.WindTurbinePrefabs.Contains(__instance.PrefabName)) {
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

                Plugin.Instance.LogError($"Exception in method: {currentMethod.Name}! Please press {Utilities.GetConsoleKeyCode()} and run 'slib report'!");
                Plugin.Instance.LogException(ex);
            }
        }

        return true;
    }*/

    [UsedImplicitly]
    [HarmonyPatch(typeof(WindTurbineGenerator), "SetTurbineRotationSpeed")]
    [HarmonyPostfix]
    public static void WindTurbineGeneratorSetTurbineRotationSpeed(ref WindTurbineGenerator __instance, float speed, ref Transform ___bladesTransform) {
        if (!ConfigData.EnableWindTurbine || GameManager.IsBatchMode || __instance == null || !__instance.IsStructureCompleted) {
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

                Plugin.Instance.LogError($"Exception in method: {currentMethod.Name}! Please press {Utilities.GetConsoleKeyCode()} and run 'slib report'!");
                Plugin.Instance.LogException(ex);
            }
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(WindTurbineGenerator), nameof(WindTurbineGenerator.GenerationRate), MethodType.Getter)]
    [HarmonyPostfix]
    public static void WindTurbineGeneratorGenerationRateGetter(ref WindTurbineGenerator __instance, ref float __result) {
        if (!ConfigData.EnableWindTurbine || __instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            __result = Functions.GetPotentialWindPowerGenerated(__instance);
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
    [HarmonyPatch(typeof(TurbineGenerator), nameof(TurbineGenerator.GetGeneratedPower))]
    [HarmonyPostfix]
    public static void TurbineGeneratorGetGeneratedPower(ref TurbineGenerator __instance, ref float __result) {
        if (!ConfigData.EnableTurbine || __instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            __result *= ConfigData.TurbineMultiplier;
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
    [HarmonyPatch(typeof(StirlingEngine), nameof(StirlingEngine.MaxPower), MethodType.Getter)]
    [HarmonyPostfix]
    public static void StirlingEngineMaxPowerGetter(ref StirlingEngine __instance, ref MoleEnergy __result) {
        if (!ConfigData.EnableStirling || __instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            __result = new MoleEnergy(ConfigData.StirlingEnergy);
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
    [HarmonyPatch(typeof(PowerTransmitterOmni), nameof(PowerTransmitterOmni.GetUsedPower))]
    [HarmonyPostfix]
    public static void PowerTransmitterOmniGetUsedPower(ref PowerTransmitterOmni __instance, ref float ____maximumPowerUsage) {
        if (!ConfigData.EnableFasterCharging || __instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            ____maximumPowerUsage = ConfigData.FastChargeRate;
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
    [HarmonyPatch(typeof(AreaPowerControl), nameof(AreaPowerControl.GetUsedPower))]
    [HarmonyPostfix]
    public static void AreaPowerControlGetUsedPower(ref AreaPowerControl __instance) {
        if (!ConfigData.EnableFasterCharging || __instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            __instance.BatteryChargeRate = ConfigData.FastChargeRate;
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
    [HarmonyPatch(typeof(BatteryCellCharger), nameof(BatteryCellCharger.GetUsedPower))]
    [HarmonyPostfix]
    public static void BatteryCellChargerGetUsedPower(ref BatteryCellCharger __instance) {
        if (!ConfigData.EnableFasterCharging || __instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            __instance.BatteryChargeRate = __instance.PrefabName == ConfigData.BatteryChargerSmall ? ConfigData.FastChargeRate / 2f : ConfigData.FastChargeRate;
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
    [HarmonyPatch(typeof(WallLightBattery), nameof(WallLightBattery.GetUsedPower))]
    [HarmonyPostfix]
    public static void WallLightBatteryGetUsedPower(ref WallLightBattery __instance) {
        if (!ConfigData.EnableFasterCharging || __instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            __instance.BatteryChargeRate = ConfigData.FastChargeRate / 2f;
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
}