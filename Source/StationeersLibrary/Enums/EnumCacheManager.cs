#region

#endregion

namespace StationeersLibrary.Enums;

// Adapted from Nautilus
// https://github.com/SubnauticaModding/Nautilus/blob/master/Nautilus/Utility/EnumCacheManager.cs

internal class EnumTypeCache(int index, string name) {
    internal int Index = index;
    internal string Name = name;
}

internal static class EnumCacheProvider {
    internal static readonly Dictionary<Type, IEnumCache> CacheManagers = [];

    internal static void RegisterManager(Type enumType, IEnumCache manager) {
        if (!enumType.IsEnum)
            return;

        if (EnumCacheProvider.CacheManagers.ContainsKey(enumType))
            return;

        EnumCacheProvider.CacheManagers.Add(enumType, manager);
    }

    internal static bool TryGetManager(Type enumType, out IEnumCache? manager) {
        manager = null;

        return enumType.IsEnum && EnumCacheProvider.CacheManagers.TryGetValue(enumType, out manager);
    }

    internal static IEnumCache? EnsureManager<TEnum>() where TEnum : Enum {
        if (!EnumCacheProvider.TryGetManager(typeof(TEnum), out IEnumCache? manager)) {
            manager = new EnumCacheManager<TEnum>();
            EnumCacheProvider.RegisterManager(typeof(TEnum), manager);
        }

        return manager;
    }
}

internal interface IEnumCache {
    IEnumerable<object> Keys { get; }
    int Count { get; }

    bool TryGetValue(object? key, out string name);
    bool ContainsKey(object? key);
    bool TryParse(string value, out object? type);
    EnumTypeCache? RequestCache(string name);
}

internal class EnumCacheManager<TEnum> : IEnumCache where TEnum : Enum {
    private class DoubleKeyDictionary : IEnumerable<KeyValuePair<int, string>> {
        private readonly SortedDictionary<int, string> _mapIntString = [];
        private readonly SortedDictionary<TEnum, string> _mapEnumString = [];
        private readonly SortedDictionary<string, TEnum> _mapStringEnum = new(StringComparer.InvariantCultureIgnoreCase);
        private readonly SortedDictionary<string, int> _mapStringInt = new(StringComparer.InvariantCultureIgnoreCase);

        public IEnumerable<TEnum> Keys => this._mapEnumString.Keys;

        public int Count => this._mapEnumString.Count;

        public bool TryGetValue(TEnum enumValue, out string name)
            => this._mapEnumString.TryGetValue(enumValue, out name);

        public bool TryGetValue(string name, out TEnum enumValue)
            => this._mapStringEnum.TryGetValue(name, out enumValue);

        public bool TryGetValue(string name, out int backingValue)
            => this._mapStringInt.TryGetValue(name, out backingValue);

        public void Add(int backingValue, string name) {
            this.Add(EnumCacheManager<TEnum>.ConvertToObject(backingValue), backingValue, name);
        }

        public void Add(TEnum enumValue, int backingValue, string name) {
            this._mapIntString.Add(backingValue, name);
            this._mapEnumString.Add(enumValue, name);
            this._mapStringEnum.Add(name, enumValue);
            this._mapStringInt.Add(name, backingValue);
        }

        public void Remove(int backingValue, string name)
            => this.Remove(EnumCacheManager<TEnum>.ConvertToObject(backingValue), backingValue, name);

        public void Remove(TEnum enumValue, int backingValue, string name) {
            this._mapIntString.Remove(backingValue);
            this._mapEnumString.Remove(enumValue);
            this._mapStringEnum.Remove(name);
            this._mapStringInt.Remove(name);
        }

        public bool IsKnownKey(TEnum key)
            => this._mapEnumString.ContainsKey(key);

        public bool IsKnownKey(string key)
            => this._mapStringEnum.ContainsKey(key);

        public bool IsKnownKey(int key)
            => this._mapIntString.ContainsKey(key);

        public IEnumerator<KeyValuePair<int, string>> GetEnumerator()
            => this._mapIntString.GetEnumerator();

        public void Clear() {
            this._mapIntString.Clear();
            this._mapEnumString.Clear();
            this._mapStringEnum.Clear();
            this._mapStringInt.Clear();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    private int _maxUsedId;
    private readonly HashSet<int> _usedIds = [];
    private readonly DoubleKeyDictionary _entries = [];

    public IEnumerable<TEnum> Keys => this._entries.Keys;
    IEnumerable<object> IEnumCache.Keys => this._entries.Keys.Cast<object>();

    public int Count => this._entries.Count;

    private static TEnum ConvertToObject(int backingValue) {
        return (TEnum) Convert.ChangeType(backingValue, Enum.GetUnderlyingType(typeof(TEnum)));
    }

    internal EnumCacheManager() {
        Array enumValues = Enum.GetValues(typeof(TEnum));
        foreach (object enumValue in enumValues) {
            int realEnumValue = Convert.ToInt32(enumValue);

            if (this._usedIds.Contains(realEnumValue))
                continue;

            this._usedIds.Add(realEnumValue);
            this._maxUsedId = Math.Max(this._maxUsedId, realEnumValue);
        }


        EnumCacheProvider.RegisterManager(typeof(TEnum), this);
    }

    EnumTypeCache? IEnumCache.RequestCache(string name) => this.RequestCache(name);

    internal EnumTypeCache? RequestCache(string name)
        => this._entries.TryGetValue(name, out int value) ? new EnumTypeCache(value, name) : null;

    internal bool TryRequestCache(string name, out EnumTypeCache? cache) {
        if (this._entries.TryGetValue(name, out int value)) {
            cache = new EnumTypeCache(value, name);
            return true;
        }

        cache = null;
        return false;
    }

    internal int GetNextAvailableIndex() {
        bool flags = ReflectionUtilities.HasAttribute<FlagsAttribute>(typeof(TEnum));
        int index = flags ? this._maxUsedId * 2 : this._maxUsedId + 1;

        while (this._entries.IsKnownKey(index) || this._usedIds.Contains(index))
            index++;

        this._maxUsedId = index;
        this._usedIds.Add(index);
        return index;
    }

    bool IEnumCache.TryGetValue(object? value, out string name)
        => this.TryGetValue(EnumCacheManager<TEnum>.ConvertToObject(Convert.ToInt32(value)), out name);

    public bool TryGetValue(TEnum key, out string value)
        => this._entries.TryGetValue(key, out value);

    public bool TryParse(string value, out TEnum type)
        => this._entries.TryGetValue(value, out type);

    bool IEnumCache.TryParse(string value, out object? type) {
        if (this._entries.TryGetValue(value, out TEnum enumValue)) {
            type = enumValue;
            return true;
        }

        type = null;
        return false;
    }

    public void Add(TEnum value, int backingValue, string name) {
        if (!this._entries.IsKnownKey(backingValue))
            this._entries.Add(value, backingValue, name);
    }

    bool IEnumCache.ContainsKey(object? key)
        => this._entries.IsKnownKey(EnumCacheManager<TEnum>.ConvertToObject(Convert.ToInt32(key)));

    public bool ContainsKey(TEnum key)
        => this._entries.IsKnownKey(key);

    public bool ContainsKey(string key)
        => this._entries.IsKnownKey(key);
}