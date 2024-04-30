using UnityEngine;

namespace ActualSolarIrradiance;

internal class Functions
{
    public static float GetPotentialPowerGenerated(float MaxVoltage)
    {
        return Mathf.Min(MaxVoltage, OrbitalSimulation.SolarIrradiance);
    }
}