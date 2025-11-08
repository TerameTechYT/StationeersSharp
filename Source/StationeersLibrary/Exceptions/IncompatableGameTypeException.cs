#region

#endregion

namespace StationeersLibrary.Exceptions;

public class IncompatableGameTypeException : Exception {
    public IncompatableGameTypeException() { }
    public IncompatableGameTypeException(GameType current, GameType required) : base($"Mod cannot be run on {current}, requires {required}") { }
    public IncompatableGameTypeException(string message) : base(message) { }
    public IncompatableGameTypeException(string message, Exception inner) : base(message, inner) { }
}