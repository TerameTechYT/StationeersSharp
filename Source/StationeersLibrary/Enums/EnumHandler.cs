#region

#endregion

namespace StationeersLibrary.Enums;

// Adapted from Nautilus
// https://github.com/SubnauticaModding/Nautilus/blob/master/Nautilus/Handlers/Enums/EnumHandler.cs
public static class EnumHandler {
    public static EnumBuilder<TEnum>? AddEntry<TEnum>(string name) where TEnum : Enum
        => EnumBuilder<TEnum>.CreateInstance(name);

    public static bool TryAddEntry<TEnum>(string name, out EnumBuilder<TEnum>? builder) where TEnum : Enum
        => (builder = EnumHandler.AddEntry<TEnum>(name)) != null;

    public static bool TryGetValue<TEnum>(string name, out TEnum enumValue) where TEnum : Enum {
        enumValue = default!;

        if (!EnumCacheProvider.TryGetManager(typeof(TEnum), out IEnumCache? manager))
            return false;

        EnumTypeCache? cache = manager.RequestCache(name);
        if (cache == null)
            return false;

        enumValue = (TEnum) Convert.ChangeType(cache.Index, Enum.GetUnderlyingType(typeof(TEnum)));
        return true;
    }

    public static bool EntryExists<TEnum>(string name) where TEnum : Enum
        => EnumHandler.TryGetValue<TEnum>(name, out _);
}