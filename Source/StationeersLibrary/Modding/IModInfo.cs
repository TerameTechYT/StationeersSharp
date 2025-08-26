#region

#endregion

namespace StationeersLibrary.Mods;

public interface IModInfo {
    /// <summary>
    /// Mod Name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Mod GUID
    /// </summary>
    public string? Guid { get; set; }

    /// <summary>
    /// Mod Version
    /// </summary>
    public Version? Version { get; set; }

    /// <summary>
    /// Mod Workshop Id
    /// </summary>
    public ulong WorkshopId { get; set; }
}