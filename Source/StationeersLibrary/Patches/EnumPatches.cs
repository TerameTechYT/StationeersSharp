#region

using HarmonyLib;
using StationeersLibrary.Enums;
using System.Collections;

#endregion

namespace StationeersLibrary.Patches;

[HarmonyPatch]
public static class EnumPatches {
    [HarmonyPatch(typeof(Enum), nameof(Enum.GetValues)), HarmonyPostfix]
    public static void EnumGetValuesPostfix(Type enumType, ref Array __result) {
        if (EnumCacheProvider.TryGetManager(enumType, out IEnumCache? manager)) {
            __result = EnumPatches.GetValues(enumType, manager, __result);
        }
    }


    [HarmonyPatch(typeof(Enum), nameof(Enum.GetNames)), HarmonyPostfix]
    public static void EnumGetNamesPostfix(Type enumType, ref Array __result) {
        if (EnumCacheProvider.TryGetManager(enumType, out IEnumCache? manager)) {
            __result = EnumPatches.GetNames(manager, __result);
        }
    }

    [HarmonyPatch(typeof(Enum), nameof(Enum.GetName)), HarmonyPrefix]
    public static bool EnumGetNamePostfix(Type enumType, object value, ref string __result) {
        if (EnumCacheProvider.TryGetManager(enumType, out IEnumCache? manager) && (manager?.TryGetValue(value, out string? name) ?? false)) {
            __result = name;
            return false;
        }

        return true;
    }


    [HarmonyPatch(typeof(Enum), nameof(Enum.IsDefined)), HarmonyPrefix]
    public static bool EnumIsDefinedPrefix(Type enumType, object value, ref bool __result) {
        if (EnumCacheProvider.TryGetManager(enumType, out IEnumCache? manager) && EnumPatches.IsDefined(manager, value)) {
            __result = true;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(Enum), nameof(Enum.Parse), [typeof(Type), typeof(string), typeof(bool)]), HarmonyPrefix]
    public static bool EnumParsePrefix(Type enumType, string value, bool ignoreCase, ref object __result) {
        if (EnumCacheProvider.TryGetManager(enumType, out IEnumCache? manager) && (manager?.TryParse(value, out object? obj) ?? false)) {
            __result = obj!;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(Enum), nameof(Enum.ToString), []), HarmonyPrefix]
    public static bool EnumTostringPrefix(Enum __instance, ref string __result) {
        return !EnumCacheProvider.TryGetManager(__instance.GetType(), out IEnumCache? manager) || !(manager?.TryGetValue(__instance, out __result) ?? false);
    }

    private static bool IsDefined(IEnumCache? cacheManager, object value) {
        return cacheManager?.ContainsKey(value) ?? false;
    }

    private static Array GetValues(Type enumType, IEnumCache cacheManager, Array __result) {
        Type genericListType = typeof(List<>).MakeGenericType(enumType);
        IList list = (IList) Activator.CreateInstance(genericListType);
        foreach (object? type in __result) {
            list.Add(type);
        }
        foreach (object type2 in cacheManager.Keys) {
            list.Add(type2);
        }

        Array array = Array.CreateInstance(enumType, list.Count);
        list.CopyTo(array, 0);

        return array;
    }

    private static Array GetNames(IEnumCache? cacheManager, Array __result) {
        if (cacheManager == null)
            return Array.Empty<string>();

        List<string> list = [.. __result.Cast<string>()];

        foreach (object type in cacheManager.Keys) {
            if (cacheManager.TryGetValue(type, out string name))
                list.Add(name);
        }

        return list.ToArray();
    }
}