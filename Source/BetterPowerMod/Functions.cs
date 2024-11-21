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

    public static float GetPotentialWindPowerGenerated(PressurekPa worldAtmosphere, float noise) {
        float pressure = worldAtmosphere.ToFloat();
        if (pressure < 1f) {
            return 0f;
        }

        float value = Mathf.Max(0, Mathf.Clamp(pressure, 1f, 100f) * noise);

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

    internal static string GetSolarPanelTooltip(SolarPanel panel, string text) {
        if (!Data.IgnoredPrefabs.Contains(panel.PrefabName)) {
            float vertical = Mathf.Lerp((float) panel.MinimumVertical, (float) panel.MaximumVertical, (float) panel.Vertical);
            float horizontal = (float) panel.Horizontal * (float) panel.MaximumHorizontal;

            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine($"Vertical {vertical.ToStringPrefix("Deg", "yellow")}");
            stringBuilder.AppendLine($"Horizontal {horizontal.ToStringPrefix("Deg", "yellow")}");
            stringBuilder.Append(text);
            return stringBuilder.ToString();
        }
        else {
            return text;
        }
    }
}