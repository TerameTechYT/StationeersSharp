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
    public static void SolarPanelPowerGeneratedGetter(ref SolarPanel Instance, ref float Result)
    {
        if (Instance != null)
        {
            Result = Functions.GetPotentialSolarPowerGenerated(Instance.PowerCable);
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(TurbineGenerator), nameof(TurbineGenerator.GetGeneratedPower))]
    [HarmonyPostfix]
    public static void TurbineGeneratorGetGeneratedPower(ref TurbineGenerator Instance, ref float Result)
    {
        Result *= 10f;
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(StirlingEngine), nameof(StirlingEngine.Awake))]
    [HarmonyPostfix]
    public static void StirlingEngineOnPowerTick(ref StirlingEngine Instance)
    {
        if (Instance != null)
        {
            _ = Traverse.Create(Instance).Field("MaxPower").SetValue(Data.TwentyKilowatts);
        }
    }


    [UsedImplicitly]
    [HarmonyPatch(typeof(PowerTransmitterOmni), nameof(PowerTransmitterOmni.GetUsedPower))]
    [HarmonyPostfix]
    public static void PowerTransmitterOmniOnPowerTick(ref PowerTransmitterOmni Instance)
    {
        if (Instance != null)
        {
            float availablePower = Functions.GetPowerAvailable(Instance.PowerCable);

            _ = Traverse.Create(Instance).Field("_maximumPowerUsage").SetValue(availablePower);
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(AreaPowerControl), nameof(AreaPowerControl.GetUsedPower))]
    [HarmonyPostfix]
    public static void AreaPowerControlOnPowerTick(ref AreaPowerControl Instance)
    {
        if (Instance != null)
        {
            Instance.BatteryChargeRate = Functions.GetPowerAvailable(Instance.PowerCable);
        }
    }

    [UsedImplicitly]
    [HarmonyPatch(typeof(BatteryCellCharger), nameof(BatteryCellCharger.GetUsedPower))]
    [HarmonyPostfix]
    public static void BatteryCellChargerOnPowerTick(ref BatteryCellCharger Instance)
    {
        if (Instance != null)
        {
            Instance.BatteryChargeRate = Functions.GetPowerAvailable(Instance.PowerCable);
        }
    }
}