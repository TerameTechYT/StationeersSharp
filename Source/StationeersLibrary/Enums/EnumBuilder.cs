#region

using Debug = UnityEngine.Debug;

#endregion

namespace StationeersLibrary.Enums;

// Adapted from Nautilus
// https://github.com/SubnauticaModding/Nautilus/blob/master/Nautilus/Handlers/Enums/EnumBuilder.cs
public sealed class EnumBuilder<TEnum> where TEnum : Enum {
    internal static EnumCacheManager<TEnum> CacheManager { get; } = (EnumCacheManager<TEnum>) EnumCacheProvider.EnsureManager<TEnum>();

    private TEnum _enumValue;

    public TEnum Value => _enumValue;

    private EnumBuilder() { }

    internal static EnumBuilder<TEnum>? CreateInstance(string name) {
        EnumBuilder<TEnum> builder = new();
        if (builder.TryAddEnum(name, out _))
            return builder;

        return null;
    }

    private bool TryAddEnum(string name, out TEnum enumValue) {
        if (EnumBuilder<TEnum>.CacheManager.TryRequestCache(name, out EnumTypeCache? cache)) {
            Debug.Log($"{name} already exists");

            enumValue = default!;
            return false;
        }

        cache ??= new EnumTypeCache(EnumBuilder<TEnum>.CacheManager.GetNextAvailableIndex(), name);
        enumValue = (TEnum) Convert.ChangeType(cache.Index, Enum.GetUnderlyingType(typeof(TEnum)));
        EnumBuilder<TEnum>.CacheManager.Add(enumValue, cache.Index, cache.Name);
        this._enumValue = enumValue;

        return true;
    }

    public static implicit operator TEnum(EnumBuilder<TEnum> enumBuilder) => enumBuilder == null ? default! : enumBuilder.Value;

    public static explicit operator EnumBuilder<TEnum>(TEnum @enum) => new() {
        _enumValue = @enum
    };

    public override string ToString() => this._enumValue.ToString();
}