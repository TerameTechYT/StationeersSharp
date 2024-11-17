#region

using Assets.Scripts;
using Assets.Scripts.Localization2;
using Assets.Scripts.Objects;
using Assets.Scripts.Util;
using Objects;
using System.Text;
using UnityEngine;
using Weather;

#endregion

namespace BetterPowerMod;

internal static class Functions {
    public static float GetPotentialSolarPowerGenerated() => OrbitalSimulation.SolarIrradiance;

    public static float GetPotentialWindPowerGenerated(float worldAtmospherePressure, float noise) {
        float value = Mathf.Max(0, Mathf.Clamp(worldAtmospherePressure, 20f, 100f) * noise);

        return WeatherManager.IsWeatherEventRunning ? 2000 + value : value;
    }

    public static float GetWindTurbineRPM(WindTurbineGenerator generator) => GameManager.DeltaTime * generator.GenerationRate * 60;

    public static PassiveTooltip GetWindTurbineTooltip(WindTurbineGenerator generator) {
        StringBuilder stringBuilder = new();

        _ = stringBuilder.AppendLine(
            $"{GameStrings.GeneratingPower} {generator.GenerationRate.ToStringPrefix("W", "yellow")}");

        _ = stringBuilder.AppendLine($"{GetWindTurbineRPM(generator).ToStringPrefix("RPM", "yellow")}");

        PassiveTooltip passiveTooltip = new() {
            Title = generator.DisplayName,
            Slider = generator.ThingHealth
        };
        passiveTooltip.SetExtendedText(stringBuilder.ToString());

        return passiveTooltip;
    }
}