// ReSharper disable InconsistentNaming

using System;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Objects.Clothing;
using Assets.Scripts.Objects.Motherboards;

namespace ExternalSuitReader;

internal class Functions
{
    internal static bool CanLogicRead(LogicType logicType)
    {
        return GetGasTypeFromLogicType(logicType, "Output") != Chemistry.GasType.Undefined;
    }

    internal static double GetLogicValue(AdvancedSuit suit, Chemistry.GasType gasType)
    {
        return suit != null && suit.HasAtmosphere && suit.HasReadableAtmosphere
            ? Convert.ToDouble(suit.WorldAtmosphere.GetGasTypeRatio(gasType))
            : 0.0;
    }

    internal static Chemistry.GasType GetGasTypeFromLogicType(LogicType logicType, string endingString)
    {
        var logicTypeAsString = Enum.GetName(typeof(LogicType), logicType) ?? "";
        var gasTypeString = logicTypeAsString.Replace("Ratio", "").Replace(endingString, "");
        return Enum.TryParse(gasTypeString, out Chemistry.GasType gasType) ? gasType : Chemistry.GasType.Undefined;
    }
}