#region

#endregion

namespace StationeersLibrary.Modding;

public interface IModSingleton<T> where T : IMod {
    static T? Instance { get; }
}
