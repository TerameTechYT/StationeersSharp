using Assets.Scripts;
using Assets.Scripts.Localization2;
using Assets.Scripts.Objects;
using Assets.Scripts.Util;
using Objects;
using System.Text;
using UnityEngine;
using Weather;

namespace BetterPowerMod;

internal class Functions {
    public static float GetPotentialSolarPowerGenerated() => OrbitalSimulation.SolarIrradiance;

    public static float GetPotentialWindPowerGenerated(float worldAtmospherePressure, float noise) =>
        Mathf.Max(0, Mathf.Clamp(worldAtmospherePressure, 1f, 100f) * noise) * (WeatherManager.IsWeatherEventRunning ? 15f : 1f);

    public static float GetWindTurbineRPM(WindTurbineGenerator generator) {
        return (GameManager.DeltaTime * generator.GenerationRate) * 60;
    }

    public static PassiveTooltip GetWindTurbineTooltip(WindTurbineGenerator generator) {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"{GameStrings.GeneratingPower} {generator.GenerationRate.ToStringPrefix("W", "yellow")}");
        stringBuilder.AppendLine($"{GetWindTurbineRPM(generator).ToStringPrefix("RPM", "yellow")}");

        var passiveTooltip = new PassiveTooltip();
        passiveTooltip.Title = generator.DisplayName;
        passiveTooltip.Slider = generator.ThingHealth;
        passiveTooltip.SetExtendedText(stringBuilder.ToString());

        return passiveTooltip;
    }
}