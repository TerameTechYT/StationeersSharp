#region

#endregion

namespace StationeersLibrary.Enums;

[Flags]
public enum SystemType {
    None = 0,
    Windows = 1,
    Linux = 2,
    Macintosh = 4,
    Any = 8,
}
