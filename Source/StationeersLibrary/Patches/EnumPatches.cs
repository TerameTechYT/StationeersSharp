#region

#endregion


namespace StationeersLibrary.Patches;

[HarmonyPatch]
public static class EnumPatches {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Enum), nameof(Enum.GetValues))]
    public static void EnumGetValuesPostfix(Type enumType, ref Array __result) {
        if (EnumCacheProvider.TryGetManager(enumType, out var manager)) {
            __result = EnumPatches.GetValues(enumType, manager, __result);
        }
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(Enum), nameof(Enum.GetNames))]
    public static void EnumGetNamesPostfix(Type enumType, ref Array __result) {
        if (EnumCacheProvider.TryGetManager(enumType, out var manager)) {
            __result = EnumPatches.GetNames(manager, __result);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Enum), nameof(Enum.GetName))]
    public static bool EnumGetNamePostfix(Type enumType, object value, ref string __result) {
        if (EnumCacheProvider.TryGetManager(enumType, out var manager) && manager.TryGetValue(value, out var name)) {
            __result = name;
            return false;
        }

        return true;
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(Enum), nameof(Enum.IsDefined))]
    public static bool EnumIsDefinedPrefix(Type enumType, object value, ref bool __result) {
        if (EnumCacheProvider.TryGetManager(enumType, out var manager) && EnumPatches.IsDefined(manager, value)) {
            __result = true;
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Enum), nameof(Enum.Parse), [typeof(Type), typeof(string), typeof(bool)])]
    public static bool EnumParsePrefix(Type enumType, string value, bool ignoreCase, ref object __result) {
        if (EnumCacheProvider.TryGetManager(enumType, out var manager) && manager.TryParse(value, out var obj)) {
            __result = obj;
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Enum), nameof(Enum.ToString), [])]
    public static bool EnumTostringPrefix(Enum __instance, ref string __result) {
        if (EnumCacheProvider.TryGetManager(__instance.GetType(), out var manager) &&
            manager.TryGetValue(__instance, out __result)) {
            return false;
        }

        return true;
    }

    private static bool IsDefined(IEnumCache cacheManager, object value) {
        return cacheManager.ContainsKey(value);
    }

    private static Array GetValues(Type enumType, IEnumCache cacheManager, Array __result) {
        Type genericListType = typeof(List<>).MakeGenericType(enumType);
        IList list = (IList)Activator.CreateInstance(genericListType);
        foreach (var type in __result) {
            list.Add(type);
        }
        foreach (var type2 in cacheManager.Keys) {
            list.Add(type2);
        }

        Array array = Array.CreateInstance(enumType, list.Count);
        list.CopyTo(array, 0);

        return array;
    }

    private static Array GetNames(IEnumCache cacheManager, Array __result) {
        var list = new List<string>();
        foreach (string type in __result) {
            list.Add(type);
        }

        foreach (var type in cacheManager.Keys) {
            if (cacheManager.TryGetValue(type, out string name))
                list.Add(name);
        }

        return list.ToArray();
    }
}
