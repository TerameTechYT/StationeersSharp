namespace BetterHydroponics;

public static class Functions {
    internal static bool CanLogicRead(LogicSlotType logicSlotType) => ConfigData.PlantReadDictionary.ContainsKey(logicSlotType);
    //internal static bool CanLogicRead(LogicType logicType) => ConfigData.LogicReadDictionary.ContainsKey(logicType);

    internal static double GetLogicValue(Plant plant, LogicSlotType logicSlotType, int slotId = 0) => ConfigData.PlantReadDictionary[logicSlotType].Invoke(plant, slotId);
    //internal static double GetLogicValue(Plant plant, LogicType logicType) => ConfigData.LogicReadDictionary[logicType].Invoke(plant);
}