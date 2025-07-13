#region

#endregion

namespace StationeersLibrary.Mods;

public interface IModSingleton<T> where T : IMod {
    static T? Instance { get; }
}
