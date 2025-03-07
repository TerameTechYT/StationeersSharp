#region

#endregion

namespace StationeersLibrary.Exceptions;

public sealed class IncompatiblePlatformException : Exception {
    public IncompatiblePlatformException(string message) : base(message) {
    }

    public IncompatiblePlatformException(string message, Exception innerException) : base(message, innerException) {
    }

    public IncompatiblePlatformException() {
    }
}