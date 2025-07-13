#region

using LaunchPadBooster;
using LaunchPadBooster.Networking;
using StationeersLaunchPad;
using System.Diagnostics;
using Logger = StationeersLaunchPad.Logger;

#endregion

namespace StationeersLibrary.Mods;

/// <summary>
/// Main class for handling modding with <see cref="StationeersLaunchPad"/>
/// Inherits the <see cref="ModBase"/> class
/// </summary>
public abstract class Mod : ModBase {
    /// <summary>
    /// Locker object.
    /// </summary>
    private static readonly object _lock = new();

    #region LOGGER

    /// <summary>
    /// Should this mod initalize the logger?
    /// </summary>
    public abstract bool UseLogger { get; }

    /// <summary>
    /// This mods <see cref="StationeersLaunchPad.Logger"/> instance
    /// </summary>
    public Logger Logger { get; private set; }

    /// <summary>
    /// This mods <see cref="StationeersLaunchPad.LogBuffer"/> instance
    /// </summary>
    protected LogBuffer Buffer => this.Logger.Buffer;

    /// <summary>
    /// This mods <see cref="Logger"/> name
    /// </summary>
    public string LoggerName => this.Logger.Name;

    #endregion // LOGGER

    #region CONFIG

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

    #endregion // CONFIG

    #region HARMONY
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

    #endregion // HARMONY

    #region INTERNAL

    /// <summary>
    /// Override to provide your own version checking function.
    /// </summary>
    public virtual Func<string, bool> VersionCheck { get; private set; }

    /// <summary>
    /// Internal <see cref="LaunchPadBooster.Mod"/> instance.
    /// </summary>
    internal LaunchPadBooster.Mod InternalMod { get; private set; }

    /// <summary>
    /// Internal <see cref="StationeersLaunchPad.LoadedMod"/> instance.
    /// </summary>
    internal LoadedMod LoadedMod { get; private set; }

    internal Logger LoadedLogger => this.LoadedMod.Logger;

    internal LogBuffer LoadedBuffer => this.LoadedLogger.Buffer;

    #endregion // INTERNAL

    #region MOD INFO

    /// <summary>
    /// This mods list of prefabs.
    /// </summary>
    public readonly List<GameObject> Prefabs = [];

    /// <summary>
    /// True if this mod has at least 1 prefab.
    /// </summary>
    public bool HasPrefabs => this.Prefabs.Count > 0;

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

    #endregion // MOD INFO

    #region INTERNAL METHODS

    /// <summary>
    /// Default constructor
    /// </summary>
    protected Mod() {
        // Fetches the caller of this constructor, which should be the class that inherits this one.
        if (ModLoader.TryGetStackTraceMod(new StackTrace(1), out LoadedMod mod)) {
            this.LoadedMod = mod;

            if (this.UseLogger) {
                this.Logger = Logger.Global.CreateChild(this.ModName);
                this.DoLoggerMove();
            }
        }
        else {
            Logger.Global.LogError($"Could not get LoadedMod for {this}");
            LaunchPadConfig.AutoLoad = false;
        }
    }

    /// <inheritdoc/>
    public override void OnLoaded(List<GameObject> prefabs) {
        this.Log($"{this} is now loading...");

        if (this.UseLogger && this.Logger == null) {
            this.Logger = Logger.Global.CreateChild(this.ModName);
        }

        if (Utilities.IsLoaded(this.ModGuid)) {
            this.LogError($"{this} has already been loaded!");
            LaunchPadConfig.AutoLoad = false;
            return;
        }

        if (!this.Data.IsGameCompatible()) {
            this.LogError($"{this} cannot be run on {this.ModGameType}, requires {Constants.GameType}");
            LaunchPadConfig.AutoLoad = false;
            return;
        }

        if (this.UseConfig) {
            string path = Path.Combine(Constants.BIE_CONFIG_FOLDER, this.ModGuid);
            if (File.Exists(path)) {
                // made oopsie, forgot to add .cfg to the config file path...
                File.Delete(path);
            }

            this.Config = new ConfigFile($"{path}.cfg", this.SaveConfigOnCreate) {
                SaveOnConfigSet = this.AutoSaveConfig
            };
            this.Config.SettingChanged += this.ConfigChanged;
            this.Config.ConfigReloaded += this.ConfigReloaded;
            this.LoadedMod?.ConfigFiles?.Add(this.Config);

            this.DoLoadConfiguration();
        }

        if (this.UseHarmony) {
            this.Harmony = new Harmony(this.ModGuid);

            this.DoHarmonyPatch();
        }

        this.Prefabs.AddRange(prefabs ?? []);

        this.InternalMod = new LaunchPadBooster.Mod(this.Data.Guid, this.Data.Version.ToString());
        if (this.HasPrefabs) {
            this.InternalMod.AddPrefabs(this.Prefabs.AsReadOnly());
        }
        if (this.VersionCheck != null) {
            this.InternalMod.SetVersionCheck(this.VersionCheck);
        }
        if (this.ModGameType == GameType.Both) {
            this.InternalMod.SetMultiplayerRequired();
        }

        SceneManager.sceneLoaded += this.SceneLoaded;
        SceneManager.sceneUnloaded += this.SceneUnloaded;
        MainMenuWindowManager.OnPageEnabled += this.MenuPageEnabled;

        lock (_lock) {
            ModBase.AllMods.Add(this);
        }

        this.LogDebug($"{this} is now loaded!");

        this.OnStart();
    }

    /// <inheritdoc/>
    public override void OnUnloaded() {
        this.Log($"{this} is now unloading...");

        if (this.UseConfig) {
            this.Config.Save();
        }

        if (this.UseHarmony) {
            this.UndoHarmonyPatch();
        }

        if (this.HasPrefabs) {
            this.Prefabs.Clear();
        }

        SceneManager.sceneLoaded -= this.SceneLoaded;
        SceneManager.sceneUnloaded -= this.SceneUnloaded;
        MainMenuWindowManager.OnPageEnabled -= this.MenuPageEnabled;

        lock (_lock) {
            ModBase.AllMods.Remove(this);
        }

        this.LogDebug($"{this} is now unloaded!");
    }

    /// <summary>
    /// Internal <see cref="ConfigFile.SettingChanged"/> event connection.
    /// Called when user makes any change to configuration file.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ConfigChanged(object sender, SettingChangedEventArgs e) => this.OnConfigChanged(sender as ConfigEntryBase, e);

    /// <summary>
    /// Internal <see cref="ConfigFile.ConfigReloaded"/> event connection.
    /// Called when configuration file is reloaded.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ConfigReloaded(object sender, EventArgs e) => this.OnConfigReloaded();

    /// <summary>
    /// Internal <see cref="Awake"/> method.
    /// Called when <see cref="MonoBehaviour"/> is initalized
    /// </summary>
    private void Awake() => this.OnStart();

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
    /// Internal logger move function, moves the contents of SLP's logger into ours
    /// </summary>
    private void DoLoggerMove() {
        // i know this is jank, but we dont really want to have 2 logger instances for the same mod

        for (int i = 0; i < this.LoadedBuffer.Count; i++) {
            LogLine line = this.LoadedBuffer[i];
            this.Buffer.Add(this.LoggerName, line.Message, line.Severity);
        }
        this.LoadedMod.Logger.Clear();
        this.LoadedMod.Logger = this.Logger;
    }

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
        if (this.AutoPatch) {
            this.LogDebug("Harmony patching starting...");
            try {
                foreach ((Assembly assembly, List<PatchClassProcessor> processors) in this.Harmony.CreatePatchersForAssemblies(this.LoadedMod.Assemblies)) {
                    AssemblyName name = assembly.GetName();
                    this.LogDebug($"Harmony patching assembly ({name.FullName})");

                    int patches = 0;
                    foreach (PatchClassProcessor processor in processors) {
                        List<MethodInfo> methods = processor.Patch();

#if DEBUG
                        foreach (MethodInfo method in methods) {
                            this.LogDebug($"Harmony patched method {method.FullDescription()}");
                        }
#endif

                        patches += methods?.Count ?? 0;
                    }

                    this.LogDebug($"Harmony finished patching assembly ({name.Name}) with {patches} patches");
                }
            }
            catch (Exception ex) {
                this.LogError("Harmony patch failed!");
                this.LogException(ex);
                LaunchPadConfig.AutoLoad = success = false;
            }
            finally {
                this.LogDebug("Harmony patching finished...");
            }
        }
        this.OnHarmonyPatched(success);
    }

    /// <summary>
    /// Internal <see cref="UndoHarmonyPatch"/>
    /// Called when mod is ready to remove patches.
    /// </summary>
    private void UndoHarmonyPatch() {
        this.Harmony.UnpatchSelf();

        this.OnHarmonyUnpatched();
    }

    #endregion // INTERNAL METHODS

    #region EVENT METHODS

    /// <summary>
    /// Called by <see cref="IMod"/> after core initialization is done.
    /// </summary>
    public abstract void OnStart();

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

    /// <inheritdoc/>
    public virtual void OnConfigReloaded() { }

    /// <summary>
    /// Called by <see cref="Mod"/> when harmony patches are completed.
    /// If <seealso cref="AutoPatch"/> is false, implement your patching logic here.
    /// </summary>
    /// <param name="success"></param>
    public virtual void OnHarmonyPatched(bool success) { }

    /// <summary>
    /// Called by <see cref="Mod"/> when harmony patches are completed.
    /// If <seealso cref="AutoPatch"/> is false, implement your unpatching logic here.
    /// </summary>
    /// <param name="success"></param>
    public virtual void OnHarmonyUnpatched() { }

    #endregion // EVENT METHODS

    #region LOGGING METHODS

    /// <summary>
    /// Log function that is redirected to <see cref="Logger"/>
    /// Can be overriden to add or remove functionality.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="severity"></param>
    public override void Log(string message, LogSeverity severity = LogSeverity.Information) {
        if (!this.UseLogger) {
            return;
        }

        this.Logger?.Log(message, severity, false);
        this.LogStationeers(message, severity);
    }

    /// <summary>
    /// Log function that is redirected to <see cref="Logger"/>
    /// Can be overriden to add or remove functionality.
    /// </summary>
    /// <param name="exception">Exception</param>
    public override void Log(Exception exception) {
        if (!this.UseLogger) {
            return;
        }

        this.Logger?.Log(exception);
        ConsoleWindow.PrintError(exception);
    }

    /// <summary>
    /// Log function that logs to stationeers console.
    /// Can be overriden to add or remove functionality.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="severity"></param>
    public virtual void LogStationeers(string message, LogSeverity severity) {
        switch (severity) {
            default:
            case LogSeverity.Debug:
                ConsoleWindow.Print(message, ConsoleColor.Gray);
                break;
            case LogSeverity.Information:
                ConsoleWindow.Print(message);
                break;
            case LogSeverity.Warning:
                ConsoleWindow.PrintAction(message);
                break;
            case LogSeverity.Error:
            case LogSeverity.Exception:
            case LogSeverity.Fatal:
                ConsoleWindow.PrintError(message);
                break;
        }
    }

    /// <summary>
    /// Log function that is redirected to <see cref="Logger"/>
    /// Can be overriden to add or remove functionality.
    /// </summary>
    /// <param name="severity">LogSeverity</param>
    /// <param name="format">string</param>
    /// <param name="args">params object[]</param>
    public override void LogFormat(LogSeverity severity, string format, params object[] args) {
        if (!this.UseLogger) {
            return;
        }

        this.Logger?.LogFormat(false, severity, format, args);
        this.LogFormatStationeers(severity, format, args);
    }

    /// <summary>
    /// Log function that logs to stationeers console.
    /// Can be overriden to add or remove functionality.
    /// </summary>
    /// <param name="severity"></param>
    /// <param name="format"></param>
    /// <param name="args"></param>
    public virtual void LogFormatStationeers(LogSeverity severity, string format, params object[] args) {
        switch (severity) {
            default:
            case LogSeverity.Debug:
                ConsoleWindow.Print(string.Format(format, args), ConsoleColor.Gray);
                break;
            case LogSeverity.Information:
                ConsoleWindow.Print(string.Format(format, args));
                break;
            case LogSeverity.Warning:
                ConsoleWindow.PrintAction(string.Format(format, args));
                break;
            case LogSeverity.Error:
            case LogSeverity.Exception:
            case LogSeverity.Fatal:
                ConsoleWindow.PrintError(string.Format(format, args));
                break;
        }
    }

    #endregion // LOGGING METHODS

    #region METHODS

    /// <summary>
    /// Registers a prefab.
    /// </summary>
    /// <typeparam name="T">The script for your prefab.</typeparam>
    /// <param name="prefab">The name of your prefab.</param>
    /// <returns>Prefab setup object.</returns>
    public PrefabSetup<T>? RegisterPrefab<T>(string prefab) where T : Thing =>
        this.InternalMod?.SetupPrefabs<T>(prefab);

    /// <summary>
    /// Registers a savedata type.
    /// </summary>
    /// <typeparam name="T">A thing savedata type.</typeparam>
    public void RegisterSaveDataType<T>() where T : ThingSaveData =>
        this.InternalMod?.AddSaveDataType<T>();

    /// <summary>
    /// Registers a network message.
    /// </summary>
    /// <typeparam name="T">A network message type.</typeparam>
    public void RegisterNetworkMessage<T>() where T : ModNetworkMessage<T>, new() =>
        this.InternalMod?.RegisterNetworkMessage<T>();

    /// <summary>
    /// Register a new configuration value.
    /// </summary>
    /// <typeparam name="T">A primitive type, enum or similar.</typeparam>
    /// <param name="data">Config data</param>
    public ConfigEntry<T> RegisterConfig<T>(ConfigData<T> data) where T : unmanaged {
        this.LogDebug($"Registering config {data}");
        return this.Config.Bind<T>(data.Definition, data.DefaultValue, data.Description);
    }

    public ConfigEntry<T>? GetConfigEntry<T>(string section, string key) where T : unmanaged =>
        this.GetConfigEntry<T>(new(section, key));

    public ConfigEntry<T>? GetConfigEntry<T>(ConfigDefinition definition) where T : unmanaged =>
        this.Config.TryGetEntry<T>(definition, out ConfigEntry<T> entry) ? entry : null;

    public T? GetConfigValue<T>(string section, string key, T? defaultValue = null) where T : unmanaged =>
        this.GetConfigValue<T>(new(section, key), defaultValue);

    public T? GetConfigValue<T>(ConfigDefinition definition, T? defaultValue = null) where T : unmanaged =>
        this.GetConfigEntry<T>(definition)?.Value ?? defaultValue;

    public void SetConfigValue<T>(string section, string key, T value) where T : unmanaged =>
        this.SetConfigValue<T>(new(section, key), value);

    public void SetConfigValue<T>(ConfigDefinition definition, T value) where T : unmanaged =>
        this.GetConfigEntry<T>(definition)?.Value = value;

    /// <summary>
    /// Note: this only compares mod info, as its the most significant.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public override bool Equals(object other) => other is Mod mod && this.Equals(mod);

    /// <summary>
    /// Note: this only compares mod info, as its the most significant.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(Mod mod) => this.Data.Equals(mod?.Data);

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

    #endregion // PUBLIC METHODS
}

public struct ConfigData<T> where T : notnull {
    public T DefaultValue { get; set; }
    public ConfigDefinition Definition { get; set; }
    public ConfigDescription Description { get; set; }

    public ConfigData(T defaultValue, ConfigDefinition definition, ConfigDescription description) {
        this.DefaultValue = defaultValue;
        this.Definition = definition;
        this.Description = description;
    }

    public ConfigData(T defaultValue, string section, string key, string description, AcceptableValueBase? acceptableValues = null, params object[] tags) {
        this.DefaultValue = defaultValue;
        this.Definition = new(section, key);
        this.Description = new(description, acceptableValues, tags);
    }

    public override bool Equals(object obj)  =>
        obj is ConfigData<T> data && this.Equals(data);

    public bool Equals(ConfigData<T> data) =>
        this.DefaultValue.Equals(data.DefaultValue) &&
        this.Definition.Equals(data.Definition) &&
        this.Description.Equals(data.Description);

    public override int GetHashCode() => 
        HashCode.Combine(
            this.DefaultValue,
            this.Definition.Section,
            this.Definition.Key,
            this.Description.Description,
            this.Description.AcceptableValues,
            this.Description.Tags
        );

    public override string ToString() => $"[{this.Definition}] : {this.DefaultValue}";
}
