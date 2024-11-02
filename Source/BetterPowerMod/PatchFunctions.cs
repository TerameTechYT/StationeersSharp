#region

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
        if (Data.EnableSolarPanel.Value && __instance != null)
            __result = Functions.GetPotentialSolarPowerGenerated();
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(Device), nameof(Device.GetPassiveTooltip))]
    [HarmonyPostfix]
    public static void DeviceGetPassiveTooltip(ref Device __instance, ref PassiveTooltip __result,
        Collider hitCollider) {
        if (Data.EnableWindTurbine.Value && __instance != null && __instance is WindTurbineGenerator generator)
            __result = Functions.GetWindTurbineTooltip(generator);
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(WindTurbineGenerator), "SetTurbineRotationSpeed")]
    [HarmonyPostfix]
    public static void WindTurbineGeneratorSetTurbineRotationSpeed(ref WindTurbineGenerator __instance, float speed) {
        if (Data.EnableWindTurbine.Value && __instance != null) {
            var traverse = Traverse.Create(__instance);
            var bladesTransform = traverse.Field("bladesTransform").GetValue<Transform>();

            var RPM = Functions.GetWindTurbineRPM(__instance);
            if (__instance.BaseAnimator != null)
                __instance.BaseAnimator.SetFloat(WindTurbineGenerator.SpeedState, __instance.GenerationRate);
            else if (bladesTransform != null && RPM > 0f)
                bladesTransform.Rotate(__instance is LargeWindTurbineGenerator ? Vector3.forward : Vector3.up,
                    RPM / 60f);
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(WindTurbineGenerator), nameof(WindTurbineGenerator.GenerationRate), MethodType.Getter)]
    [HarmonyPostfix]
    public static void WindTurbineGeneratorGenerationRate(ref WindTurbineGenerator __instance, ref float __result) {
        if (Data.EnableWindTurbine.Value && __instance != null && __instance.HasRoom != null) {
            var noise = WindTurbineGenerator.GetNoise(__instance.NoiseIntensity);
            var atmosphere = __instance.GetWorldAtmospherePressure();

            __result = Functions.GetPotentialWindPowerGenerated(atmosphere.ToFloat(), noise);
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(TurbineGenerator), nameof(TurbineGenerator.GetGeneratedPower))]
    [HarmonyPostfix]
    public static void TurbineGeneratorGetGeneratedPower(ref TurbineGenerator __instance, ref float __result) {
        if (Data.EnableTurbine.Value && __instance != null)
            __result *= 10f;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(StirlingEngine), nameof(StirlingEngine.Awake))]
    [HarmonyPostfix]
    public static void StirlingEngineAwake(ref StirlingEngine __instance) {
        if (Data.EnableStirling.Value && __instance != null)
            _ = Traverse.Create(__instance).Field("MaxPower").SetValue(Data.TwentyKilowatts);
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(PowerTransmitterOmni), nameof(PowerTransmitterOmni.GetUsedPower))]
    [HarmonyPostfix]
    public static void PowerTransmitterOmniGetUsedPower(ref PowerTransmitterOmni __instance) {
        if (Data.EnableFasterCharging.Value && __instance != null)
            _ = Traverse.Create(__instance).Field("_maximumPowerUsage").SetValue(Data.TwoAndAHalfKilowatts);
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(AreaPowerControl), nameof(AreaPowerControl.GetUsedPower))]
    [HarmonyPostfix]
    public static void AreaPowerControlGetUsedPower(ref AreaPowerControl __instance) {
        if (Data.EnableFasterCharging.Value && __instance != null)
            __instance.BatteryChargeRate = Data.TwoAndAHalfKilowatts;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(BatteryCellCharger), nameof(BatteryCellCharger.GetUsedPower))]
    [HarmonyPostfix]
    public static void BatteryCellChargerGetUsedPower(ref BatteryCellCharger __instance) {
        if (Data.EnableFasterCharging.Value && __instance != null)
            __instance.BatteryChargeRate = Data.TwoAndAHalfKilowatts;
    }
}