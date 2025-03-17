namespace BetterHydroponics;

public static class Functions {
    internal static bool CanLogicRead(LogicSlotType logicSlotType) => Data.LogicSlotReadDictionary.ContainsKey(logicSlotType);
    internal static bool CanLogicRead(LogicType logicType) => Data.LogicReadDictionary.ContainsKey(logicType);

    internal static double GetLogicValue(Plant plant, LogicSlotType logicSlotType, int slotId = 0) => Data.LogicSlotReadDictionary[logicSlotType].Invoke(plant, slotId);
    internal static double GetLogicValue(Plant plant, LogicType logicType) => Data.LogicReadDictionary[logicType].Invoke(plant);
}