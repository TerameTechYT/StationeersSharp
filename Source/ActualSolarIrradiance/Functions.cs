using Assets.Scripts.Objects.Electrical;
using UnityEngine;

namespace ActualSolarIrradiance;

internal class Functions
{
    public static float GetPotentialSolarPowerGenerated(Cable cable)
    {
        return Mathf.Min(cable?.MaxVoltage ?? Data.OneKilowatt, OrbitalSimulation.SolarIrradiance);
    }

    public static float GetPotentialWindPowerGenerated(Cable cable, float worldAtmospherePressure, float noise)
    {

        return Mathf.Min(0f, worldAtmospherePressure * noise);
    }

    public static float GetPowerAvailable(Cable cable)
    {
        return Mathf.Max(cable?.MaxVoltage ?? Data.OneKilowatt, cable?.CableNetwork.EstimatedRemainingLoad ?? 0);
    }
}