#region

using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Objects.Clothing;
using Assets.Scripts.Objects.Motherboards;
using StationeersLibrary;
using StationeersLibrary.Modding;
using UnityEngine;

#endregion

namespace ExternalSuitReader;

public class Plugin : Mod<Plugin> {
    public static Plugin? Instance { get; private set; }

    public override bool UseLogger => true;
    public override bool UseConfig => true;
    public override bool UseHarmony => true;

    public override ModInfo Data => new ModInfo() {
        Name = "ExternalSuitReader",
        Guid = "externalsuitreader",
        Version = new Version(1, 8, 0, 522),
        WorkshopId = 3071985478ul,
        GameType = GameType.Both,
    };

    public Plugin() : base() => Plugin.Instance = this;

    public override void OnStart() { }
}

internal struct Data {
    // Config Data
    //public static ConfigEntry<bool> enableExperimentalSaving;
    //public static bool EnableExperimentalSaving => enableExperimentalSaving?.DefaultValue ?? false;

    /*
     * Base Allowed Logic Types:
     * 
     * LogicType.PressureExternal:
     * LogicType.Setting:
     * LogicType.Volume:
     * LogicType.PressureSetting:
     * LogicType.TemperatureSetting:
     * LogicType.TemperatureExternal:
     * LogicType.Filtration:
     * LogicType.AirRelease:
     * LogicType.PositionX:
     * LogicType.PositionY:
     * LogicType.PositionZ:
     * LogicType.VelocityMagnitude:
     * LogicType.VelocityRelativeX:
     * LogicType.VelocityRelativeY:
     * LogicType.VelocityRelativeZ:
     * LogicType.SoundAlert:
     * LogicType.ForwardX:
     * LogicType.ForwardY:
     * LogicType.ForwardZ:
     * LogicType.Orientation:
     * LogicType.VelocityX:
     * LogicType.VelocityY:
     * LogicType.VelocityZ:
     * LogicType.EntityState:
     */
    public static Dictionary<long, List<DoubleReference>> AllAdvancedSuits = [];
    public static int ChannelCount => 8;

    public static readonly Dictionary<LogicType, Func<AdvancedSuit, double>> LogicReadDictionary = new() {
                // oxygen
                {LogicType.RatioOxygenOutput, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.Oxygen)},
                {LogicType.RatioLiquidOxygenOutput, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.LiquidOxygen)},

                // nitrogen
                {LogicType.RatioNitrogenOutput, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.Nitrogen)},
                {LogicType.RatioLiquidNitrogenOutput, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.LiquidNitrogen)},

                // carbon dioxide
                {LogicType.RatioCarbonDioxideOutput, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.CarbonDioxide)},
                {LogicType.RatioLiquidCarbonDioxideOutput, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.LiquidCarbonDioxide)},

                // methane
                {LogicType.RatioMethaneOutput, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.Methane)},
                {LogicType.RatioLiquidMethaneOutput, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.LiquidMethane)},

                // pollutant
                {LogicType.RatioPollutantOutput, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.Pollutant)},
                {LogicType.RatioLiquidPollutantOutput, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.LiquidPollutant)},

                // nitrous oxide
                {LogicType.RatioNitrousOxideOutput, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.NitrousOxide)},
                {LogicType.RatioLiquidNitrousOxideOutput, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.LiquidNitrousOxide)},

                // steam, water & polluted water
                {LogicType.RatioSteam, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.Steam)},
                {LogicType.RatioWaterOutput, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.Water)},
                {LogicType.RatioWaterOutput2, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.PollutedWater)},

                // hydrogen
                {LogicType.RatioHydrogen, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.Hydrogen)},
                {LogicType.RatioLiquidHydrogen, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.LiquidHydrogen)},

                //hydrazine
                {LogicType.RatioHydrazine, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.Hydrazine)},
                {LogicType.RatioLiquidHydrazine, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.LiquidHydrazine)},

                // silanol
                {LogicType.RatioSilanol, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.Silanol)},
                {LogicType.RatioLiquidSilanol, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.LiquidSilanol)},


                // acids
                {LogicType.RatioLiquidAlcohol, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.LiquidAlcohol)},
                {LogicType.RatioHydrochloricAcid, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.HydrochloricAcid)},
                {LogicType.RatioLiquidHydrochloricAcid, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.LiquidHydrochloricAcid)},

                // ozone
                {LogicType.RatioOzone, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.Ozone)},
                {LogicType.RatioLiquidOzone, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.LiquidOzone)},

                {LogicType.RatioHelium, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.Helium)},

                // data channels
                /*{LogicType.Channel0, (suit) => Functions.GetSuitChannel(suit.ReferenceId, 0)},
                {LogicType.Channel1, (suit) => Functions.GetSuitChannel(suit.ReferenceId, 1)},
                {LogicType.Channel2, (suit) => Functions.GetSuitChannel(suit.ReferenceId, 2)},
                {LogicType.Channel3, (suit) => Functions.GetSuitChannel(suit.ReferenceId, 3)},
                {LogicType.Channel4, (suit) => Functions.GetSuitChannel(suit.ReferenceId, 4)},
                {LogicType.Channel5, (suit) => Functions.GetSuitChannel(suit.ReferenceId, 5)},
                {LogicType.Channel6, (suit) => Functions.GetSuitChannel(suit.ReferenceId, 6)},
                {LogicType.Channel7, (suit) => Functions.GetSuitChannel(suit.ReferenceId, 7)},*/

                // other
                {LogicType.TotalMolesOutput, (suit) => suit.WorldAtmosphere.TotalMoles.ToDouble()},
                {LogicType.TargetPrefabHash, (suit) => CursorManager.CursorThing?.PrefabHash ?? 0.0},
                {LogicType.Time, (suit) => WorldManager.DaysPast},
                {LogicType.Charge, (suit) => suit.Battery?.PowerRatio ?? 0.0},

                // target/looking at position
                {LogicType.TargetX, (suit) => CursorManager.CursorHit.point.x},
                {LogicType.TargetY, (suit) => CursorManager.CursorHit.point.y},
                {LogicType.TargetZ, (suit) => CursorManager.CursorHit.point.x},
        };

    public static readonly Dictionary<LogicType, Action<AdvancedSuit, double>> LogicWriteDictionary = [];
}