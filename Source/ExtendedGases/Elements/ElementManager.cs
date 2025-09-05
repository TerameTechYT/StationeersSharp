#region

using static Assets.Scripts.Atmospherics.Chemistry;

#endregion


namespace ExtendedGases.Elements;
public static class ElementManager {
    private static readonly object _lock = new();

    private static readonly Dictionary<string, Element> _elements = [];
    private static readonly Dictionary<GasType, string> _gasTypeString = [];

    public static Dictionary<string, Element> Elements => ElementManager._elements;

    public static Element Get(string name) {
        return ElementManager._elements[name];
    }

    public static bool TryGet(string name, out Element element) {
        return ElementManager._elements.TryGetValue(name, out element);
    }

    public static Element Get(GasType gasType) {
        return ElementManager.Get(ElementManager._gasTypeString[gasType]);
    }

    public static bool TryGet(GasType gasType, out Element element) {
        return ElementManager.TryGet(ElementManager._gasTypeString[gasType], out element);
    }

    public static void Add(Element element) {
        lock (_lock) {
            ElementManager._elements.Add(element.GasName, element);
            ElementManager._gasTypeString.Add(element.GasType, element.GasName);
            ElementManager._gasTypeString.Add(element.LiquidType, element.LiquidName);
        }
    }

    public static void Remove(Element element) {
        lock (_lock) {
            ElementManager._elements.Remove(element.GasName);
            ElementManager._gasTypeString.Remove(element.GasType);
            ElementManager._gasTypeString.Remove(element.LiquidType);
        }
    }
}
