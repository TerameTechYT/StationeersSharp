#region

using System.Diagnostics;
using System.Reflection;

#endregion

namespace StationeersLibrary.Modding;

// Adapted from https://github.com/TheRealBeef/Stationeers-ModProfile-Lib/
public class ModProfiler : IDisposable {
    internal static readonly Dictionary<MethodBase, int> _belowThresholdCounts = [];

    public const double NORMAL_THRESHOLD = 0.001;
    public const double UPDATE_THRESHOLD = 1;

    //private readonly Logger _logger;
    private readonly MethodBase _method;
    private readonly bool _isUpdateMethod;
    private readonly Stopwatch _stopwatch;

    public TimeSpan Elapsed => this._stopwatch.Elapsed;

    public double ElapsedMilliseconds => this.Elapsed.TotalMilliseconds;

    internal ModProfiler(/* Logger logger,*/ MethodBase method, bool isUpdateMethod = false) {
#if DEBUG
        this._logger = logger;
        this._method = method;
        this._isUpdateMethod = isUpdateMethod;
        this._stopwatch = Stopwatch.StartNew();
#endif
    }

    public void Dispose() {
#if DEBUG
        this._stopwatch.Stop();

        double threshold = this._isUpdateMethod ? UPDATE_THRESHOLD : NORMAL_THRESHOLD;
        if (this.ElapsedMilliseconds > threshold) {
            if (ModProfiler._belowThresholdCounts.TryGetValue(this._method, out int value)) {
                ModProfiler._belowThresholdCounts[this._method] = 0;
            }

            this._logger.LogDebug($"{this._method.Name} completed in {this.ElapsedMilliseconds:F3} ms{(value > 0 ? $" - {value} executions below threshold ({threshold:F3})ms" : "")}");
        } else {
            if (ModProfiler._belowThresholdCounts.TryGetValue(this._method, out int value)) {
                ModProfiler._belowThresholdCounts[this._method] = value + 1;
            } else {
                ModProfiler._belowThresholdCounts.TryAdd(this._method, 1);
            }
        }
#endif
    }
}