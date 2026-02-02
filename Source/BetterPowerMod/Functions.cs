#region

using Assets.Scripts.Localization2;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Util;
using Objects;
using StationeersLibrary;
using System.Text;
using UnityEngine;

#endregion

namespace BetterPowerMod;

internal static class Functions {
    internal static readonly GameString VerticalDegrees = GameString.Create("DegreesVertical", "Vertical <color=yellow>{0} degrees</color>");
    internal static readonly GameString HorizontalDegrees = GameString.Create("DegreesHorizontal", "Horizontal <color=yellow>{0} degrees</color>");
    internal static readonly GameString GeneratingPower = GameString.Create("PowerGenerating", "Generating <color=yellow>{0}W</color>");
    internal static readonly GameString GlobalSpeed = GameString.Create("SpeedRpm", "Speed <color=yellow>{0}RPM</color>");

    internal static string GetWindTurbineInfo(ref WindTurbineGenerator generator, float turbineRotationSpeed) {
        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine($"{GeneratingPower.AsString(generator.CalculateGenerationRate().ToStringRounded())}");
        stringBuilder.AppendLine($"{GlobalSpeed.AsString(turbineRotationSpeed.ToStringRounded())}");

        return stringBuilder.ToString();
    }

    internal static string GetSolarPanelInfo(ref SolarPanel solarPanel) {
        StringBuilder stringBuilder = new();
        if (!ConfigData.FlatSolarPanelPrefabs.Contains(solarPanel.PrefabName)) {
            double vertical = Mathf.Lerp((float) solarPanel.MinimumVertical, (float) solarPanel.MaximumVertical, (float) solarPanel.Vertical);
            double horizontal = solarPanel.Horizontal * solarPanel.MaximumHorizontal;

            stringBuilder.AppendLine("\n");
            stringBuilder.AppendLine($"{VerticalDegrees.AsString($"{vertical:F2}")}");
            stringBuilder.AppendLine($"{HorizontalDegrees.AsString($"{horizontal:F2}")}");
        }

        return stringBuilder.ToString();
    }
}