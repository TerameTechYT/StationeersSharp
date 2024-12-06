#region

using Assets.Scripts;
using Assets.Scripts.Localization2;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Util;
using Objects;
using System.Text;
using UnityEngine;
using Weather;

#endregion

namespace BetterPowerMod;

internal static class Functions {
    internal static float GetPotentialSolarPowerGenerated(SolarPanel panel) => OrbitalSimulation.SolarIrradiance;

    internal static float GetPotentialWindPowerGenerated(WindTurbineGenerator generator) {
        float pressure = generator.GetWorldAtmospherePressure().ToFloat();
        if (pressure < 1f) {
            return 0f;
        }

        float noise = WindTurbineGenerator.GetNoise(generator.NoiseIntensity);
        float value = Mathf.Max(0, Mathf.Clamp(pressure, 1f, 100f) * noise);

        return WeatherManager.IsWeatherEventRunning && WeatherManager.CurrentWeatherEvent != null ? WeatherManager.CurrentWeatherEvent.WindStrength * value : value;
    }

    internal static float GetWindTurbineRPM(WindTurbineGenerator generator) => GameManager.DeltaTime * generator.GenerationRate * 60;

    internal static PassiveTooltip GetWindTurbineTooltip(WindTurbineGenerator generator) {
        StringBuilder stringBuilder = new();

        _ = stringBuilder.AppendLine(
            $"{GameStrings.GeneratingPower} {generator.GenerationRate.ToStringPrefix("W", "yellow")}");

        _ = stringBuilder.AppendLine($"Speed {GetWindTurbineRPM(generator).ToStringPrefix("RPM", "yellow")}");

        return new PassiveTooltip() {
            Title = generator.DisplayName,
            Slider = generator.ThingHealth,
            Extended = stringBuilder.ToString()
        };
    }

    internal static string GetSolarPanelTooltip(SolarPanel panel, string text) {
        if (!Data.IgnoredPrefabs.Contains(panel.PrefabName)) {
            double vertical = panel.Vertical * panel.MaximumVertical;
            double horizontal = panel.Horizontal * panel.MaximumHorizontal;

            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine($"Vertical {vertical.ToStringPrefix("Deg", "yellow")}");
            stringBuilder.AppendLine($"Horizontal {horizontal.ToStringPrefix("Deg", "yellow")}");
            stringBuilder.Append(text);
            return stringBuilder.ToString();
        }
        return text;
    }
}