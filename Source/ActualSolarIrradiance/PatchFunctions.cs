using Assets.Scripts.Objects.Electrical;
using HarmonyLib;
using JetBrains.Annotations;

namespace ActualSolarIrradiance;

[HarmonyPatch]
public static class PatchFunctions
{
    [UsedImplicitly]
    [HarmonyPatch(typeof(SolarPanel), nameof(SolarPanel.PowerGenerated), MethodType.Getter)]
    [HarmonyPostfix]
    public static void SolarPanelPowerGeneratedGetter(ref SolarPanel __instance, ref float __result)
    {
        if (__instance != null)
        {
            __result = Functions.GetPotentialSolarPowerGenerated(__instance.PowerCable);
        }
    }

    /*[UsedImplicitly]
    [HarmonyPatch(typeof(TurbineGenerator), nameof(TurbineGenerator.GetGeneratedPower))]
    [HarmonyPostfix]
    public static void TurbineGeneratorGetGeneratedPower(ref TurbineGenerator __instance, ref float __result)
    {
        __result *= 10f;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(StirlingEngine), nameof(StirlingEngine.Awake))]
    [HarmonyPostfix]
    public static void StirlingEngineOnPowerTick(ref StirlingEngine __instance)
    {
        if (__instance != null)
        {
            _ = Traverse.Create(__instance).Field("MaxPower").SetValue(Data.TwentyKilowatts);
        }
    }


    [UsedImplicitly]
    [HarmonyPatch(typeof(PowerTransmitterOmni), nameof(PowerTransmitterOmni.GetUsedPower))]
    [HarmonyPostfix]
    public static void PowerTransmitterOmniOnPowerTick(ref PowerTransmitterOmni __instance)
    {
        if (__instance != null)
        {
            float availablePower = Functions.GetPowerAvailable(__instance.PowerCable);

            _ = Traverse.Create(__instance).Field("_maximumPowerUsage").SetValue(availablePower);
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(AreaPowerControl), nameof(AreaPowerControl.GetUsedPower))]
    [HarmonyPostfix]
    public static void AreaPowerControlOnPowerTick(ref AreaPowerControl __instance)
    {
        if (__instance != null)
        {
            __instance.BatteryChargeRate = Functions.GetPowerAvailable(__instance.PowerCable);
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(BatteryCellCharger), nameof(BatteryCellCharger.GetUsedPower))]
    [HarmonyPostfix]
    public static void BatteryCellChargerOnPowerTick(ref BatteryCellCharger __instance)
    {
        if (__instance != null)
        {
            __instance.BatteryChargeRate = Functions.GetPowerAvailable(__instance.PowerCable);
        }
    }*/
}