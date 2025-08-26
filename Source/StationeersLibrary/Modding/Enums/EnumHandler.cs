#region

#endregion

namespace StationeersLibrary.Modding.Enums;

// Adapted from Nautilius
// https://github.com/SubnauticaModding/Nautilus/blob/master/Nautilus/Utility/EnumCacheManager.cs
public static class EnumHandler {
    public static EnumBuilder<TEnum> AddEntry<TEnum>(string name, Assembly ownerAssembly) where TEnum : Enum {
        return EnumBuilder<TEnum>.CreateInstance(name, ownerAssembly);
    }

    public static EnumBuilder<TEnum> AddEntry<TEnum>(string name) where TEnum : Enum {
        var callingAssembly = Assembly.GetCallingAssembly();
        callingAssembly = callingAssembly == Assembly.GetExecutingAssembly()
            ? ReflectionUtilities.CallingAssemblyByStackTrace()
            : callingAssembly;

        return AddEntry<TEnum>(name, callingAssembly);
    }

    public static bool TryAddEntry<TEnum>(string name, Assembly ownerAssembly, out EnumBuilder<TEnum> builder) where TEnum : Enum {
        return (builder = AddEntry<TEnum>(name, ownerAssembly)) != null;
    }

    public static bool TryAddEntry<TEnum>(string name, out EnumBuilder<TEnum> builder) where TEnum : Enum {
        return (builder = AddEntry<TEnum>(name)) != null;
    }

    public static bool TryGetValue<TEnum>(string name, out TEnum enumValue) where TEnum : Enum {
        enumValue = default;

        if (!EnumCacheProvider.TryGetManager(typeof(TEnum), out var manager))
            return false;

        var cache = manager.RequestCacheForTypeName(name, false, true);

        if (cache != null) {
            enumValue = (TEnum)Convert.ChangeType(cache.Index, Enum.GetUnderlyingType(typeof(TEnum)));
            return true;
        }

        return false;
    }
    public static bool TryGetOwnerAssembly<TEnum>(TEnum modEnumValue, out Assembly addedBy) where TEnum : Enum {
        addedBy = null;
        if (!EnumCacheProvider.TryGetManager(typeof(TEnum), out IEnumCache manager))
            return false;

        if (manager.TypesAddedBy.TryGetValue(modEnumValue.ToString(), out addedBy))
            return true;

        return false;
    }

    public static bool TryGetValue<TEnum>(string name, out TEnum enumValue, out Assembly addedBy) where TEnum : Enum {
        enumValue = default;
        addedBy = null;

        if (!EnumCacheProvider.TryGetManager(typeof(TEnum), out IEnumCache manager))
            return false;

        var cache = manager.RequestCacheForTypeName(name, false, true);

        if (cache != null) {
            enumValue = (TEnum)Convert.ChangeType(cache.Index, Enum.GetUnderlyingType(typeof(TEnum)));
            addedBy = manager.TypesAddedBy[enumValue.ToString()];
            return true;
        }

        return false;
    }

    public static bool ModdedEnumExists<TEnum>(string name) where TEnum : Enum {
        return TryGetValue<TEnum>(name, out _);
    }
}