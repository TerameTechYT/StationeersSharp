#region

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
        float value = Mathf.Max(0, Mathf.Clamp(pressure, 1f, Constants.ONE_ATMOSPHERE_PRESSURE_KPA) * noise);

        return WeatherManager.IsWeatherEventRunning && WeatherManager.CurrentWeatherEvent != null ? WeatherManager.CurrentWeatherEvent.WindStrength * value : value;
    }

    internal static float GetWindTurbineRPM(WindTurbineGenerator generator) => GameManager.DeltaTime * generator.GenerationRate * 60;

    internal static PassiveTooltip GetWindTurbineTooltip(WindTurbineGenerator generator) {
        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine($"{GameStrings.GeneratingPower} {generator.GenerationRate.ToStringPrefix("W", "yellow")}");
        stringBuilder.AppendLine($"Speed {GetWindTurbineRPM(generator).ToStringPrefix("RPM", "yellow")}");

        return new PassiveTooltip() {
            Title = generator.DisplayName,
            Slider = generator.ThingHealth,
            Extended = stringBuilder.ToString()
        };
    }

    internal static string GetSolarPanelTooltip(SolarPanel panel, string text) {
        if (!Data.IgnoredSolarPanelPrefabs.Contains(panel.PrefabName)) {
            double vertical = panel.Vertical * panel.MaximumVertical;
            double horizontal = panel.Horizontal * panel.MaximumHorizontal;

            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine($"Vertical {vertical.ToStringPrefix(Constants.DEGREE_SYMBOL, "yellow")}");
            stringBuilder.AppendLine($"Horizontal {horizontal.ToStringPrefix(Constants.DEGREE_SYMBOL, "yellow")}");
            stringBuilder.Append(text);
            return stringBuilder.ToString();
        }
        return text;
    }
}