﻿#region

using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Pipes;
using HarmonyLib;
using JetBrains.Annotations;
using Objects;
using UnityEngine;

#endregion

namespace BetterPowerMod;

[HarmonyPatch]
public static class PatchFunctions {
    [UsedImplicitly]
    [HarmonyPatch(typeof(SolarPanel), nameof(SolarPanel.PowerGenerated), MethodType.Getter)]
    [HarmonyPostfix]
    public static void SolarPanelPowerGeneratedGetter(ref SolarPanel __instance, ref float __result) {
        if (Data.EnableSolarPanel && __instance != null)
            __result = Functions.GetPotentialSolarPowerGenerated(__instance);
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(SolarPanel), nameof(SolarPanel.SolarInfo))]
    [HarmonyPostfix]
    public static void SolarPanelSolarInfo(ref SolarPanel __instance, ref string __result) {
        if (GameManager.IsBatchMode)
            return; // exit as server will never be the one rendering tooltips

        if (Data.EnableSolarPanel && __instance != null)
            __result = Functions.GetSolarPanelTooltip(__instance, __result);
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(Device), nameof(Device.GetPassiveTooltip))]
    [HarmonyPostfix]
    public static void DeviceGetPassiveTooltip(ref Device __instance, ref PassiveTooltip __result,
        Collider hitCollider) {
        if (GameManager.IsBatchMode)
            return; // exit as server will never be the one rendering tooltips

        if (Data.EnableWindTurbine && __instance != null && __instance is WindTurbineGenerator generator)
            __result = Functions.GetWindTurbineTooltip(generator);
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(WindTurbineGenerator), "SetTurbineRotationSpeed")]
    [HarmonyPostfix]
    public static void WindTurbineGeneratorSetTurbineRotationSpeed(ref WindTurbineGenerator __instance, float speed) {
        if (GameManager.IsBatchMode)
            return; // exit as server will never be the one rendering the turbine (i think)

        if (Data.EnableWindTurbine && __instance != null) {
            Transform bladesTransform = Traverse.Create(__instance).Field("bladesTransform").GetValue<Transform>();

            float RPM = Functions.GetWindTurbineRPM(__instance);
            if (__instance.BaseAnimator != null) {
                __instance.BaseAnimator.SetFloat(WindTurbineGenerator.SpeedState, __instance.GenerationRate);
            }
            else if (bladesTransform != null && RPM > 0f) {
                bladesTransform.Rotate(__instance is LargeWindTurbineGenerator ? Vector3.forward : Vector3.up, RPM / 60f);
            }
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(WindTurbineGenerator), nameof(WindTurbineGenerator.GenerationRate), MethodType.Getter)]
    [HarmonyPostfix]
    public static void WindTurbineGeneratorGenerationRateGetter(ref WindTurbineGenerator __instance, ref float __result) {
        if (Data.EnableWindTurbine && __instance != null && !__instance.HasRoom) {
            __result = Functions.GetPotentialWindPowerGenerated(__instance);
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(TurbineGenerator), nameof(TurbineGenerator.GetGeneratedPower))]
    [HarmonyPostfix]
    public static void TurbineGeneratorGetGeneratedPower(ref TurbineGenerator __instance, ref float __result) {
        if (Data.EnableTurbine && __instance != null)
            __result *= 10f;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(StirlingEngine), nameof(StirlingEngine.MaxPower), MethodType.Getter)]
    [HarmonyPostfix]
    public static void StirlingEngineMaxPowerGetter(ref StirlingEngine __instance, ref MoleEnergy __result) {
        if (Data.EnableStirling && __instance != null)
            __result = new MoleEnergy(Data.StirlingEnergy);
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(PowerTransmitterOmni), nameof(PowerTransmitterOmni.GetUsedPower))]
    [HarmonyPostfix]
    public static void PowerTransmitterOmniGetUsedPower(ref PowerTransmitterOmni __instance) {
        if (Data.EnableFasterCharging && __instance != null)
            _ = Traverse.Create(__instance).Field("_maximumPowerUsage").SetValue(Data.FastChargeRate);
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(AreaPowerControl), nameof(AreaPowerControl.GetUsedPower))]
    [HarmonyPostfix]
    public static void AreaPowerControlGetUsedPower(ref AreaPowerControl __instance) {
        if (Data.EnableFasterCharging && __instance != null)
            __instance.BatteryChargeRate = Data.FastChargeRate;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(BatteryCellCharger), nameof(BatteryCellCharger.GetUsedPower))]
    [HarmonyPostfix]
    public static void BatteryCellChargerGetUsedPower(ref BatteryCellCharger __instance) {
        if (Data.EnableFasterCharging && __instance != null)
            __instance.BatteryChargeRate = Data.FastChargeRate;
    }
}