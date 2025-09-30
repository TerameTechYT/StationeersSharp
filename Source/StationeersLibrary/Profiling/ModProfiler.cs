#region

using StationeersLaunchPad;
using StationeersLibrary.Modding;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#endregion

namespace StationeersLibrary.Profiling;

public class ModProfilerManager {
    internal readonly ILogger Logger;

    internal ModProfilerManager(ILogger logger) {
        this.Logger = logger;
    }

    internal void Log(string message, LogSeverity severity) {
        this.Logger?.Log(message, severity);
    }

    public ModProfiler Profile([CallerMemberName] string functionName = "") {
        return new ModProfiler(this, functionName);
    }
}

public class ModProfiler : IDisposable {
    private readonly ModProfilerManager _manager;

    private readonly string _functionName;
    private readonly Stopwatch _stopwatch;

    public TimeSpan Elapsed => this._stopwatch.Elapsed;

    public double ElapsedMilliseconds => this.Elapsed.TotalMilliseconds;

    internal ModProfiler(ModProfilerManager manager, string functionName) {
#if DEBUG
        this._functionName = functionName;
        this._stopwatch = Stopwatch.StartNew();
#endif
    }

    public void Dispose() {
#if DEBUG
        this._stopwatch.Stop();

        this._manager.Log($"{this._functionName} completed, took {this.ElapsedMilliseconds:F3}ms", LogSeverity.Information);
#endif
    }
}
