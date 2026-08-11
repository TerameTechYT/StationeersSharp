#region

using Assets.Scripts.Objects;
using BepInEx.Configuration;
using BepInEx.Logging;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using LaunchPadBooster.Networking;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

#endregion


namespace StationeersLibrary.Modding;

/// <summary>
/// Main class for handling modding with <see cref="StationeersLaunchPad"/>
/// Inherits the <see cref="ModBase"/> class
/// </summary>
public abstract class Mod<T> : ModBase, IModSingleton<T>, IEquatable<Mod<T>>, IEquatable<T> where T : ModBase {
    /// <summary>
    /// Locker object.
    /// </summary>
    private static readonly object _lock = new();

    #region MOD LOGGER

    /// <summary>
    /// Should this mod initalize the logger?
    /// </summary>
    public abstract bool UseLogger { get; }

    /// <summary>
    /// This mods <see cref="ManualLogSource"/> instance.
    /// </summary>
    public ManualLogSource? Logger { get; private set; }

    #endregion // LOGGER

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
    public abstract override ModInfo Data { get; }

    #endregion // MOD INFO

    #region MOD PROFILER

    public virtual bool UseProfiler { get; }
#if DEBUG
    = true;
#endif

    #endregion

    #region MOD CONFIG

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
    public ConfigFile? Config { get; private set; }

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
    public Harmony? Harmony { get; private set; }

    #endregion // HARMONY

    #region INTERNAL

    /// <summary>
    /// Override to provide your own version validator function.
    /// </summary>
    public virtual IVersionValidator? VersionValidator { get; private set; }

    /// <summary>
    /// Override to provide your own version join function.
    /// </summary>
    public virtual IJoinValidator? JoinValidator { get; private set; }

    /// <summary>
    /// Override to provide your own version join prefix serializer function.
    /// </summary>
    public virtual IJoinPrefixSerializer? JoinPrefixSerializer { get; private set; }

    /// <summary>
    /// Override to provide your own version join suffix serializer function.
    /// </summary>
    public virtual IJoinSuffixSerializer? JoinSuffixSerializer { get; private set; }

    /// <summary>
    /// Override to provide your own version update prefix serializer function.
    /// </summary>
    public virtual IUpdatePrefixSerializer? UpdatePrefixSerializer { get; private set; }

    /// <summary>
    /// Override to provide your own version update suffix serializer function.
    /// </summary>
    public virtual IUpdateSuffixSerializer? UpdateSuffixSerializer { get; private set; }

    /// <summary>
    /// Internal <see cref="LaunchPadBooster.Mod"/> instance.
    /// </summary>
    internal LaunchPadBooster.Mod? InternalMod { get; private set; }

    #endregion // INTERNAL

    #region INTERNAL METHODS

    /// <inheritdoc/>
    public override void OnLoaded(List<GameObject> prefabs, List<Assembly> assemblies, ConfigFile config, ModData data) {
        if (this.UseLogger) {
            this.Logger = BepInEx.Logging.Logger.CreateLogSource(this.ModGuid);
            this.Logger.LogEvent += this.OnLogEvent;
        }

        using ModProfiler? _ = this.Profile();

        this.LogDebug($"{this} is now loading...");

        if (Utilities.IsLoaded(this.ModGuid)) {
            this.LogFatal($"{this} has already been loaded!");
            return;
        }

        if (!this.Data.IsGameCompatible()) {
            this.LogFatal($"{this} cannot be run on {this.ModGameType}, requires {Constants.GAME_TYPE}");
            return;
        }

        if (this.ModHasIncompatibilities) {
            bool incompatible = false;
            foreach (ModIncompatibilityInfo incompatibility in this.ModIncompatibilities) {
                if (Utilities.IsLoaded(incompatibility.Guid)) {
                    incompatible = true;
                    this.LogFatal($"{this} is incompatible with mod {incompatibility}");
                }
            }

            if (incompatible) {
                return;
            }
        }

        if (this.ModHasDependencies) {
            bool missing = false;
            foreach (ModDependencyInfo dependency in this.ModDependencies) {
                if (!Utilities.IsLoaded(dependency.Guid) && dependency.DependencyType == DependencyType.Hard) {
                    missing = true;
                    this.LogFatal($"{this} could not find dependency mod {dependency}");
                }
            }

            if (missing) {
                return;
            }
        }

        if (this.UseConfig) {
            this.Config = config;
            this.Config.SettingChanged += this.ConfigChanged;
            this.Config.ConfigReloaded += this.ConfigReloaded;

            string path = $"{Path.Combine(Constants.BIE_CONFIG_FOLDER, this.ModGuid)}.cfg";
            if (File.Exists(path)) {
                ConfigFile oldConfig = new ConfigFile(path, false);
                foreach ((ConfigDefinition key, ConfigEntryBase value) in oldConfig) {
                    this.Config.Bind(key, value);
                }
                oldConfig.Clear();

                File.Delete(path);
            }

            this.DoLoadConfiguration();
        }

        if (this.UseHarmony) {
            this.Harmony = new Harmony(this.ModGuid);

            this.DoHarmonyPatch(assemblies);
        }

        this.Prefabs.AddRange(prefabs ?? []);

        this.InternalMod = new LaunchPadBooster.Mod(this.Data.Guid, this.Data.Version.ToString());
        if (this.HasPrefabs) {
            this.InternalMod.AddPrefabs(this.Prefabs.AsReadOnly());
        }

        if (this.ModGameType == GameType.Both) {
            this.InternalMod.Networking.Required = true;
            this.InternalMod.Networking.VersionValidator = this.VersionValidator;
            this.InternalMod.Networking.JoinValidator = this.JoinValidator;
            this.InternalMod.Networking.JoinPrefixSerializer = this.JoinPrefixSerializer;
            this.InternalMod.Networking.JoinSuffixSerializer = this.JoinSuffixSerializer;
            this.InternalMod.Networking.UpdatePrefixSerializer = this.UpdatePrefixSerializer;
            this.InternalMod.Networking.UpdateSuffixSerializer = this.UpdateSuffixSerializer;
        }

        SceneManager.sceneLoaded += this.SceneLoaded;
        SceneManager.sceneUnloaded += this.SceneUnloaded;
        //MainMenuWindowManager.OnPageEnabled += this.MenuPageEnabled;

        lock (_lock) {
            ModBase.AllMods.Add(this);
        }

        this.Log($"{this} is now loaded!");
        this.OnStart();
        this.OnSplashLoaded(new(SceneManager.GetActiveScene(), LoadSceneMode.Single)).Forget();
    }

    /// <inheritdoc/>
    public override void OnUnloaded() {
        using ModProfiler? _ = this.Profile();

        this.LogDebug($"{this} is now unloading...");

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
        //MainMenuWindowManager.OnPageEnabled -= this.MenuPageEnabled;

        lock (_lock) {
            ModBase.AllMods.Remove(this);
        }

        this.Log($"{this} is now unloaded!");
    }

    /// <summary>
    /// Internal <see cref="ConfigFile.SettingChanged"/> event connection.
    /// Called when user makes any change to configuration file.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ConfigChanged(object sender, SettingChangedEventArgs e) {
        using ModProfiler? _ = this.Profile();

        this.OnConfigChanged(e.ChangedSetting);
    }

    /// <summary>
    /// Internal <see cref="ConfigFile.ConfigReloaded"/> event connection.
    /// Called when configuration file is reloaded.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ConfigReloaded(object sender, EventArgs e) {
        using ModProfiler? _ = this.Profile();

        this.OnConfigReloaded();
    }

    /// <summary>
    /// Internal <see cref="Awake"/> method.
    /// Called when <see cref="MonoBehaviour"/> is initalized
    /// </summary>
    private void Awake() {
        using ModProfiler? _ = this.Profile();

        this.OnAwake();
    }

    /// <summary>
    /// Internal <see cref="Awake"/> method.
    /// Called every frame before <see cref="LateUpdate"/>
    /// </summary>
    private void Update() {
        using ModProfiler? _ = this.Profile(true);

        this.OnUpdate(Time.deltaTime);
    }

    /// <summary>
    /// Internal <see cref="LateUpdate"/> method.
    /// Called after <see cref="Update"/>
    /// </summary>
    private void LateUpdate() {
        using ModProfiler? _ = this.Profile(true);

        this.OnLateUpdate(Time.deltaTime);
    }

    /// <summary>
    /// Internal <see cref="FixedUpdate"/> method.
    /// Called on a fixed framerate frames.
    /// </summary>
    private void FixedUpdate() {
        using ModProfiler? _ = this.Profile(true);

        this.OnLateUpdate(Time.fixedDeltaTime);
    }

    /// <summary>
    /// Internal <see cref="DoLoadConfiguration"/> method.
    /// Called when mod is ready to load configuration
    /// </summary>
    private void DoLoadConfiguration() {
        using ModProfiler? _ = this.Profile();

        this.LogDebug("Loading configuration...");

        try {
            this.OnConfigLoad();
        } catch (Exception ex) {
            this.LogFatal("Failed to load configuration!");
            this.LogException(ex);
        } finally {
            this.Log($"Loaded configuration with {this.Config.Count.ToStringSuffix("value", "", "s")}!");
        }
    }

    /// <summary>
    /// Internal <see cref="DoHarmonyPatch"/>
    /// Called when mod is ready to do patches.
    /// </summary>
    private void DoHarmonyPatch(List<Assembly> _assemblies) {
        using ModProfiler? _ = this.Profile();

        bool success = true;
        if (this.AutoPatch) {
            this.LogDebug("Harmony patching assemblies starting...");

            int assemblies = 0;
            try {
                success = this.DoAssembliesPatch(_assemblies, out assemblies);
            } catch (Exception ex) {
                this.LogFatal("Harmony patching failed!");
                this.LogException(ex);
                success = false;
            } finally {
                this.LogDebug($"Harmony patched {assemblies.ToStringSuffix("assemblies")}!");
            }
        }
        this.OnHarmonyPatched(success);
    }

    private bool DoAssembliesPatch(List<Assembly> _assemblies, out int patched) {
        int assemblies = 0;
        bool success = true;
        foreach ((Assembly assembly, List<ConditionalPatchClassProcessor> processors) in this.Harmony.CreatePatchersForAssemblies(_assemblies)) {
            using ModProfiler? _ = this.Profile();

            assemblies++;

            this.LogDebug($"Harmony patching assembly ({assembly.FullName()})");

            int patches = 0;
            try {
                success = this.DoClassPatch(processors, out patches);
            } catch (Exception ex) {
                this.LogFatal("Harmony patching failed!");
                this.LogException(ex);
                success = false;
            } finally {
                this.LogDebug($"Harmony patched assembly with {patches.ToStringSuffix("patch", "", "es")}!");
            }
        }

        patched = assemblies;
        return success;
    }

    private bool DoClassPatch(List<ConditionalPatchClassProcessor> processors, out int patched) {
        int patches = 0;
        bool success = true;

        this.LogDebug($"Harmony patching methods...");
        foreach (ConditionalPatchClassProcessor processor in processors) {
            using ModProfiler? _ = this.Profile();

            List<MethodInfo>? methods = null;
            try {
                methods = processor.Patch();
            } catch (Exception ex) {
                this.LogFatal("Harmony patching failed!");
                this.LogException(ex);
                success = false;
            } finally {
                if (methods?.Count > 0) {
                    this.LogDebug($"Harmony patched methods: \n\n{methods.Join((method) => $"{method.FullDescription()}", "\n")}\n");

                    patches += methods.Count;
                }
            }
        }
        patched = patches;
        return success;
    }

    /// <summary>
    /// Internal <see cref="UndoHarmonyPatch"/>
    /// Called when mod is ready to remove patches.
    /// </summary>
    private void UndoHarmonyPatch() {
        using ModProfiler? _ = this.Profile();

        this.Harmony.UnpatchSelf();

        this.OnHarmonyUnpatched();
    }

    #endregion // INTERNAL METHODS

    #region EVENT METHODS

    /// <summary>
    /// Called by <see cref="Mod"/> after core initialization is done.
    /// </summary>
    public abstract void OnStart();

    /// <summary>
    /// Called by <see cref="Mod"/> when configuration is ready to be binded.
    /// Implement your configurations here.
    /// </summary>
    public virtual void OnConfigLoad() { }

    /// <summary>
    /// Called by <see cref="Mod"/> when any configuration has been changed.
    /// </summary>
    /// <param name="entry"></param>
    public virtual void OnConfigChanged(ConfigEntryBase entry) { }

    /// <summary>
    /// Called when a config value is registered.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public virtual void OnConfigRegistered<TValue>(ConfigEntry<TValue> entry) { }

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

    public virtual void OnLogEvent(object sender, LogEventArgs e) { }

    public static LogLevel ToLogLevel(LogSeverity severity) {
        return severity switch {
            LogSeverity.Debug => LogLevel.Debug,
            LogSeverity.Information => LogLevel.Info,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Fatal => LogLevel.Fatal,
            LogSeverity.Exception => LogLevel.Error,
            LogSeverity.All => LogLevel.All,
            _ => LogLevel.Info
        };
    }

    /// <summary>
    /// Log function that is redirected to <see cref="Logger"/>
    /// Can be overriden to add or remove functionality.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="severity"></param>
    public override void Log(string message, LogSeverity severity = LogSeverity.Information) {
        if (this.UseLogger) {
            this.Logger?.Log(ToLogLevel(severity), message);
        }
    }

    /// <summary>
    /// Log function that is redirected to <see cref="Logger"/>
    /// Can be overriden to add or remove functionality.
    /// </summary>
    /// <param name="exception">Exception</param>
    public override void Log(Exception exception) {
        if (this.UseLogger) {
            this.Logger?.Log(ToLogLevel(LogSeverity.Exception), exception);
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
        if (this.UseLogger) {
            this.Logger?.Log(ToLogLevel(severity), string.Format(format, args));
        }
    }

    #endregion // LOGGING METHODS

    #region METHODS

    /// <summary>
    /// Registers a prefab.
    /// </summary>
    /// <typeparam name="TPrefab">The script for your prefab.</typeparam>
    /// <param name="prefab">The name of your prefab.</param>
    /// <returns>Prefab setup object.</returns>
    public LaunchPadBooster.PrefabSetup<TPrefab> RegisterPrefab<TPrefab>(string prefab) where TPrefab : Thing {
        using ModProfiler? _ = this.Profile();

        this.LogDebug($"Registering prefab {typeof(TPrefab).Name} with name {prefab}.");
        LaunchPadBooster.PrefabSetup<TPrefab> prefabSetup = this.InternalMod.SetupPrefabs<TPrefab>(prefab);
        this.LogDebug($"Registered prefab {typeof(TPrefab).Name} with name {prefab}.");
        return prefabSetup;
    }

    /// <summary>
    /// Registers a savedata type.
    /// </summary>
    /// <typeparam name="TSaveData">A thing savedata type.</typeparam>
    public void RegisterSaveData<TSaveData>() where TSaveData : ThingSaveData {
        using ModProfiler? _ = this.Profile();

        this.LogDebug($"Registering SaveData {typeof(TSaveData).Name}...");
        this.InternalMod.AddSaveDataType<TSaveData>();
        this.LogDebug($"Registered SaveData!");
    }

    /// <summary>
    /// Registers a network message.
    /// </summary>
    /// <typeparam name="TNetworkMessage">A network message type.</typeparam>
    public void RegisterNetworkMessage<TNetworkMessage>() where TNetworkMessage : INetworkMessage, new() {
        using ModProfiler? _ = this.Profile();

        this.LogDebug($"Registering NetworkMessage {typeof(TNetworkMessage).Name}...");
        this.InternalMod.Networking.RegisterMessage<TNetworkMessage>();
        this.LogDebug($"Registered NetworkMessage!");
    }

    public void RegisterRPC<TNetworkRPC>() where TNetworkRPC : INetworkRPC, new() {
        using ModProfiler? _ = this.Profile();

        this.LogDebug($"Registering NetworkRPC {typeof(TNetworkRPC).Name}...");
        this.InternalMod.Networking.RegisterRPC<TNetworkRPC>();
        this.LogDebug($"Registered NetworkRPC!");
    }

    /// <summary>
    /// Register a new configuration value.
    /// </summary>
    /// <typeparam name="TValue">A primitive type, enum or similar.</typeparam>
    /// <param name="data">Config data</param>
    public ConfigEntry<TValue> RegisterConfig<TValue>(ConfigData<TValue> data) where TValue : IComparable {
        using ModProfiler? _ = this.Profile();

        this.LogDebug($"Registering Config({data}) - default: {data.DefaultValue}");
        ConfigEntry<TValue> entry = this.Config.Bind<TValue>(data.Definition, data.DefaultValue, data.Description);
        this.LogDebug($"Registered Config has value: {entry.Value}");
        this.OnConfigRegistered<TValue>(entry);
        return entry;
    }

    /// <summary>
    /// Register a new configuration value.
    /// </summary>
    /// <typeparam name="TValue">A primitive type, enum or similar.</typeparam>
    /// <param name="defaultValue"></param>
    /// <param name="definition"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    public ConfigEntry<TValue> RegisterConfig<TValue>(TValue defaultValue, ConfigDefinition definition, ConfigDescription description) where TValue : IComparable {
        using ModProfiler? _ = this.Profile();

        this.LogDebug($"Registering Config ({definition}) - default: {defaultValue}");
        ConfigEntry<TValue> entry = this.Config.Bind<TValue>(definition, defaultValue, description);
        this.LogDebug($"Registered Config has value: {entry.Value}");
        this.OnConfigRegistered<TValue>(entry);
        return entry;
    }

    public ModProfiler? Profile(bool isUpdateMethod = false) {
        StackTrace stackTrace = new();
        StackFrame frame = stackTrace.GetFrame(1);

        return new ModProfiler(this.Logger, frame.GetMethod(), isUpdateMethod);
    }

    /// <summary>
    /// Gets the configuration entry from the section and key of a bound entry.
    /// </summary>
    /// <typeparam name="TValue">A primitive type, enum or similar.</typeparam>
    /// <param name="section"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public ConfigEntry<TValue>? GetConfigEntry<TValue>(string section, string key) where TValue : IComparable =>
        this.GetConfigEntry<TValue>(new(section, key));

    /// <summary>
    /// Gets the configuration entry from the definition of a bound entry.
    /// </summary>
    /// <typeparam name="TValue">A primitive type, enum or similar.</typeparam>
    /// <param name="definition"></param>
    /// <returns></returns>
    public ConfigEntry<TValue>? GetConfigEntry<TValue>(ConfigDefinition definition) where TValue : IComparable =>
        this.Config.TryGetEntry<TValue>(definition, out ConfigEntry<TValue> entry) ? entry : null;

    /// <summary>
    /// Gets the configuration value from the section and key of a bound entry.
    /// </summary>
    /// <typeparam name="TValue">A primitive type, enum or similar.</typeparam>
    /// <param name="section"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public TValue? GetConfigValue<TValue>(string section, string key) where TValue : IComparable =>
        this.GetConfigValue<TValue>(new(section, key));

    /// <summary>
    /// Gets the configuration value from the defintition of a bound entry.
    /// </summary>
    /// <typeparam name="TValue">A primitive type, enum or similar.</typeparam>
    /// <param name="definition"></param>
    /// <returns></returns>
    public TValue? GetConfigValue<TValue>(ConfigDefinition definition) where TValue : IComparable =>
        (TValue?) this.GetConfigEntry<TValue>(definition)?.BoxedValue;

    /// <summary>
    /// Gets the configuration value from the section and key of a bound entry.
    /// Will return defaultValue if it cannot acquire the value.
    /// </summary>
    /// <typeparam name="TValue">A primitive type, enum or similar.</typeparam>
    /// <param name="section"></param>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public TValue GetConfigValue<TValue>(string section, string key, TValue defaultValue) where TValue : IComparable =>
        this.GetConfigValue<TValue>(new(section, key), defaultValue);

    /// <summary>
    /// Gets the configuration value from the definition of a bound entry.
    /// Will return defaultValue if it cannot acquire the value.
    /// </summary>
    /// <typeparam name="TValue">A primitive type, enum or similar.</typeparam>
    /// <param name="definition"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public TValue GetConfigValue<TValue>(ConfigDefinition definition, TValue defaultValue) where TValue : IComparable =>
        (TValue?) this.GetConfigEntry<TValue>(definition)?.BoxedValue ?? defaultValue;

    /// <summary>
    /// Sets a configuration value from the section and key of a bound entry.
    /// </summary>
    /// <typeparam name="TValue">A primitive type, enum or similar.</typeparam>
    /// <param name="section"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SetConfigValue<TValue>(string section, string key, TValue value) where TValue : IComparable =>
        this.SetConfigValue<TValue>(new(section, key), value);

    /// <summary>
    /// Sets a configuration value from the definition of a bound entry.
    /// </summary>
    /// <typeparam name="TValue">A primitive type, enum or similar.</typeparam>
    /// <param name="definition"></param>
    /// <param name="value"></param>
    public void SetConfigValue<TValue>(ConfigDefinition definition, TValue value) where TValue : IComparable =>
        this.GetConfigEntry<TValue>(definition)?.Value = value;

    /// <summary>
    /// Note: this only compares mod info, as its the most significant.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(T other) => base.Equals(other);

    /// <summary>
    /// Note: this only compares mod info, as its the most significant.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(Mod<T> other) => base.Equals(other);

    /// <inheritdoc/>
    public override bool Equals(object other) => base.Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => base.GetHashCode();

    #endregion // PUBLIC METHODS

    #region OPERATORS

    public static bool operator ==(Mod<T> left, Mod<T> right) => left.Equals(right);
    public static bool operator !=(Mod<T> left, Mod<T> right) => !left.Equals(right);

    #endregion
}

public struct ConfigData<T> : IEquatable<ConfigData<T>> where T : IComparable {
    public T DefaultValue { get; set; }
    public ConfigDefinition Definition { get; set; }
    public ConfigDescription Description { get; set; }

    public ConfigData(T defaultValue, ConfigDefinition definition, ConfigDescription description) {
        this.DefaultValue = defaultValue;
        this.Definition = definition;
        this.Description = description;
    }

    public ConfigData(T defaultValue, T min, T max, ConfigDefinition definition, ConfigDescription description) {
        this.DefaultValue = defaultValue;
        this.Definition = definition;
        this.Description = new ConfigDescription(description.Description, new AcceptableValueRange<T>(min, max), description.Tags);
    }

    public ConfigData(T defaultValue, string section, string key, string description, AcceptableValueBase? acceptableValues = null, params object[] tags) {
        this.DefaultValue = defaultValue;
        this.Definition = new(section, key);
        this.Description = new(description, acceptableValues, tags);
    }

    public ConfigData(T defaultValue, T min, T max, string section, string key, string description, params object[] tags) {
        this.DefaultValue = defaultValue;
        this.Definition = new(section, key);
        this.Description = new(description, new AcceptableValueRange<T>(min, max), tags);
    }

    public override readonly bool Equals(object obj) =>
        obj is ConfigData<T> data && this.Equals(data);

    public readonly bool Equals(ConfigData<T> data) =>
        this.DefaultValue.Equals(data.DefaultValue) &&
        this.Definition.Equals(data.Definition) &&
        this.Description.Equals(data.Description);

    public override readonly int GetHashCode() =>
        HashCode.Combine(
            this.DefaultValue,
            this.Definition.Section,
            this.Definition.Key,
            this.Description.Description,
            this.Description.AcceptableValues,
            this.Description.Tags
        );

    public override readonly string ToString() => $"{this.Definition}";

    public static bool operator ==(ConfigData<T> left, ConfigData<T> right) => left.Equals(right);
    public static bool operator !=(ConfigData<T> left, ConfigData<T> right) => !left.Equals(right);
}
