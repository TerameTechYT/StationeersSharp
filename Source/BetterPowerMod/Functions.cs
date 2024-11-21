#region

using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Localization2;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Util;
using Objects;
using System;
using System.Text;
using UnityEngine;
using Weather;

#endregion

namespace BetterPowerMod;

internal static class Functions {
    public static float GetPotentialSolarPowerGenerated() => OrbitalSimulation.SolarIrradiance;

    public static float GetPotentialWindPowerGenerated(PressurekPa worldAtmospherePressure, float noise) {
        float value = Mathf.Max(0, Mathf.Clamp(worldAtmospherePressure.ToFloat(), 20f, 100f) * noise);

        return WeatherManager.IsWeatherEventRunning && WeatherManager.CurrentWeatherEvent != null ? WeatherManager.CurrentWeatherEvent.WindStrength * value : value;
    }

    public static float GetWindTurbineRPM(WindTurbineGenerator generator) => GameManager.DeltaTime * generator.GenerationRate * 60;

    public static PassiveTooltip GetWindTurbineTooltip(WindTurbineGenerator generator) {
        StringBuilder stringBuilder = new();

        _ = stringBuilder.AppendLine(
            $"{GameStrings.GeneratingPower} {generator.GenerationRate.ToStringPrefix("W", "yellow")}");

        _ = stringBuilder.AppendLine($"{GetWindTurbineRPM(generator).ToStringPrefix("RPM", "yellow")}");

        return new PassiveTooltip() {
            Title = generator.DisplayName,
            Slider = generator.ThingHealth,
            Extended = stringBuilder.ToString()
        };
    }

    internal static string GetExtraSolarPanelTooltip(SolarPanel panel, string text) {
        StringBuilder stringBuilder = new();
        stringBuilder.Append($"Vertical: {(panel.Vertical * panel.MaximumVertical).ToStringPrefix("Deg", "yellow", true)}");
        stringBuilder.AppendLine();
        stringBuilder.Append($"Horizontal: {(panel.Horizontal * panel.MaximumHorizontal).ToStringPrefix("Deg", "yellow", true)}");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine(text);

        return stringBuilder.ToString();
    }
}