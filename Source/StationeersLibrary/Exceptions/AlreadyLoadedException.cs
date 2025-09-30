#region

using StationeersLibrary.Modding;

#endregion

namespace StationeersLibrary.Exceptions;

public class AlreadyLoadedException : Exception {
    public AlreadyLoadedException(ModInfo info) : base($"Mod {info.Name} ({info.Guid}) - {info.Version} has already been loaded!") {
    }

    public AlreadyLoadedException(string name, string guid, string version) : base($"Mod {name} ({guid}) - {version} has already been loaded!") {
    }

    public AlreadyLoadedException(string message) : base(message) {
    }

    public AlreadyLoadedException(string message, Exception innerException) : base(message, innerException) {
    }

    public AlreadyLoadedException() {
    }
}