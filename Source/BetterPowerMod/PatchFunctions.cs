#region

using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.Objects.Structures;
using HarmonyLib;
using Objects;
using StationeersLibrary;
using StationeersLibrary.Modding;
using UnityEngine;

#endregion

namespace BetterPowerMod;

[HarmonyPatch]
public static class PatchFunctions {
    [HarmonyPatch(typeof(SolarPanel), nameof(SolarPanel.PowerGenerated))]
    [HarmonyPatchConfig<Plugin>("Configurables", "Solar Panel Patches")]
    [HarmonyPrefix]
    public static void SolarPowerGenerated(ref SolarPanel __instance) {
        if (__instance == null) {
            return;
        }

        try {
            __instance.MaxPowerGenerated = OrbitalSimulation.SolarIrradiance;
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }

    [HarmonyPatch(typeof(SolarPanel), nameof(SolarPanel.SolarInfo))]
    [HarmonyPatchConfig<Plugin>("Configurables", "Solar Panel Patches")]
    [HarmonyPostfix]
    public static void SolarPanelGetSolarPanelInfo(ref SolarPanel __instance, ref string __result) {
        if (GameManager.IsBatchMode || __instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            __result += Functions.GetSolarPanelInfo(ref __instance);
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }

    [HarmonyPatch(typeof(Device), nameof(Device.GetPassiveTooltip))]
    [HarmonyPatchConfig<Plugin>("Configurables", "Wind Turbine Patches")]
    [HarmonyPostfix]
    public static void DeviceGetPassiveTooltipPrefix(ref Device __instance, ref PassiveTooltip __result, ref Collider hitCollider) {
        if (__instance is not WindTurbineGenerator windTurbineGenerator || !__instance.IsStructureCompleted) {
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
    [HarmonyPatchConfig<Plugin>("Configurables", "Wind Turbine Patches")]
    [HarmonyPostfix]
    public static void WindTurbineGeneratorMAXPowerOutputGetter(ref WindTurbineGenerator __instance, ref float __result) {
        if (__instance == null || !__instance.IsStructureCompleted) {
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
    [HarmonyPatchConfig<Plugin>("Configurables", "Wind Turbine Patches")]
    [HarmonyPostfix]
    public static void WindTurbineGeneratorMaxPowerOutputStormGetter(ref WindTurbineGenerator __instance, ref float __result) {
        if (__instance == null || !__instance.IsStructureCompleted) {
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
    [HarmonyPatchConfig<Plugin>("Configurables", "Wall Turbine Patches")]
    [HarmonyPostfix]
    public static void TurbineGeneratorGetGeneratedPower(ref TurbineGenerator __instance, ref float __result, CableNetwork cableNetwork) {
        if (__instance == null || !__instance.IsStructureCompleted || __instance.PowerCableNetwork != cableNetwork) {
            return;
        }

        try {
            __result *= ConfigData.TurbineMultiplier;
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }*/

    [HarmonyPatch(typeof(StirlingEngine), nameof(StirlingEngine.GetGeneratedPower))]
    [HarmonyPatchConfig<Plugin>("Configurables", "Stirling Patches")]
    [HarmonyPrefix]
    public static void StirlingEngineMaxPowerGetter(ref StirlingEngine __instance, ref float ___maxPower) {
        if (__instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            ___maxPower = ConfigData.StirlingEnergy;
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }

    [HarmonyPatch(typeof(PowerTransmitterOmni), nameof(PowerTransmitterOmni.GetUsedPower))]
    [HarmonyPatchConfig<Plugin>("Configurables", "Charging Patches")]
    [HarmonyPostfix]
    public static void PowerTransmitterOmniGetUsedPower(ref PowerTransmitterOmni __instance, ref float ____maximumPowerUsage) {
        if (__instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            ____maximumPowerUsage = ConfigData.FastChargeRate;
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }

    [HarmonyPatch(typeof(AreaPowerControl), nameof(AreaPowerControl.GetUsedPower))]
    [HarmonyPatchConfig<Plugin>("Configurables", "Charging Patches")]
    [HarmonyPostfix]
    public static void AreaPowerControlGetUsedPower(ref AreaPowerControl __instance) {
        if (__instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            __instance.BatteryChargeRate = ConfigData.FastChargeRate;
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }

    [HarmonyPatch(typeof(BatteryCellCharger), nameof(BatteryCellCharger.GetUsedPower))]
    [HarmonyPatchConfig<Plugin>("Configurables", "Charging Patches")]
    [HarmonyPostfix]
    public static void BatteryCellChargerGetUsedPower(ref BatteryCellCharger __instance) {
        if (__instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            __instance.BatteryChargeRate = __instance.PrefabName == ConfigData.BatteryChargerSmall ? ConfigData.FastChargeRate / 2f : ConfigData.FastChargeRate;
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }

    [HarmonyPatch(typeof(WallLightBattery), nameof(WallLightBattery.GetUsedPower))]
    [HarmonyPatchConfig<Plugin>("Configurables", "Charging Patches")]
    [HarmonyPostfix]
    public static void WallLightBatteryGetUsedPower(ref WallLightBattery __instance) {
        if (__instance == null || !__instance.IsStructureCompleted) {
            return;
        }

        try {
            __instance.BatteryChargeRate = ConfigData.FastChargeRate / 2f;
        } catch (Exception ex) {
            Utilities.ExceptionReporter(Plugin.Instance, ref ex);
        }
    }
}