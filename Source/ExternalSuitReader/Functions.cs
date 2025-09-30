#region

using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Clothing;
using Assets.Scripts.Objects.Motherboards;
using System.Xml.Serialization;

#endregion

namespace ExternalSuitReader;

internal static class Functions {
    internal static bool CanLogicRead(LogicType logicType) => Data.LogicReadDictionary.ContainsKey(logicType);
    internal static bool CanLogicWrite(LogicType logicType) => Data.LogicWriteDictionary.ContainsKey(logicType);

    internal static double GetLogicValue(AdvancedSuit suit, LogicType logicType) => Data.LogicReadDictionary[logicType].Invoke(suit);
    internal static void WriteLogicValue(AdvancedSuit suit, LogicType logicType, double value) => Data.LogicWriteDictionary[logicType].Invoke(suit, value);

    internal static double GetSuitChannel(long referenceId, int channel) => !Data.AllAdvancedSuits.TryGetValue(referenceId, out List<DoubleReference> channels) ? 0.0 : channels[channel].Value;

    internal static void SetSuitChannel(long referenceId, int channel, double value) {
        if (Data.AllAdvancedSuits.TryGetValue(referenceId, out List<DoubleReference> channels)) {
            channels[channel].Value = value;
        }
    }
}

[XmlInclude(typeof(SuitSaveData))]
public class AdvancedSuitSaveData(List<DoubleReference> channels) : SuitSaveData {
    [XmlElement]
    [XmlArrayItem("Channel")]
    public List<DoubleReference> Channels = channels;

    public static AdvancedSuitSaveData? Create(ThingSaveData reference, List<DoubleReference> channels) {
        if (reference is AdvancedSuitSaveData data) {
            data.Channels = channels;

            return data;
        }

        return null;
    }
}