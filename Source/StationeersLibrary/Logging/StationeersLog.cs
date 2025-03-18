#region

#endregion

namespace StationeersLibrary.Logging;

public static class StationeersLog {
    private static StreamWriter _fileStream;
    internal static StreamWriter LogStream => _fileStream ??= new StreamWriter(Constants.LOG_FILE);

    public static void LogException(string name, Exception ex) => StationeersLog.Log(name, $"[{ex.Source} - {ex.StackTrace}]: {ex.Message}", Severity.Error);
    public static void LogError(string name, string message) => StationeersLog.Log(name, message, Severity.Error);
    public static void LogWarning(string name, string message) => StationeersLog.Log(name, message, Severity.Warning);
    public static void LogInfo(string name, string message) => StationeersLog.Log(name, message, Severity.Info);

    public static void LogDebug(string name, string message) {
        if (Constants.DEBUG_MODE) {
            StationeersLog.Log(name, message, Severity.Info);
        }
    }

    private static void Log(string name, string message, Severity severity) {
        string newMessage = $"[{name}]: {message}";

        switch (severity) {
            case Severity.Error: {
                ConsoleWindow.PrintError(newMessage);
                break;
            }
            case Severity.Warning: {
                ConsoleWindow.PrintAction(newMessage);
                break;
            }
            case Severity.Info: {
                ConsoleWindow.Print(newMessage);
                break;
            }
            default:
            case Severity.Debug: {
                ConsoleWindow.Print(newMessage, color: ConsoleColor.Gray, aged: false);
                break;
            }
        }

        LogStream.WriteLine($"[{name} - {severity}]: {message}");
    }
}
