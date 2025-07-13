#region

#endregion

namespace BetterHydroponics;

public class Plugin : Mod, IModSingleton<Plugin> {
    public static Plugin? Instance { get; private set; }

    public override bool UseLogger => true;
    public override bool UseConfig => true;
    public override bool UseHarmony => true;

    public override ModInfo Data => new ModInfo() {
        Name = "BetterHydroponics",
        Guid = "betterhydroponics",
        Version = new Version(1, 4, 0, 140),
        WorkshopId = 3449149492ul,
        GameType = GameType.Both,
    };

    public Plugin() : base() => Plugin.Instance = this;

    public override void OnStart() {}
}

internal struct ConfigData {
    public static readonly Dictionary<LogicSlotType, Func<Plant, int, double>> PlantReadDictionary = new() {
                { LogicSlotType.Temperature, (plant, slotId) => plant?.PlantStatus.TemperatureEfficiency ?? 0.0},
                { LogicSlotType.Pressure, (plant, slotId) => plant?.PlantStatus.PressureEfficiency ?? 0.0},
                { LogicSlotType.PressureAir, (plant, slotId) => plant?.PlantStatus.BreathingEfficiency ?? 0.0},
                { LogicSlotType.Volume, (plant, slotId) => plant?.PlantStatus.HydrationEfficiency ?? 0.0},
                { LogicSlotType.Charge, (plant, slotId) => plant?.PlantStatus.LightEfficiency ?? 0.0},
                { LogicSlotType.On, (plant, slotId) => plant?.PlantRecord.Age ?? 0.0},
                { LogicSlotType.Lock, (plant, slotId) => plant?.PlantRecord.LightStress ?? 0.0},
                { LogicSlotType.Open, (plant, slotId) => plant?.PlantRecord.TimeLitRatio ?? 0.0},
                { LogicSlotType.Mode, (plant, slotId) => plant?.PlantRecord.TimeDarknessRatio ?? 0.0},
        };

    /*public static readonly Dictionary<LogicType, Func<Plant, double>> LogicReadDictionary = new() {
            { LogicType.TemperatureSetting, (plant) => plant?.PlantStatus.TemperatureEfficiency ?? 0.0 },
            { LogicType.PressureEfficiency, (plant) => plant?.PlantStatus.PressureEfficiency ?? 0.0 },
            { LogicType.PressureSetting, (plant) => plant?.PlantStatus.BreathingEfficiency ?? 0.0 },
            { LogicType.RatioWaterInput, (plant) => plant?.PlantStatus.HydrationEfficiency ?? 0.0 },
            { LogicType.Charge, (plant) => plant?.PlantStatus.LightEfficiency ?? 0.0 },
            { LogicType.Setting, (plant) => plant?.PlantRecord.LightStress ?? 0.0 },
            { LogicType.SettingInput, (plant) => plant?.PlantRecord.TimeLitRatio ?? 0.0 },
            { LogicType.SettingOutput, (plant) => plant?.PlantRecord.TimeDarknessRatio ?? 0.0 },
            { LogicType.Time, (plant) => plant?.PlantRecord.Age ?? 0.0 },
    };*/
}