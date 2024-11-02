#region

using System;
using System.Collections.Generic;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Objects.Clothing;
using Assets.Scripts.Objects.Motherboards;

#endregion

namespace ExternalSuitReader;

public static class Functions {
    private static readonly Dictionary<LogicType, Chemistry.GasType> LogicPairs = new() {
        { LogicType.RatioOxygenOutput, Chemistry.GasType.Oxygen },
        { LogicType.RatioLiquidOxygenOutput, Chemistry.GasType.LiquidOxygen },
        { LogicType.RatioNitrogenOutput, Chemistry.GasType.Nitrogen },
        { LogicType.RatioLiquidNitrogenOutput, Chemistry.GasType.LiquidNitrogen },
        { LogicType.RatioCarbonDioxideOutput, Chemistry.GasType.CarbonDioxide },
        { LogicType.RatioLiquidCarbonDioxideOutput, Chemistry.GasType.LiquidCarbonDioxide },
        { LogicType.RatioVolatilesOutput, Chemistry.GasType.Volatiles },
        { LogicType.RatioLiquidVolatilesOutput, Chemistry.GasType.LiquidVolatiles },
        { LogicType.RatioPollutantOutput, Chemistry.GasType.Pollutant },
        { LogicType.RatioLiquidPollutantOutput, Chemistry.GasType.LiquidPollutant },
        { LogicType.RatioNitrousOxideOutput, Chemistry.GasType.NitrousOxide },
        { LogicType.RatioLiquidNitrousOxideOutput, Chemistry.GasType.LiquidNitrousOxide },
        { LogicType.RatioSteamOutput, Chemistry.GasType.Steam },
        { LogicType.RatioWaterOutput, Chemistry.GasType.Water },
        { LogicType.RatioWaterOutput2, Chemistry.GasType.PollutedWater }
    };

    public static bool CanLogicRead(LogicType logicType) {
        if (LogicPairs.TryGetValue(logicType, out _) || logicType == LogicType.TotalMolesOutput) return true;

        return false;
    }

    public static double GetLogicValue(AdvancedSuit suit, LogicType logicType) {
        if (suit != null && suit.HasAtmosphere && suit.HasReadableAtmosphere) {
            if (LogicPairs.TryGetValue(logicType, out var gasType))
                return Convert.ToDouble(suit.WorldAtmosphere.GetGasTypeRatio(gasType));
            if (logicType == LogicType.TotalMolesOutput) return Convert.ToDouble(suit.WorldAtmosphere.TotalMoles);
        }

        return 0.0;
    }
}