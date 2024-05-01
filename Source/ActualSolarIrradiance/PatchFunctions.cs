#pragma warning disable CA1707

using Assets.Scripts.Objects.Electrical;
using HarmonyLib;

namespace ActualSolarIrradiance;

[HarmonyPatch]
public static class PatchFunctions
{

    [HarmonyPatch(typeof(SolarPanel), "PowerGenerated", MethodType.Getter)]
    [HarmonyPostfix]
    public static void SolarPanelPowerGeneratedGetter(ref SolarPanel __instance, ref float __result)
    {
        if (__instance != null)
        {
            __result = Functions.GetPotentialPowerGenerated(__instance.PowerCable?.MaxVoltage ?? Data.FiveKilowatts);
        }
    }
}