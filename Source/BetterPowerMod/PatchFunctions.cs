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
using StationeersLibrary.Exceptions;
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
            __instance.MaxPowerGenerated = OrbitalSimulation.SolarIrradiance;
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }

    [HarmonyPatch(typeof(SolarPanel), nameof(SolarPanel.SolarInfo))]
    [HarmonyPostfix]
    public static void SolarPanelGetSolarPanelInfo(ref SolarPanel __instance, ref string __result) {
        if (!ConfigData.EnableSolarPanel || GameManager.IsBatchMode || __instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            __result += Functions.GetSolarPanelInfo(ref __instance);
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }

    [HarmonyPatch(typeof(Device), nameof(Device.GetPassiveTooltip))]
    [HarmonyPostfix]
    public static void DeviceGetPassiveTooltipPrefix(ref Device __instance, ref PassiveTooltip __result, ref Collider hitCollider) {
        if (!ConfigData.EnableWindTurbine || __instance is not WindTurbineGenerator windTurbineGenerator || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            float turbineRotationSpeed = Traverse.Create(windTurbineGenerator).Field("_turbineRotationSpeed").GetValue<float>();

            __result = new() {
                Title = windTurbineGenerator.DisplayName,
                State = Functions.GetWindTurbineInfo(ref windTurbineGenerator, turbineRotationSpeed),
                Slider = windTurbineGenerator.ThingHealth,
            };
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }

    }

    [HarmonyPatch(typeof(WindTurbineGenerator), nameof(WindTurbineGenerator.MAXPowerOutput), MethodType.Getter)]
    [HarmonyPostfix]
    public static void WindTurbineGeneratorMAXPowerOutputGetter(ref WindTurbineGenerator __instance, ref float __result) {
        if (!ConfigData.EnableWindTurbine || __instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            PressurekPa pressure = __instance.GetWorldAtmospherePressure();
            __result += pressure.ToFloat();
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }

    [HarmonyPatch(typeof(WindTurbineGenerator), nameof(WindTurbineGenerator.MaxPowerOutputStorm), MethodType.Getter)]
    [HarmonyPostfix]
    public static void WindTurbineGeneratorMaxPowerOutputStormGetter(ref WindTurbineGenerator __instance, ref float __result) {
        if (!ConfigData.EnableWindTurbine || __instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            PressurekPa pressure = __instance.GetWorldAtmospherePressure();
            __result += pressure.ToFloat();
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }

    /*[HarmonyPatch(typeof(TurbineGenerator), nameof(TurbineGenerator.GetGeneratedPower))]
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
    }*/

    [HarmonyPatch(typeof(StirlingEngine), nameof(StirlingEngine.GetGeneratedPower))]
    [HarmonyPrefix]
    public static void StirlingEngineMaxPowerGetter(ref StirlingEngine __instance, ref float ___maxPower) {
        if (!ConfigData.EnableStirling || __instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            ___maxPower = ConfigData.StirlingEnergy;
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