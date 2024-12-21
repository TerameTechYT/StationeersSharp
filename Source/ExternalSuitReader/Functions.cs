#region

using Assets.Scripts.Atmospherics;
using Assets.Scripts.Objects.Clothing;
using Assets.Scripts.Objects.Motherboards;
using System;
using System.Collections.Generic;

#endregion

namespace ExternalSuitReader;

internal static class Functions {
    internal static bool CanLogicRead(LogicType logicType) {
        return Data.LogicPairs.TryGetValue(logicType, out _) || logicType == LogicType.TotalMolesOutput;
    }

    internal static double GetLogicValue(AdvancedSuit suit, LogicType logicType) {
        if (suit.HasAtmosphere && suit.HasReadableAtmosphere) {
            if (Data.LogicPairs.TryGetValue(logicType, out Chemistry.GasType gasType))
                return Convert.ToDouble(suit.WorldAtmosphere.GetGasTypeRatio(gasType));

            if (logicType == LogicType.TotalMolesOutput)
                return Convert.ToDouble(suit.WorldAtmosphere.TotalMoles);
        }

        return 0.0;
    }
}