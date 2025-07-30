#region

#endregion

namespace StationeersLibrary.Enums;

public enum DependencyType {
    /// <summary>
    /// There is no dependency.
    /// </summary>
    None,

    /// <summary>
    /// This dependency is optional.
    /// </summary>
    Soft,

    /// <summary>
    /// This dependency is required.
    /// </summary>
    Hard,
}
