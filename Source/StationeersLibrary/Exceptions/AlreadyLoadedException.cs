#region

#endregion

namespace StationeersLibrary.Exceptions;

public sealed class AlreadyLoadedException : Exception {
    public AlreadyLoadedException(string name, string guid, string version) : base($"Mod {name} ({guid}) - {version} has already been loaded!") {

    }

    public AlreadyLoadedException(string message) : base(message) {
    }

    public AlreadyLoadedException(string message, Exception innerException) : base(message, innerException) {
    }

    public AlreadyLoadedException() {
    }
}