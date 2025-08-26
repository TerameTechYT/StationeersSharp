#region

#endregion

namespace StationeersLibrary.Modding.Enums;

// Adapted from Nautilius
// https://github.com/SubnauticaModding/Nautilus/blob/master/Nautilus/Utility/EnumCacheManager.cs

internal class EnumTypeCache {
    internal int Index;
    internal string Name;

    public EnumTypeCache() {}

    public EnumTypeCache(int index, string name) {
        Index = index;
        Name = name;
    }
}

internal static class EnumCacheProvider {
    internal static Dictionary<Type, IEnumCache> CacheManagers { get; } = [];

    internal static void RegisterManager(Type enumType, IEnumCache manager) {
        if (!enumType.IsEnum)
            return;

        if (EnumCacheProvider.CacheManagers.ContainsKey(enumType))
            return;

        EnumCacheProvider.CacheManagers.Add(enumType, manager);
    }

    internal static bool TryGetManager(Type enumType, out IEnumCache manager) {
        manager = null;

        return enumType.IsEnum && EnumCacheProvider.CacheManagers.TryGetValue(enumType, out manager);
    }

    internal static IEnumCache EnsureManager<TEnum>() where TEnum : Enum {
        if (!EnumCacheProvider.TryGetManager(typeof(TEnum), out var manager)) {
            manager = new EnumCacheManager<TEnum>();
            EnumCacheProvider.RegisterManager(typeof(TEnum), manager);
        }

        return manager;
    }
}

internal interface IEnumCache {
    Dictionary<string, Assembly> TypesAddedBy { get; }
    IEnumerable<object> ModdedKeys { get; }
    int ModdedKeysCount { get; }
    bool TryGetValue(object key, out string name);
    bool ContainsKey(object key);
    bool TryParse(string value, out object type);
    EnumTypeCache RequestCacheForTypeName(string name, bool checkDeactivated = true, bool checkRequestedOnly = false, Assembly? addedBy = null);
}

internal class EnumCacheManager<TEnum> : IEnumCache where TEnum : Enum {
    private class DoubleKeyDictionary : IEnumerable<KeyValuePair<int, string>> {
        private readonly SortedDictionary<int, string> _mapIntString = [];
        private readonly SortedDictionary<TEnum, string> _mapEnumString = [];

        private readonly SortedDictionary<string, TEnum> _mapStringEnum =
            new(StringComparer.InvariantCultureIgnoreCase);

        private readonly SortedDictionary<string, int> _mapStringInt =
            new(StringComparer.InvariantCultureIgnoreCase);

        public bool TryGetValue(TEnum enumValue, out string name) {
            return this._mapEnumString.TryGetValue(enumValue, out name);
        }

        public bool TryGetValue(string name, out TEnum enumValue) {
            return this._mapStringEnum.TryGetValue(name, out enumValue);
        }

        public bool TryGetValue(string name, out int backingValue) {
            return this._mapStringInt.TryGetValue(name, out backingValue);
        }

        public void Add(int backingValue, string name) {
            var enumValue = ConvertToObject(backingValue);
            this.Add(enumValue, backingValue, name);
        }

        public void Add(TEnum enumValue, int backingValue, string name) {
            this._mapIntString.Add(backingValue, name);
            this._mapEnumString.Add(enumValue, name);
            this._mapStringEnum.Add(name, enumValue);
            this._mapStringInt.Add(name, backingValue);

            if (backingValue > this.LargestIntValue)
                this.LargestIntValue = backingValue;
        }

        public void Remove(int backingValue, string name) {
            var enumValue = ConvertToObject(backingValue);
            this.Remove(enumValue, backingValue, name);
        }

        public void Remove(TEnum enumValue, int backingValue, string name) {
            this._mapIntString.Remove(backingValue);
            this._mapEnumString.Remove(enumValue);
            this._mapStringEnum.Remove(name);
            this._mapStringInt.Remove(name);
        }

        public int LargestIntValue { get; private set; }

        public IEnumerable<TEnum> KnownsEnumKeys => this._mapEnumString.Keys;

        public int KnownsEnumCount => this._mapEnumString.Count;

        public bool IsKnownKey(TEnum key) {
            return this._mapEnumString.ContainsKey(key);
        }

        public bool IsKnownKey(string key) {
            return this._mapStringEnum.ContainsKey(key);
        }

        public bool IsKnownKey(int key) {
            return this._mapIntString.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<int, string>> GetEnumerator() {
            return this._mapIntString.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this._mapIntString.GetEnumerator();
        }

        public void Clear() {
            this._mapIntString.Clear();
            this._mapEnumString.Clear();
            this._mapStringEnum.Clear();
            this._mapStringInt.Clear();
        }
    }

    private static readonly Type _underlyingType = Enum.GetUnderlyingType(typeof(TEnum));

    internal readonly string EnumTypeName = typeof(TEnum).DeclaringType is { } d
        ? $"{d.Name}{typeof(TEnum).Name}"
        : $"{typeof(TEnum).Name}";

    private readonly HashSet<int> _usedIds = [];
    private readonly int _maxUsedId;

    private readonly DoubleKeyDictionary entries = [];

    public IEnumerable<TEnum> ModdedKeys => this.entries.KnownsEnumKeys;
    IEnumerable<object> IEnumCache.ModdedKeys => this.entries.KnownsEnumKeys.Cast<object>();

    public int ModdedKeysCount => this.entries.KnownsEnumCount;

    private readonly Dictionary<string, Assembly> _typesAddedBy = [];
    public Dictionary<string, Assembly> TypesAddedBy => this._typesAddedBy;

    bool IEnumCache.TryGetValue(object value, out string name) {
        return this.TryGetValue(ConvertToObject(Convert.ToInt32(value)), out name);
    }

    public bool TryGetValue(TEnum key, out string value) {
        return this.entries.TryGetValue(key, out value);
    }

    public bool TryParse(string value, out TEnum type) {
        return this.entries.TryGetValue(value, out type);
    }

    public string ValueToName(TEnum value) {
        if (this.entries.TryGetValue(value, out var name))
            return name;
        return null;
    }

    bool IEnumCache.TryParse(string value, out object type) {
        if (this.entries.TryGetValue(value, out TEnum enumValue)) {
            type = enumValue;
            return true;
        }

        type = null;
        return false;
    }

    public void Add(TEnum value, int backingValue, string name, Assembly addedBy) {
        if (!this.entries.IsKnownKey(backingValue)) {
            this.entries.Add(value, backingValue, name);
            this._typesAddedBy[name] = addedBy;
        }
    }

    bool IEnumCache.ContainsKey(object key) {
        return this.entries.IsKnownKey(ConvertToObject(Convert.ToInt32(key)));
    }
    public bool ContainsEnumKey(TEnum key) {
        return this.entries.IsKnownKey(key);
    }
    public bool ContainsStringKey(string key) {
        return this.entries.IsKnownKey(key);
    }

    internal EnumCacheManager() {
        this._maxUsedId = Enum.GetValues(typeof(TEnum)).Length;
        for (int i = 0; i < this._maxUsedId; i++) {
            this._usedIds.Add(i);
        }

        EnumCacheProvider.RegisterManager(typeof(TEnum), this);
    }

    private static TEnum ConvertToObject(int backingValue) {
        return (TEnum)Convert.ChangeType(backingValue, _underlyingType);
    }

    EnumTypeCache IEnumCache.RequestCacheForTypeName(string name, bool checkDeactivated, bool checkRequestedOnly, Assembly? addedBy) {
        return this.RequestCacheForTypeName(name, checkDeactivated, checkRequestedOnly, addedBy);
    }

    internal EnumTypeCache RequestCacheForTypeName(string name, bool checkDeactivated = true, bool checkRequestedOnly = false, Assembly? addedBy = null) {
        if (this.entries.TryGetValue(name, out int value)) {
            return new EnumTypeCache(value, name);
        }

        return null;
    }

    internal int GetNextAvailableIndex() {
        int index = this._maxUsedId + 1;

        while (this.entries.IsKnownKey(index) ||             
               this._usedIds.Contains(index)) {
            index++;
        }

        return index;
    }
}