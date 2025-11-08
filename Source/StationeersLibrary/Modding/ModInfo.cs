#region



#endregion

namespace StationeersLibrary.Modding;

/// <summary>
/// Mod Information Class
/// </summary>
public sealed class ModInfo : IEquatable<ModInfo>, IModInfo {
    /// <inheritdoc/>
    public string? Name { get; set; }

    /// <inheritdoc/>
    public string? Guid { get; set; }

    /// <inheritdoc/>
    public Version? Version { get; set; }

    /// <summary>
    /// Mod Version (as a string)
    /// </summary>
    public string? VersionString => this.Version?.ToString();

    /// <inheritdoc/>
    public ulong WorkshopId { get; set; } = 0ul;

    /// <inheritdoc/>
    public List<ModIncompatibilityInfo> Incompatibilities { get; set; } = [];

    public bool HasIncompatibilities => this.Incompatibilities.Count > 0;

    /// <inheritdoc/>
    public List<ModDependencyInfo> Dependencies { get; set; } = [];

    public bool HasDependencies => this.Dependencies.Count > 0;

    /// <inheritdoc/>
    public GameType GameType { get; set; } = GameType.Both;

    /// <summary>
    /// Is the current game type compatible
    /// </summary>
    /// <returns></returns>
    public bool IsGameCompatible() => this.GameType == GameType.Both || this.GameType == Constants.GAME_TYPE;

    /// <summary>
    /// Is this version equal to the given info
    /// </summary>
    /// <param name="info">Info to compare</param>
    /// <returns>Is Equal</returns>
    public bool VersionEquals(ModInfo info) => this.Version == info?.Version;

    /// <summary>
    /// Is this version newer than the given info
    /// </summary>
    /// <param name="info">Info to compare</param>
    /// <returns>Is Newer</returns>
    public bool VersionNewer(ModInfo info) => this.Version > info?.Version;

    /// <summary>
    /// Is this version older than the given info
    /// </summary>
    /// <param name="info">Info to compare</param>
    /// <returns>Is Older</returns>
    public bool VersionOlder(ModInfo info) => this.Version < info?.Version;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj) => obj is ModInfo info && this.Equals(info);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public bool Equals(ModInfo info) =>
        this.Name == info.Name &&
        this.Guid == info.Guid &&
        this.Version == info.Version &&
        this.WorkshopId == info.WorkshopId &&
        this.Incompatibilities.SequenceEqual(info.Incompatibilities) &&
        this.Dependencies.SequenceEqual(info.Dependencies) &&
        this.GameType == info.GameType;

    /// <summary>
    /// ModInfo as a string
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"{this.Name} ({this.Guid}) - v{this.Version} {(this.WorkshopId == 0ul ? "" : $"- {this.WorkshopId}")}";

    /// <summary>
    /// Hashcode of ModInfo
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode() =>
        HashCode.Combine(this.Name,
            this.Guid,
            this.Version,
            this.WorkshopId,
            this.Incompatibilities,
            this.Dependencies,
            this.GameType);
}