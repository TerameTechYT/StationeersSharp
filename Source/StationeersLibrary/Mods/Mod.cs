#region

using StationeersLaunchPad;
using LaunchPadBooster;
using LaunchPadBooster.Networking;
using InternalMod = LaunchPadBooster.Mod;
using Logger = StationeersLaunchPad.Logger;
using MoonSharp.Interpreter.CoreLib;
using System.Diagnostics;

#endregion

namespace StationeersLibrary.Mods;

/// <summary>
/// Main class for handling modding with <see cref="StationeersLaunchPad"/>
/// Inherits the <see cref="MonoBehaviour"/> class
/// </summary>
public abstract class Mod : MonoBehaviour {
    /// <summary>
    /// Should this mod initalize the logger?
    /// </summary>
    public virtual bool UseLogger { get; protected set; } = true;

    /// <summary>
    /// This mods <see cref="StationeersLaunchPad.Logger"/> instance
    /// </summary>
    public Logger Logger { get; private set; }

    /// <summary>
    /// Should this mod initalize the config?
    /// </summary>
    public abstract bool UseConfig { get; }

    /// <summary>
    /// Should this mod automatically save configuration changes?
    /// </summary>
    public virtual bool AutoSaveConfig { get; protected set; } = true;

    /// <summary>
    /// Should this mod save configuration on creation?
    /// </summary>
    public virtual bool SaveConfigOnCreate { get; protected set; } = true;

    /// <summary>
    /// This mods <see cref="ConfigFile"/> instance.
    /// </summary>
    public ConfigFile Config { get; private set; }

    /// <summary>
    /// Should this mod initalize harmony?
    /// </summary>
    public abstract bool UseHarmony { get; }

    /// <summary>
    /// Should this mod automatically patch with harmony?
    /// </summary>
    public virtual bool AutoPatch { get; protected set; } = true;

    /// <summary>
    /// This mods <see cref="HarmonyLib.Harmony"/> instance.
    /// </summary>
    public Harmony Harmony { get; private set; }

    /// <summary>
    /// Override to provide your own version checking function.
    /// <see cref="https://github.com/StationeersLaunchPad/LaunchPadBooster/blob/master/README.md#multiplayer"/>
    /// </summary>
    public virtual Func<string, bool> VersionCheck { get; private set; }

    /// <summary>
    /// Internal <see cref="LaunchPadBooster.Mod"/> instance.
    /// </summary>
    internal InternalMod InternalMod { get; private set; }

    /// <summary>
    /// Internal <see cref="StationeersLaunchPad.LoadedMod"/> instance.
    /// </summary>
    internal LoadedMod LoadedMod { get; private set; }

    /// <summary>
    /// This mods list of prefabs.
    /// </summary>
    public readonly List<GameObject> Prefabs = [];

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
    /// Quick accessor for <see cref="ModInfo.GameType"/>
    /// </summary>
    public GameType ModGameType => this.Data.GameType;

    public Mod() {
        LoadedMod mod = null;
        ModLoader.TryGetStackTraceMod(new StackTrace(2), out mod);

        if (mod != null) {
            this.LoadedMod = mod;
            UnityEngine.Debug.LogError("could not get loadedmod");
        }
    }

    /// <summary>
    /// Internal <see cref="Awake"/> method.
    /// Called when <see cref="MonoBehaviour"/> is initalized
    /// </summary>
    /// <exception cref="AlreadyLoadedException"></exception>
    /// <exception cref="IncompatableGameTypeException"></exception>
    private void Awake() {
        if (Utilities.IsLoaded(this.ModGuid)) {
            throw new AlreadyLoadedException(this.Data);
        }

        if (!this.Data.IsGameCompatible()) {
            throw new IncompatableGameTypeException(Constants.GameType, this.ModGameType);
        }

        this.InternalMod = new InternalMod(this.ModGuid, this.ModVersionString);
        this.InternalMod.AddPrefabs(this.Prefabs.AsReadOnly());
        this.InternalMod.SetVersionCheck(this.VersionCheck);
        if (this.ModGameType == GameType.Both) {
            this.InternalMod.SetMultiplayerRequired();
        }

        if (this.UseLogger) {
            this.Logger = Logger.Global.CreateChild(this.ModName);
        }

        if (this.UseConfig) {
            this.Config = new ConfigFile(Path.Combine(Constants.BIE_CONFIG_FOLDER, this.ModGuid), this.SaveConfigOnCreate);
            this.Config.SaveOnConfigSet = this.AutoSaveConfig;
            this.Config.SettingChanged += this.ConfigChanged;
            this.Config.ConfigReloaded += this.ConfigReloaded;

            this.DoLoadConfiguration();
        }

        if (this.UseHarmony) {
            this.Harmony = new Harmony(this.ModGuid);

            this.DoHarmonyPatch();
        }

        SceneManager.sceneLoaded += this.SceneLoaded;
        SceneManager.sceneUnloaded += this.SceneUnloaded;
        MainMenuWindowManager.OnPageEnabled += this.MenuPageEnabled;

        this.OnAwake();
    }

    /// <summary>
    /// Called by <see cref="StationeersLaunchPad"/> with configuration and any prefabs.
    /// </summary>
    /// <param name="prefabs"></param>
    public void OnLoaded(List<GameObject> prefabs) {
        this.Prefabs.AddRange(prefabs ?? []);
    }

    /// <summary>
    /// Internal <see cref="ConfigFile.SettingChanged"/> event connection.
    /// Called when user makes any change to configuration file.
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">SettingChangedEventArgs</param>
    private void ConfigChanged(object sender, SettingChangedEventArgs e) => this.OnConfigChanged(sender as ConfigEntryBase, e);

    /// <summary>
    /// Internal <see cref="ConfigFile.ConfigReloaded"/> event connection.
    /// Called when configuration file is reloaded.
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">EventArgs</param>
    private void ConfigReloaded(object sender, EventArgs e) => this.OnConfigReloaded();

    /// <summary>
    /// Internal <see cref="Awake"/> method.
    /// Called every frame before <see cref="LateUpdate"/>
    /// </summary>
    private void Update() => this.OnUpdate(Time.deltaTime);

    /// <summary>
    /// Internal <see cref="LateUpdate"/> method.
    /// Called after <see cref="Update"/>
    /// </summary>
    private void LateUpdate() => this.OnLateUpdate(Time.deltaTime);

    /// <summary>
    /// Internal <see cref="FixedUpdate"/> method.
    /// Called on a fixed framerate frames.
    /// </summary>
    private void FixedUpdate() => this.OnLateUpdate(Time.fixedDeltaTime);

    /// <summary>
    /// Internal <see cref="DoLoadConfiguration"/> method.
    /// Called when mod is ready to load configuration
    /// </summary>
    private void DoLoadConfiguration() {
        this.LogDebug("Loading configuration...");

        this.OnLoadConfiguration();

        this.LogDebug("Loaded configuration!");
    }

    /// <summary>
    /// Internal <see cref="DoHarmonyPatch"/>
    /// Called when mod is ready to do patches.
    /// </summary>
    private void DoHarmonyPatch() {
        bool success = true;
        try {
            if (this.AutoPatch) {
                this.LogDebug("Harmony patching starting...");
                this.Harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
        }
        catch (Exception ex) {
            this.LogException(ex);
            this.LogError("Failed to patch harmony!");
            success = false;
        }
        finally {
            this.LogDebug("Harmony patching finished!");
            this.OnHarmonyPatched(success);
        }
    }

    public PrefabSetup<T> RegisterPrefab<T>(string prefab = null) {
        return this.InternalMod.SetupPrefabs<T>(prefab);
    }

    public void RegisterSaveDataType<T>() => this.InternalMod.AddSaveDataType<T>();

    public void RegisterNetworkMessage<T>() where T : ModNetworkMessage<T>, new() => this.InternalMod.RegisterNetworkMessage<T>();

    /// <summary>
    /// Internal <see cref="Mod.SceneLoaded"/> method.
    /// Called when a scene is loaded.
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="loadSceneMode"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode) {
        SceneLoadArgs args = new SceneLoadArgs(scene, loadSceneMode);

        UniTask task = scene.name switch {
            Constants.SPLASH_SCENE_NAME => this.OnSplashLoaded(args),
            Constants.BASE_SCENE_NAME => this.OnBaseLoaded(args),
            Constants.CHARACTER_CUSTOMIZATION_SCENE_NAME => this.OnCharacterCustomizationLoaded(args),
            _ => throw new NotImplementedException($"Unknown Scene Loaded, name: {scene.name}"),
        };

        this.OnSceneLoaded(args).Forget();
        task.Forget();
    }

    /// <summary>
    /// Internal <see cref="Mod.SceneUnloaded"/> method,
    /// Called when a scene is unloaded.
    /// </summary>
    /// <param name="scene"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void SceneUnloaded(Scene scene) {
        SceneLoadArgs args = new SceneLoadArgs(scene);

        UniTask task = scene.name switch {
            Constants.SPLASH_SCENE_NAME => this.OnSplashUnloaded(args),
            Constants.BASE_SCENE_NAME => this.OnBaseUnloaded(args),
            Constants.CHARACTER_CUSTOMIZATION_SCENE_NAME => this.OnCharacterCustomizationUnloaded(args),
            _ => throw new NotImplementedException($"Unknown Scene Unloaded, name: {scene.name}"),
        };

        this.OnSceneLoaded(args).Forget();
        task.Forget();
    }

    /// <summary>
    /// Internal <see cref="Mod.MenuPageEnabled"/> method.
    /// Called when a main menu page is enabled.
    /// </summary>
    /// <param name="page"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void MenuPageEnabled(string page) {
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
            _ => throw new NotImplementedException($"Unknown Page Enabled, name: {page}"),
        };

        this.OnMenuPageEnabled(args).Forget();
        task.Forget();
    }

    /// <summary>
    /// Called by <see cref="Mod"/> after core initialization is done.
    /// </summary>
    public abstract void OnAwake();

    /// <summary>
    /// Called by <see cref="Mod"/> when configuration is ready to be binded.
    /// Implement your configurations here.
    /// </summary>
    public virtual void OnLoadConfiguration() { }

    /// <summary>
    /// Called by <see cref="Mod"/> when any configuration has been changed.
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="args"></param>
    public virtual void OnConfigChanged(ConfigEntryBase entry, SettingChangedEventArgs args) { }

    /// <summary>
    /// Called by <see cref="Mod"/> when the configuration is reloaded.
    /// </summary>
    public virtual void OnConfigReloaded() { }

    /// <summary>
    /// Called by <see cref="Mod"/> every frame.
    /// </summary>
    public virtual void OnUpdate(float deltaTime) { }

    /// <summary>
    /// Called by <see cref="Mod"/> after <see cref="OnUpdate"/>
    /// </summary>
    public virtual void OnLateUpdate(float deltaTime) {}

    /// <summary>
    /// Called by <see cref="Mod"/> on a fixed framerate.
    /// </summary>
    public virtual void OnFixedUpdate(float deltaTime) {}

    /// <summary>
    /// Called by Unity for handling UGUI events.
    /// </summary>
    public virtual void OnGUI() { }

    /// <summary>
    /// Called when the <see cref="GameObject"/> with this <see cref="Mod"/> attached is enabled.
    /// </summary>
    public virtual void OnEnable() { }

    /// <summary>
    /// Called when the <see cref="GameObject"/> with this <see cref="Mod"/> attached is disabled.
    /// </summary>
    public virtual void OnDisable() { }

    /// <summary>
    /// Called when the <see cref="GameObject"/> with this <see cref="Mod"/> attached is about to be destroyed.
    /// </summary>
    public virtual void OnDestroy() { }

    /// <summary>
    /// Called by <see cref="Mod"/> when harmony patches are completed.
    /// If <seealso cref="AutoPatch"/> is false, implement your patching logic here.
    /// </summary>
    /// <param name="success"></param>
    public virtual void OnHarmonyPatched(bool success) { }

    /// <summary>
    /// Called when any scene is loaded
    /// </summary>
    /// <param name="args">SceneLoadArgs</param>
    public virtual UniTask OnSceneLoaded(SceneLoadArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when the inital game loading scene is loaded. 
    /// This is unlikely to ever be called.
    /// </summary>
    /// <param name="args">SceneLoadArgs</param>
    public virtual UniTask OnSplashLoaded(SceneLoadArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when the base scene is loaded.
    /// </summary>
    /// <param name="args"></param>
    /// <returns>SceneLoadArgs</returns>
    public virtual UniTask OnBaseLoaded(SceneLoadArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when the character customization screen is loaded.
    /// </summary>
    /// <param name="args">SceneLoadArgs</param>
    public virtual UniTask OnCharacterCustomizationLoaded(SceneLoadArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when any scene is unloaded.
    /// </summary>
    /// <param name="args">SceneLoadArgs</param>
    public virtual UniTask OnSceneUnloaded(SceneLoadArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when inital game loading scene is unloaded.
    /// </summary>
    /// <param name="args">SceneLoadArgs</param>
    public virtual UniTask OnSplashUnloaded(SceneLoadArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when the base scene is unloaded.
    /// </summary>
    /// <param name="args">SceneLoadArgs</param>
    public virtual UniTask OnBaseUnloaded(SceneLoadArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when the character customization screen is unloaded.
    /// </summary>
    /// <param name="args">SceneLoadArgs</param>
    public virtual UniTask OnCharacterCustomizationUnloaded(SceneLoadArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when any main menu page is enabled.
    /// </summary>
    /// <param name="args">MenuPageEnabledArgs</param>
    public virtual UniTask OnMenuPageEnabled(MenuPageEnabledArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when the main menu is enabled.
    /// </summary>
    /// <param name="args">MenuPageEnabledArgs</param>
    public virtual UniTask OnMainMenuPageEnabled(MenuPageEnabledArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when the new game menu is enabled.
    /// </summary>
    /// <param name="args">MenuPageEnabledArgs</param>
    public virtual UniTask OnNewGamePageEnabled(MenuPageEnabledArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when the load game menu is enabled.
    /// </summary>
    /// <param name="args">MenuPageEnabledArgs</param>
    public virtual UniTask OnLoadGamePageEnabled(MenuPageEnabledArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when the difficulty selection menu is enabled.
    /// </summary>
    /// <param name="args">MenuPageEnabledArgs</param>
    public virtual UniTask OnDifficultySelectionPageEnabled(MenuPageEnabledArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when the start conditions menu is enabled.
    /// </summary>
    /// <param name="args">MenuPageEnabledArgs</param>
    public virtual UniTask OnStartingConditionsPageEnabled(MenuPageEnabledArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when the tutorials menu is enabled.
    /// </summary>
    /// <param name="args">MenuPageEnabledArgs</param>
    public virtual UniTask OnTutorialsPageEnabled(MenuPageEnabledArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when the workshop mods menu is enabled.
    /// </summary>
    /// <param name="args">MenuPageEnabledArgs</param>
    public virtual UniTask OnWorkshopPageEnabled(MenuPageEnabledArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Called when the settings menu is enabled.
    /// </summary>
    /// <param name="args">MenuPageEnabledArgs</param>
    public virtual UniTask OnSettingsPageEnabled(MenuPageEnabledArgs args) => UniTask.CompletedTask;

    /// <summary>
    /// Log function that is redirected to <see cref="Logger"/>
    /// Can be overriden to add or remove functionality.
    /// </summary>
    /// <param name="message">string</param>
    /// <param name="severity">LogSeverity</param>
    public virtual void Log(string message, LogSeverity severity = LogSeverity.Information) {
        if (!this.UseLogger) {
            return;
        }

        this.Logger?.Log(message, severity);
    }

    /// <summary>
    /// Log function that is redirected to <see cref="Logger"/>
    /// Can be overriden to add or remove functionality.
    /// </summary>
    /// <param name="exception">Exception</param>
    public virtual void Log(Exception exception) {
        if (!this.UseLogger) {
            return;
        }

        this.Logger?.Log(exception);
    }

    /// <summary>
    /// Shortcut function <see cref="Log"/> to log a <see cref="LogSeverity.Debug"/> severity message.
    /// Does not log anything if not in debug mode.
    /// </summary>
    /// <param name="message">string</param>
    public void LogDebug(string message) {
#if DEBUG
        this.Log(message, LogSeverity.Debug);
#endif
    }

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
    public virtual void LogFormat(LogSeverity severity, string format, params object[] args) {
        if (!this.UseLogger) {
            return;
        }

        this.Logger?.LogFormat(true, severity, format, args);
    }

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
}