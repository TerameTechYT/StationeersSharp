#region

#endregion

namespace StationeersLibrary.Modding.Enums;

// Adapted from Nautilius
// https://github.com/SubnauticaModding/Nautilus/blob/master/Nautilus/Handlers/Enums/EnumBuilder.cs
public sealed class EnumBuilder<TEnum> where TEnum : Enum {
    internal static EnumCacheManager<TEnum> CacheManager { get; } = (EnumCacheManager<TEnum>)EnumCacheProvider.EnsureManager<TEnum>();

    private TEnum _enumValue;

    public TEnum Value => _enumValue;

    private EnumBuilder() { }

    internal static EnumBuilder<TEnum> CreateInstance(string name, Assembly addedBy) {
        var builder = new EnumBuilder<TEnum>();
        if (builder.TryAddEnum(name, addedBy, out _)) {
            return builder;
        }

        return null;
    }

    private bool TryAddEnum(string name, Assembly addedBy, out TEnum enumValue) {
        if (CacheManager.RequestCacheForTypeName(name, false, true) != null) {
            enumValue = default;
            return false;
        }

        if (Enum.GetNames(typeof(TEnum)).Any((x) => x.ToLowerInvariant() == name.ToLowerInvariant())) {
            enumValue = default;
            return false;
        }

        EnumTypeCache cache = CacheManager.RequestCacheForTypeName(name, addedBy: addedBy) ?? new EnumTypeCache() {
            Name = name,
            Index = CacheManager.GetNextAvailableIndex()
        };

        enumValue = (TEnum)Convert.ChangeType(cache.Index, Enum.GetUnderlyingType(typeof(TEnum)));

        CacheManager.Add(enumValue, cache.Index, cache.Name, addedBy);

        _enumValue = enumValue;

        return true;
    }

    public static implicit operator TEnum(EnumBuilder<TEnum> enumBuilder) {
        return enumBuilder.Value;
    }

    public static explicit operator EnumBuilder<TEnum>(TEnum @enum) {
        var enumBuilder = new EnumBuilder<TEnum> {
            _enumValue = @enum
        };

        return enumBuilder;
    }
    public override string ToString() {
        return _enumValue.ToString();
    }
}