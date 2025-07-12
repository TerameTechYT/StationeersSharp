namespace StationeersLibrary.Mods;

/// <summary>
/// Mod Information Class
/// </summary>
public sealed class ModInfo {
    /// <summary>
    /// Mod Name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Mod GUID
    /// </summary>
    public string Guid { get; set; }

    /// <summary>
    /// Mod Version
    /// </summary>
    public Version Version { get; set; }

    /// <summary>
    /// Mod Version (as a string)
    /// </summary>
    public string VersionString => this.Version.ToString();

    /// <summary>
    /// Mod Workshop Id
    /// </summary>
    public ulong WorkshopId { get; set; }

    /// <summary>
    /// Mod Game Type
    /// </summary>
    public GameType GameType { get; set; }

    /// <summary>
    /// Is the current game type compatible
    /// </summary>
    /// <returns></returns>
    public bool IsGameCompatible() => this.GameType == GameType.Both ? true : this.GameType == Constants.GameType;

    /// <summary>
    /// Is this version equal to the given info
    /// </summary>
    /// <param name="info">Info to compare</param>
    /// <returns>Is Equal</returns>
    public bool Equals(ModInfo info) => this.Version == info?.Version;

    /// <summary>
    /// Is this version newer than the given info
    /// </summary>
    /// <param name="info">Info to compare</param>
    /// <returns>Is Newer</returns>
    public bool Newer(ModInfo info) => this.Version > info?.Version;

    /// <summary>
    /// Is this version older than the given info
    /// </summary>
    /// <param name="info">Info to compare</param>
    /// <returns>Is Older</returns>
    public bool Older(ModInfo info) => this.Version < info?.Version;
}