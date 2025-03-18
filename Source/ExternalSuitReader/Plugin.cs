#region

using static BepInEx.BepInDependency;
using MainMenuUI = Assets.Scripts.UI.MainMenu;

#endregion

namespace ExternalSuitReader;

[BepInPlugin(Data.ModGuid, Data.ModName, Data.ModVersion)]
[BepInDependency(Constants.STATIONEERS_LIBRARY_GUID, DependencyFlags.HardDependency)]
[BepInProcess(Constants.CLIENT_EXECUTABLE_NAME)]
[BepInProcess(Constants.HEADLESS_EXECUTABLE_NAME)]
public class Plugin : BaseUnityPlugin {
    public static Plugin Instance {
        get; private set;
    }

    public static Harmony HarmonyInstance {
        get; private set;
    }

    public static ManualLogSource LoggerInstance {
        get => Instance.Logger;
    }

    [UsedImplicitly]
    public void Awake() {
        if (Utilities.IsLoaded(Data.ModGuid)) {
            throw new AlreadyLoadedException(Data.ModName, Data.ModGuid, Data.ModVersion);
        }

        this.LoadConfiguration();

        Plugin.Instance = this;
        Plugin.HarmonyInstance = new Harmony(Data.ModGuid);
        Plugin.HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

        // Thx jixxed for awesome code :)
        SceneManager.sceneLoaded += (scene, _) => {
            if (scene.name == Constants.BASE_SCENE_NAME) {
                OnBaseLoaded().Forget();
            }
        };
    }

    private void LoadConfiguration() => Data.enableExperimentalSaving = Config.Bind(new ConfigDefinition("Configurables", "Enable Experimental Saving"), false);

    public async UniTask OnBaseLoaded() {
        // Wait until game has loaded into main menu
        await UniTask.WaitUntil(() => MainMenuUI.Instance.IsVisible);

        // Print version after main menu is visible
        LogInfo($"v{Data.ModVersion} is installed.");

        Utilities.SetModVersion(Data.ModHandle, Data.ModVersion);
    }

    public static void LogException(Exception ex) => Plugin.Log($"[{ex?.Source} - {ex?.StackTrace}]: {ex?.Message}", Severity.Error);
    public static void LogError(string message) => Plugin.Log(message, Severity.Error);
    public static void LogWarning(string message) => Plugin.Log(message, Severity.Warning);
    public static void LogInfo(string message) => Plugin.Log(message, Severity.Info);
    public static void LogDebug(string message) {
        if (Constants.DEBUG_MODE) {
            Plugin.Log(message, Severity.Debug);
        }
    }

    private static void Log(string message, Severity severity) {
        string newMessage = $"[{Data.ModName}]: {message}";

        switch (severity) {
            case Severity.Error: {
                Plugin.LoggerInstance.LogError(message);
                ConsoleWindow.PrintError(newMessage);
                break;
            }
            case Severity.Warning: {
                Plugin.LoggerInstance.LogWarning(message);
                ConsoleWindow.PrintAction(newMessage);
                break;
            }
            case Severity.Info: {
                Plugin.LoggerInstance.LogInfo(message);
                ConsoleWindow.Print(newMessage);
                break;
            }
            default:
            case Severity.Debug: {
                Plugin.LoggerInstance.LogDebug(message);
                ConsoleWindow.Print(newMessage, color: ConsoleColor.Gray, aged: false);
            }
            break;
        }
    }
}

internal struct Data {
    // Mod Data
    public const string ModGuid = "externalsuitreader";
    public const string ModName = "ExternalSuitReader";
    public const string ModVersion = "1.5.0";
    public const ulong ModHandle = 3071985478;

    // Config Data
    public static ConfigEntry<bool> enableExperimentalSaving;
    public static bool EnableExperimentalSaving => enableExperimentalSaving?.Value ?? false;

    /*
     * Base Allowed Logic Types:
     * 
     * LogicType.PressureExternal:
     * LogicType.Setting:
     * LogicType.Volume:
     * LogicType.PressureSetting:
     * LogicType.TemperatureSetting:
     * LogicType.TemperatureExternal:
     * LogicType.Filtration:
     * LogicType.AirRelease:
     * LogicType.PositionX:
     * LogicType.PositionY:
     * LogicType.PositionZ:
     * LogicType.VelocityMagnitude:
     * LogicType.VelocityRelativeX:
     * LogicType.VelocityRelativeY:
     * LogicType.VelocityRelativeZ:
     * LogicType.SoundAlert:
     * LogicType.ForwardX:
     * LogicType.ForwardY:
     * LogicType.ForwardZ:
     * LogicType.Orientation:
     * LogicType.VelocityX:
     * LogicType.VelocityY:
     * LogicType.VelocityZ:
     * LogicType.EntityState:
     */
    public static Dictionary<long, List<DoubleReference>> AllAdvancedSuits = [];
    public static int ChannelCount => 8;

    public static readonly Dictionary<LogicType, Func<AdvancedSuit, double>> LogicReadDictionary = new() {
        // oxygen
        {LogicType.RatioOxygenOutput, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.Oxygen)},
        {LogicType.RatioLiquidOxygenOutput, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.LiquidOxygen)},

        // nitrogen
        {LogicType.RatioNitrogenOutput, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.Nitrogen)},
        {LogicType.RatioLiquidNitrogenOutput, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.LiquidNitrogen)},

        // carbon dioxide
        {LogicType.RatioCarbonDioxideOutput, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.CarbonDioxide)},
        {LogicType.RatioLiquidCarbonDioxideOutput, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.LiquidCarbonDioxide)},

        // volatiles
        {LogicType.RatioVolatilesOutput, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.Volatiles)},
        {LogicType.RatioLiquidVolatilesOutput, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.LiquidVolatiles)},

        // pollutant
        {LogicType.RatioPollutantOutput, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.Pollutant)},
        {LogicType.RatioLiquidPollutantOutput, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.LiquidPollutant)},

        // nitrous oxide 
        {LogicType.RatioNitrousOxideOutput, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.NitrousOxide)},
        {LogicType.RatioLiquidNitrousOxideOutput, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.LiquidNitrousOxide)},

        // steam, water & polluted water
        {LogicType.RatioSteam, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.Steam)},
        {LogicType.RatioWaterOutput, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.Water)},
        {LogicType.RatioWaterOutput2, (suit) => suit.WorldAtmosphere.GetGasTypeRatio(Chemistry.GasType.PollutedWater)},

        // data channels
        /*{LogicType.Channel0, (suit) => Functions.GetSuitChannel(suit.ReferenceId, 0)},
        {LogicType.Channel1, (suit) => Functions.GetSuitChannel(suit.ReferenceId, 1)},
        {LogicType.Channel2, (suit) => Functions.GetSuitChannel(suit.ReferenceId, 2)},
        {LogicType.Channel3, (suit) => Functions.GetSuitChannel(suit.ReferenceId, 3)},
        {LogicType.Channel4, (suit) => Functions.GetSuitChannel(suit.ReferenceId, 4)},
        {LogicType.Channel5, (suit) => Functions.GetSuitChannel(suit.ReferenceId, 5)},
        {LogicType.Channel6, (suit) => Functions.GetSuitChannel(suit.ReferenceId, 6)},
        {LogicType.Channel7, (suit) => Functions.GetSuitChannel(suit.ReferenceId, 7)},*/

        // other
        {LogicType.TotalMolesOutput, (suit) => suit.WorldAtmosphere.TotalMoles.ToDouble()},
        {LogicType.TargetPrefabHash, (suit) => CursorManager.CursorThing?.PrefabHash ?? 0.0},
        {LogicType.Time, (suit) => WorldManager.DaysPast},
        {LogicType.Charge, (suit) => suit.Battery?.PowerRatio ?? 0.0},

        // target/looking at position
        {LogicType.TargetX, (suit) => CursorManager.CursorHit.point.x},
        {LogicType.TargetY, (suit) => CursorManager.CursorHit.point.y},
        {LogicType.TargetZ, (suit) => CursorManager.CursorHit.point.x},
    };

    public static readonly Dictionary<LogicType, Action<AdvancedSuit, double>> LogicWriteDictionary = new() {
        // data channels
        /*{LogicType.Channel0, (suit, value) => Functions.SetSuitChannel(suit.ReferenceId, 0, value)},
        {LogicType.Channel1, (suit, value) => Functions.SetSuitChannel(suit.ReferenceId, 1, value)},
        {LogicType.Channel2, (suit, value) => Functions.SetSuitChannel(suit.ReferenceId, 2, value)},
        {LogicType.Channel3, (suit, value) => Functions.SetSuitChannel(suit.ReferenceId, 3, value)},
        {LogicType.Channel4, (suit, value) => Functions.SetSuitChannel(suit.ReferenceId, 4, value)},
        {LogicType.Channel5, (suit, value) => Functions.SetSuitChannel(suit.ReferenceId, 5, value)},
        {LogicType.Channel6, (suit, value) => Functions.SetSuitChannel(suit.ReferenceId, 6, value)},
        {LogicType.Channel7, (suit, value) => Functions.SetSuitChannel(suit.ReferenceId, 7, value)},*/
    };
}