#region

#endregion

namespace StationeersLibrary.Modding;

public class ModDependencyInfo : IModInfo {
    /// <inheritdoc/>
    public string? Name { get; set; }

    /// <inheritdoc/>
    public string? Guid { get; set; }

    /// <inheritdoc/>
    public Version? Version { get; set; }

    /// <summary>
    /// Max version (if null, no max version)
    /// </summary>
    public Version? MaxVersion { get; set; }

    /// <summary>
    /// Mod Version (as a string)
    /// </summary>
    public string? VersionString => this.Version?.ToString();

    /// <summary>
    /// Max Mod Version (as a string)
    /// </summary>
    public string? MaxVersionString => this.MaxVersion?.ToString();

    /// <inheritdoc/>
    public ulong WorkshopId { get; set; } = 0ul;

    /// <inheritdoc/>
    public DependencyType DependencyType { get; set; } = DependencyType.None;

    /// <summary>
    /// Is this version equal to the given info
    /// </summary>
    /// <param name="info">Info to compare</param>
    /// <returns>Is Equal</returns>
    public bool VersionEquals(ModDependencyInfo info) => this.Version == info?.Version;

    /// <summary>
    /// Is this version newer than the given info
    /// </summary>
    /// <param name="info">Info to compare</param>
    /// <returns>Is Newer</returns>
    public bool VersionNewer(ModDependencyInfo info) => this.Version > info?.Version;

    /// <summary>
    /// Is this version older than the given info
    /// </summary>
    /// <param name="info">Info to compare</param>
    /// <returns>Is Older</returns>
    public bool VersionOlder(ModDependencyInfo info) => this.Version < info?.Version;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj) => obj is ModDependencyInfo info && this.Equals(info);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public bool Equals(ModDependencyInfo info) =>
        this.Name == info.Name &&
        this.Guid == info.Guid &&
        this.Version == info.Version &&
        this.WorkshopId == info.WorkshopId &&
        this.DependencyType == info.DependencyType;

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
            this.DependencyType);
}
