#region

using Assets.Scripts.Localization2;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Util;
using Objects;
using StationeersLibrary;
using System.Text;

#endregion

namespace BetterPowerMod;

internal static class Functions {
    internal static float GetPotentialSolarPowerGenerated(ref SolarPanel panel) {
        float panelArea = panel.PanelSize.x * panel.PanelSize.y;

        return OrbitalSimulation.SolarIrradiance * panelArea;
    }

    internal static string GetWindTurbineInfo(ref WindTurbineGenerator generator, ref float turbineRotationSpeed) {
        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine($"{GameStrings.GeneratingPower} {generator.GenerationRate.ToStringPrefix("W", "yellow")}");
        stringBuilder.AppendLine($"Speed {turbineRotationSpeed.ToStringPrefix("RPM", "yellow")}");

        return stringBuilder.ToString();
    }

    internal static string GetSolarPanelInfo(ref SolarPanel solarPanel) {
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
    }
}