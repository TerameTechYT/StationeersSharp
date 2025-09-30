#region

using StationeersLaunchPad;

#endregion

namespace StationeersLibrary.Modding;

public interface ILogger {
    /// <summary>
    /// Log function that is redirected to <see cref="Logger"/>
    /// Can be overriden to add or remove functionality.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="severity"></param>
    public abstract void Log(string message, LogSeverity severity = LogSeverity.Information);

    /// <summary>
    /// Log function that is redirected to <see cref="Logger"/>
    /// Can be overriden to add or remove functionality.
    /// </summary>
    /// <param name="exception">Exception</param>
    public abstract void Log(Exception exception);

    /// <summary>
    /// Shortcut function <see cref="Log"/> to log a <see cref="LogSeverity.Debug"/> severity message.
    /// Does not log anything if not in debug mode.
    /// </summary>
    /// <param name="message">string</param>
    public abstract void LogDebug(string message);

    /// <summary>
    /// Shortcut function <see cref="Log"/> to log a <see cref="LogSeverity.Information"/> severity message.
    /// </summary>
    /// <param name="message">string</param>
    public abstract void LogInfo(string message);

    /// <summary>
    /// Shortcut function <see cref="Log"/> to log a <see cref="LogSeverity.Warning"/> severity message.
    /// </summary>
    /// <param name="message">string</param>
    public abstract void LogWarning(string message);

    /// <summary>
    /// Shortcut function <see cref="Log"/> to log a <see cref="LogSeverity.Error"/> severity message.
    /// </summary>
    /// <param name="message">string</param>
    public abstract void LogError(string message);

    /// <summary>
    /// Shortcut function <see cref="Log"/> to log a <see cref="Exception"/>
    /// </summary>
    /// <param name="message">string</param>
    public abstract void LogException(Exception exception);

    /// <summary>
    /// Shortcut function <see cref="Log"/> to log a <see cref="LogSeverity.Fatal"/> severity message.
    /// </summary>
    /// <param name="message">string</param>
    public abstract void LogFatal(string message);

    /// <summary>
    /// Log function that is redirected to <see cref="Logger"/>
    /// Can be overriden to add or remove functionality.
    /// </summary>
    /// <param name="severity">LogSeverity</param>
    /// <param name="format">string</param>
    /// <param name="args">params object[]</param>
    public abstract void LogFormat(LogSeverity severity, string format, params object[] args);

    /// <summary>
    /// Shortcut function <see cref="LogFormat"/> to log a formatted <see cref="LogSeverity.Debug"/> severity message.
    /// Does not log anything if not in debug mode.
    /// </summary>
    /// <param name="message">string</param>
    public abstract void LogDebugFormat(string message, params object[] args);
    /// <summary>
    /// Shortcut function <see cref="LogFormat"/> to log a formatted <see cref="LogSeverity.Information"/> severity message.
    /// </summary>
    /// <param name="message">string</param>
    public abstract void LogInfoFormat(string message, params object[] args);

    /// <summary>
    /// Shortcut function <see cref="LogFormat"/> to log a formatted <see cref="LogSeverity.Warning"/> severity message.
    /// </summary>
    /// <param name="message">string</param>
    public abstract void LogWarningFormat(string message, params object[] args);

    /// <summary>
    /// Shortcut function <see cref="LogFormat"/> to log a formatted <see cref="LogSeverity.Exception"/> severity message.
    /// </summary>
    /// <param name="message">string</param>
    public abstract void LogErrorFormat(string message, params object[] args);

    /// <summary>
    /// Shortcut function <see cref="LogFormat"/> to log a formatted <see cref="LogSeverity.Fatal"/> severity message.
    /// </summary>
    /// <param name="message">string</param>
    public abstract void LogFatalFormat(string message, params object[] args);
}
