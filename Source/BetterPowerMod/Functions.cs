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

        float intensity = Functions.IsCurrentlyStorm() ? Functions.GetWeatherWindStrength() * generator.WeatherUtilisationMultiplier : 1f;
        float noise = WindTurbineGenerator.GetNoise(generator.NoiseIntensity * intensity);
        float value = Mathf.Max(0, Mathf.Clamp(pressure, 1f, Constants.ONE_ATMOSPHERE_PRESSURE_KPA) * noise);

        return Functions.IsCurrentlyStorm() ? Functions.GetWeatherWindStrength() * value : value;
    }

    internal static bool IsCurrentlyStorm() => WeatherManager.IsWeatherEventRunning && WeatherManager.CurrentWeatherEvent != null;

    internal static WeatherEvent? GetWeatherEvent() => Functions.IsCurrentlyStorm() ? WeatherManager.CurrentWeatherEvent : null;
    internal static float GetWeatherWindStrength() => Functions.GetWeatherEvent()?.WindStrength ?? 1f;

    internal static float GetWindTurbineRPM(WindTurbineGenerator generator) => 720f * GameManager.DeltaTime * generator.GenerationRate;

    /*internal static PassiveTooltip GetWindTurbineTooltip(WindTurbineGenerator generator, Collider hitCollider) {
            PassiveTooltip passiveTooltip = PatchFunctions.DeviceGetPassiveTooltipReversePatch(generator, hitCollider);

            if (generator.IsStructureCompleted) {
                    passiveTooltip.Title = generator.DisplayName;
                    passiveTooltip.State = Functions.GetWindTurbineInfo(generator);
            }

            return passiveTooltip;
    }

    internal static string GetWindTurbineInfo(WindTurbineGenerator generator) {
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine($"{GameStrings.GeneratingPower} {generator.GenerationRate.ToStringPrefix("W", "yellow")}");
            stringBuilder.AppendLine($"Speed {Functions.GetWindTurbineRPM(generator).ToStringPrefix("RPM", "yellow")}");

            return stringBuilder.ToString();
    }

    internal static PassiveTooltip GetSolarPanelTooltip(SolarPanel solarPanel, Collider hitCollider) {
            PassiveTooltip passiveTooltip = PatchFunctions.DeviceGetPassiveTooltipReversePatch(solarPanel, hitCollider);

            if (solarPanel.IsStructureCompleted) {
                    passiveTooltip.Title = solarPanel.DisplayName;
                    passiveTooltip.State = Functions.GetSolarPanelInfo(solarPanel);
            }

            if (solarPanel.DamageState.Total > 0f) {
                    passiveTooltip.RepairString = ISolarRepairer.Tooltip;
            }

            return passiveTooltip;
    }

    internal static string GetSolarPanelInfo(SolarPanel solarPanel) {
            StringBuilder stringBuilder = new();
            float efficency = (solarPanel.GenerationEfficiency * (1f - solarPanel.DamageState.TotalRatio) * 100f).RoundToSignificantDigits(2);
            float health = (100f - (solarPanel.DamageState.TotalRatio * 100f)).RoundToSignificantDigits(2);

            stringBuilder.AppendLine($"{GameStrings.GeneratingPower} {solarPanel.GenerationRate.ToStringPrefix("W", "yellow")}");
            stringBuilder.AppendLine($"Efficency {efficency.ToStringPercent("yellow")}");
            stringBuilder.AppendLine($"Health {health.ToStringPercent(solarPanel.DamageColor)}");

            if (!ConfigData.FlatSolarPanelPrefabs.Contains(solarPanel.PrefabName)) {
                    double vertical = solarPanel.Vertical * solarPanel.MaximumVertical;
                    double horizontal = solarPanel.Horizontal * solarPanel.MaximumHorizontal;

                    stringBuilder.AppendLine($"Vertical {vertical.ToStringPrefix("degrees", "yellow")}");
                    stringBuilder.AppendLine($"Horizontal {horizontal.ToStringPrefix("degrees", "yellow")}");
            }

            return stringBuilder.ToString();
    }*/
}