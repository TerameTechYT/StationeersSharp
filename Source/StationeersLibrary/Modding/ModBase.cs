#region

using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using StationeersLibrary.Args;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

#endregion

namespace StationeersLibrary.Modding;

/// <summary>
/// Base class for handling modding
/// Inherits the <see cref="MonoBehaviour"/> class and <see cref="IMod"/> inteface
/// </summary>
public abstract class ModBase : MonoBehaviour, IMod, ILogger, IEquatable<ModBase> {
    /// <summary>
    /// Instance for all mods
    /// </summary>
    public static readonly List<IMod> AllMods = [];

    #region MOD INFO

    /// <summary>
    /// This mods ModInfo instance.
    /// </summary>
    public abstract ModInfo Data { get; }

    /// <summary>
    /// Quick accessor for <see cref="ModInfo.Name"/>
    /// </summary>
    public string ModName => this.Data.Name;

    /// <summary>
    /// Quick accessor for <see cref="ModInfo.Guid"/>
    /// </summary>
    public string ModGuid => this.Data.Guid;

    /// <summary>
    /// Quick accessor for <see cref="ModInfo.Version"/>
    /// </summary>
    public Version ModVersion => this.Data.Version;

    /// <summary>
    /// Quick accessor for <see cref="ModInfo.ModVersionString"/>
    /// </summary>
    public string ModVersionString => this.ModVersionString;

    /// <summary>
    /// Quick accessor for <see cref="ModInfo.WorkshopId"/>
    /// </summary>
    public ulong ModWorkshopId => this.Data.WorkshopId;

    /// <summary>
    /// Quick accessor for <see cref="ModInfo.Incompatibilities"/>
    /// </summary>
    public List<ModIncompatibilityInfo> ModIncompatibilities => this.Data.Incompatibilities;

    /// <summary>
    /// Quick accessor for <see cref="ModInfo.HasIncompatibilities"/>
    /// </summary>
    public bool ModHasIncompatibilities => this.Data.HasIncompatibilities;

    /// <summary>
    /// Quick accessor for <see cref="ModInfo.Dependencies"/>
    /// </summary>
    public List<ModDependencyInfo> ModDependencies => this.Data.Dependencies;

    /// <summary>
    /// Quick accessor for <see cref="ModInfo.HasDependencies"/>
    /// </summary>
    public bool ModHasDependencies => this.Data.HasDependencies;

    /// <summary>
    /// Quick accessor for <see cref="ModInfo.GameType"/>
    /// </summary>
    public GameType ModGameType => this.Data.GameType;

    #endregion // MOD INFO

    #region METHODS

    /// <inheritdoc/>
    public abstract void OnLoaded(List<GameObject> prefabs, List<Assembly> assemblies, ConfigFile config, ModData data);

    /// <inheritdoc/>
    public abstract void OnUnloaded();

    /// <summary>
    /// Internal <see cref="SceneLoaded"/> method.
    /// Called when a scene is loaded.
    /// </summary>
    /// <param name="scene">The scene being loaded</param>
    /// <param name="loadSceneMode">The scenes loading mode</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    protected void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode) {
        SceneLoadArgs args = new SceneLoadArgs(scene, loadSceneMode);

        UniTask task = scene.name switch {
            Constants.SPLASH_SCENE_NAME => this.OnSplashLoaded(args),
            Constants.BASE_SCENE_NAME => this.OnBaseLoaded(args),
            Constants.CHARACTER_CUSTOMIZATION_SCENE_NAME => this.OnCharacterCustomizationLoaded(args),
            _ => throw new ArgumentOutOfRangeException($"Unknown Scene Loaded, name: {scene.name}"),
        };

        this.OnSceneLoaded(args).Forget();
        task.Forget();
    }

    /// <summary>
    /// Internal <see cref="SceneUnloaded"/> method,
    /// Called when a scene is unloaded.
    /// </summary>
    /// <param name="scene">The scene being unloaded</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    protected void SceneUnloaded(Scene scene) {
        SceneLoadArgs args = new SceneLoadArgs(scene);

        UniTask task = scene.name switch {
            Constants.SPLASH_SCENE_NAME => this.OnSplashUnloaded(args),
            Constants.BASE_SCENE_NAME => this.OnBaseUnloaded(args),
            Constants.CHARACTER_CUSTOMIZATION_SCENE_NAME => this.OnCharacterCustomizationUnloaded(args),
            _ => throw new ArgumentOutOfRangeException($"Unknown Scene Unloaded, name: {scene.name}"),
        };

        this.OnSceneLoaded(args).Forget();
        task.Forget();
    }

    /// <summary>
    /// Internal <see cref="MenuPageEnabled"/> method.
    /// Called when a main menu page is enabled.
    /// </summary>
    /// <param name="page">The oage being enabled</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    protected void MenuPageEnabled(string page) {
        MenuPageEnabledArgs args = new MenuPageEnabledArgs(page);
        UniTask task = page switch {
            Constants.MAIN_MENU_PAGE => this.OnMainMenuPageEnabled(args),
            Constants.NEW_GAME_PAGE => this.OnNewGamePageEnabled(args),
            Constants.LOAD_GAME_PAGE => this.OnLoadGamePageEnabled(args),
            Constants.DIFFICULTY_SELECTION_PAGE => this.OnDifficultySelectionPageEnabled(args),
            Constants.STARTING_CONDITIONS_PAGE => this.OnStartingConditionsPageEnabled(args),
            Constants.TUTORIALS_PAGE => this.OnTutorialsPageEnabled(args),
            Constants.WORKSHOP_PAGE => this.OnWorkshopPageEnabled(args),
            Constants.SETTINGS_PAGE => this.OnSettingsPageEnabled(args),
            _ => throw new ArgumentOutOfRangeException($"Unknown Page Enabled, name: {page}"),
        };

        this.OnMenuPageEnabled(args).Forget();
        task.Forget();
    }

    /// <summary>
    /// Note: this only compares mod info, as its the most significant.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public override bool Equals(object other) => other is ModBase mod && this.Equals(mod);

    /// <summary>
    /// Note: this only compares mod info, as its the most significant.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(ModBase mod) => this.Data.Equals(mod?.Data);

    /// <summary>
    /// Returns identifier to distinguish mod.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => this.Data.ToString();

    /// <summary>
    /// Hash code for this mod.
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode() => this.Data.GetHashCode();

    #endregion

    #region EVENT METHODS

    /// <inheritdoc/>
    public virtual void OnAwake() { }

    /// <inheritdoc/>
    public virtual void OnUpdate(float deltaTime) { }

    /// <inheritdoc/>
    public virtual void OnLateUpdate(float deltaTime) { }

    /// <inheritdoc/>
    public virtual void OnFixedUpdate(float deltaTime) { }

    /// <inheritdoc/>
    public virtual void OnGUI() { }

    /// <inheritdoc/>
    public virtual void OnEnable() { }

    /// <inheritdoc/>
    public virtual void OnDisable() { }

    /// <inheritdoc/>
    public virtual void OnDestroy() { }

    /// <inheritdoc/>
    public virtual void OnApplicationFocus(bool hasFocus) { }

    /// <inheritdoc/>
    public virtual void OnApplicationPause(bool pauseStatus) { }

    /// <inheritdoc/>
    public virtual void OnApplicationQuit() { }

    #endregion // EVENT METHODS

    #region UNITASK METHODS

    /// <inheritdoc/>
    public virtual UniTask OnSceneLoaded(SceneLoadArgs args) => UniTask.CompletedTask;

    /// <inheritdoc/>
    public virtual UniTask OnSplashLoaded(SceneLoadArgs args) => UniTask.CompletedTask;

    /// <inheritdoc/>
    public virtual UniTask OnBaseLoaded(SceneLoadArgs args) => UniTask.CompletedTask;

    /// <inheritdoc/>
    public virtual UniTask OnCharacterCustomizationLoaded(SceneLoadArgs args) => UniTask.CompletedTask;

    /// <inheritdoc/>
    public virtual UniTask OnSceneUnloaded(SceneLoadArgs args) => UniTask.CompletedTask;

    /// <inheritdoc/>
    public virtual UniTask OnSplashUnloaded(SceneLoadArgs args) => UniTask.CompletedTask;

    /// <inheritdoc/>
    public virtual UniTask OnBaseUnloaded(SceneLoadArgs args) => UniTask.CompletedTask;

    /// <inheritdoc/>
    public virtual UniTask OnCharacterCustomizationUnloaded(SceneLoadArgs args) => UniTask.CompletedTask;

    /// <inheritdoc/>
    public virtual UniTask OnMenuPageEnabled(MenuPageEnabledArgs args) => UniTask.CompletedTask;

    /// <inheritdoc/>
    public virtual UniTask OnMainMenuPageEnabled(MenuPageEnabledArgs args) => UniTask.CompletedTask;

    /// <inheritdoc/>
    public virtual UniTask OnNewGamePageEnabled(MenuPageEnabledArgs args) => UniTask.CompletedTask;

    /// <inheritdoc/>
    public virtual UniTask OnLoadGamePageEnabled(MenuPageEnabledArgs args) => UniTask.CompletedTask;

    /// <inheritdoc/>
    public virtual UniTask OnDifficultySelectionPageEnabled(MenuPageEnabledArgs args) => UniTask.CompletedTask;

    /// <inheritdoc/>
    public virtual UniTask OnStartingConditionsPageEnabled(MenuPageEnabledArgs args) => UniTask.CompletedTask;

    /// <inheritdoc/>
    public virtual UniTask OnTutorialsPageEnabled(MenuPageEnabledArgs args) => UniTask.CompletedTask;

    /// <inheritdoc/>
    public virtual UniTask OnWorkshopPageEnabled(MenuPageEnabledArgs args) => UniTask.CompletedTask;

    /// <inheritdoc/>
    public virtual UniTask OnSettingsPageEnabled(MenuPageEnabledArgs args) => UniTask.CompletedTask;

    #endregion // UNITASK METHODS

    #region LOGGING METHODS

    /// <summary>
    /// Log function that is redirected to <see cref="Logger"/>
    /// Can be overriden to add or remove functionality.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="severity"></param>
    public virtual void Log(string message, LogSeverity severity = LogSeverity.Information) { }

    /// <summary>
    /// Log function that is redirected to <see cref="Logger"/>
    /// Can be overriden to add or remove functionality.
    /// </summary>
    /// <param name="exception">Exception</param>
    public virtual void Log(Exception exception) { }

    /// <summary>
    /// Shortcut function <see cref="Log"/> to log a <see cref="LogSeverity.Debug"/> severity message.
    /// Does not log anything if not in debug mode.
    /// </summary>
    /// <param name="message">string</param>
    public void LogDebug(string message) => this.Log(message, LogSeverity.Debug);

    /// <summary>
    /// Shortcut function <see cref="Log"/> to log a <see cref="LogSeverity.Information"/> severity message.
    /// </summary>
    /// <param name="message">string</param>
    public void LogInfo(string message) => this.Log(message, LogSeverity.Information);

    /// <summary>
    /// Shortcut function <see cref="Log"/> to log a <see cref="LogSeverity.Warning"/> severity message.
    /// </summary>
    /// <param name="message">string</param>
    public void LogWarning(string message) => this.Log(message, LogSeverity.Warning);

    /// <summary>
    /// Shortcut function <see cref="Log"/> to log a <see cref="LogSeverity.Error"/> severity message.
    /// </summary>
    /// <param name="message">string</param>
    public void LogError(string message) => this.Log(message, LogSeverity.Error);

    /// <summary>
    /// Shortcut function <see cref="Log"/> to log a <see cref="Exception"/>
    /// </summary>
    /// <param name="message">string</param>
    public void LogException(Exception exception) => this.Log(exception);

    /// <summary>
    /// Shortcut function <see cref="Log"/> to log a <see cref="LogSeverity.Fatal"/> severity message.
    /// </summary>
    /// <param name="message">string</param>
    public void LogFatal(string message) => this.Log(message, LogSeverity.Fatal);

    /// <summary>
    /// Log function that is redirected to <see cref="Logger"/>
    /// Can be overriden to add or remove functionality.
    /// </summary>
    /// <param name="severity">LogSeverity</param>
    /// <param name="format">string</param>
    /// <param name="args">params object[]</param>
    public virtual void LogFormat(LogSeverity severity, string format, params object[] args) { }

    /// <summary>
    /// Shortcut function <see cref="LogFormat"/> to log a formatted <see cref="LogSeverity.Debug"/> severity message.
    /// Does not log anything if not in debug mode.
    /// </summary>
    /// <param name="message">string</param>
    public void LogDebugFormat(string message, params object[] args) => this.LogFormat(LogSeverity.Debug, message, args);

    /// <summary>
    /// Shortcut function <see cref="LogFormat"/> to log a formatted <see cref="LogSeverity.Information"/> severity message.
    /// </summary>
    /// <param name="message">string</param>
    public void LogInfoFormat(string message, params object[] args) => this.LogFormat(LogSeverity.Information, message, args);

    /// <summary>
    /// Shortcut function <see cref="LogFormat"/> to log a formatted <see cref="LogSeverity.Warning"/> severity message.
    /// </summary>
    /// <param name="message">string</param>
    public void LogWarningFormat(string message, params object[] args) => this.LogFormat(LogSeverity.Warning, message, args);

    /// <summary>
    /// Shortcut function <see cref="LogFormat"/> to log a formatted <see cref="LogSeverity.Exception"/> severity message.
    /// </summary>
    /// <param name="message">string</param>
    public void LogErrorFormat(string message, params object[] args) => this.LogFormat(LogSeverity.Error, message, args);

    /// <summary>
    /// Shortcut function <see cref="LogFormat"/> to log a formatted <see cref="LogSeverity.Fatal"/> severity message.
    /// </summary>
    /// <param name="message">string</param>
    public void LogFatalFormat(string message, params object[] args) => this.LogFormat(LogSeverity.Fatal, message, args);

    #endregion // LOGGING METHODS
}
