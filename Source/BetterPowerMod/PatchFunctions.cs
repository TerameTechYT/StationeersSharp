#region

using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Networks;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.Objects.Structures;
using Assets.Scripts.Util;
using HarmonyLib;
using JetBrains.Annotations;
using Objects;
using StationeersLibrary;
using UnityEngine;

#endregion

namespace BetterPowerMod;

[HarmonyPatch]
public static class PatchFunctions {
    [HarmonyPatch(typeof(SolarPanel), nameof(SolarPanel.PowerGenerated))]
    [HarmonyPrefix]
    public static void SolarPowerGenerated(ref SolarPanel __instance) {
        if (!ConfigData.EnableSolarPanel || __instance == null) {
            return;
        }

        try {
            __instance.MaxPowerGenerated = Functions.GetPotentialSolarPowerGenerated(ref __instance);
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }

    [HarmonyPatch(typeof(SolarPanel), nameof(SolarPanel.SolarInfo))]
    [HarmonyPrefix]
    public static bool SolarPanelGetSolarPanelInfo(ref SolarPanel __instance, ref string __result) {
        if (!ConfigData.EnableSolarPanel || GameManager.IsBatchMode || __instance == null || !__instance.IsStructureCompleted) {
            return true;
        }

        try {
            __result = Functions.GetSolarPanelInfo(ref __instance);

            return false;
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }

        return true;
    }

    [HarmonyPatch(typeof(WindTurbineGenerator), nameof(WindTurbineGenerator.GetWorldAtmospherePressureClamped))]
    [HarmonyPostfix]
    public static void WindTurbineGeneratorGetWorldAtmospherePressureClamped(ref WindTurbineGenerator __instance, ref PressurekPa __result) {
        if (!ConfigData.EnableWindTurbine || __instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            PressurekPa pressure = __instance.GetWorldAtmospherePressure();
            __result = pressure > PressurekPa.One ? PressurekPa.Zero : RocketMath.Clamp(pressure, PressurekPa.One, PressurekPa.One * Constants.ONE_ATMOSPHERE_PRESSURE_KPA);
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }

    [HarmonyPatch(typeof(TurbineGenerator), nameof(TurbineGenerator.GetGeneratedPower))]
    [HarmonyPostfix]
    public static void TurbineGeneratorGetGeneratedPower(ref TurbineGenerator __instance, ref float __result, CableNetwork cableNetwork) {
        if (!ConfigData.EnableTurbine || __instance == null || !__instance.IsStructureCompleted || __instance.PowerCableNetwork != cableNetwork) {
            return;
        }

        try {
            __result *= ConfigData.TurbineMultiplier;
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }

    [HarmonyPatch(typeof(StirlingEngine), nameof(StirlingEngine.MaxPower), MethodType.Getter)]
    [HarmonyPostfix]
    public static void StirlingEngineMaxPowerGetter(ref StirlingEngine __instance, ref MoleEnergy __result) {
        if (!ConfigData.EnableStirling || __instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            __result = new MoleEnergy(ConfigData.StirlingEnergy);
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }

    [HarmonyPatch(typeof(PowerTransmitterOmni), nameof(PowerTransmitterOmni.GetUsedPower))]
    [HarmonyPostfix]
    public static void PowerTransmitterOmniGetUsedPower(ref PowerTransmitterOmni __instance, ref float ____maximumPowerUsage) {
        if (!ConfigData.EnableFasterCharging || __instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            ____maximumPowerUsage = ConfigData.FastChargeRate;
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }

    [HarmonyPatch(typeof(AreaPowerControl), nameof(AreaPowerControl.GetUsedPower))]
    [HarmonyPostfix]
    public static void AreaPowerControlGetUsedPower(ref AreaPowerControl __instance) {
        if (!ConfigData.EnableFasterCharging || __instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            __instance.BatteryChargeRate = ConfigData.FastChargeRate;
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }

    [HarmonyPatch(typeof(BatteryCellCharger), nameof(BatteryCellCharger.GetUsedPower))]
    [HarmonyPostfix]
    public static void BatteryCellChargerGetUsedPower(ref BatteryCellCharger __instance) {
        if (!ConfigData.EnableFasterCharging || __instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            __instance.BatteryChargeRate = __instance.PrefabName == ConfigData.BatteryChargerSmall ? ConfigData.FastChargeRate / 2f : ConfigData.FastChargeRate;
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }

    [HarmonyPatch(typeof(WallLightBattery), nameof(WallLightBattery.GetUsedPower))]
    [HarmonyPostfix]
    public static void WallLightBatteryGetUsedPower(ref WallLightBattery __instance) {
        if (!ConfigData.EnableFasterCharging || __instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            __instance.BatteryChargeRate = ConfigData.FastChargeRate / 2f;
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }
}