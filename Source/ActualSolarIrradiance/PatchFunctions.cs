#pragma warning disable CA1707

using Assets.Scripts.Objects.Electrical;
using HarmonyLib;
using UnityEngine;

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
            __result = Mathf.Min(__instance.PowerCable.MaxVoltage, Functions.GetPotentialPowerGenerated());
        }
    }
}