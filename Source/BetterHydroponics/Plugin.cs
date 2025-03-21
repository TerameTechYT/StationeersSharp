#region

using HarmonyLib.Tools;
using static BepInEx.BepInDependency;
using MainMenuUI = Assets.Scripts.UI.MainMenu;

#endregion

namespace BetterHydroponics;

[BepInPlugin(Data.ModGuid, Data.ModName, Data.ModVersion)]
// https://steamcommunity.com/sharedfiles/filedetails/?id=3428763681
//[BepInIncompatibility("")]
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
        get => Plugin.Instance.Logger;
    }

    [UsedImplicitly]
    public void Awake() {
        Plugin.LogDebug("Mod Started.");
        if (Utilities.IsLoaded(Data.ModGuid)) {
            throw new AlreadyLoadedException(Data.ModName, Data.ModGuid, Data.ModVersion);
        }

        Plugin.LogDebug("Loading configuration.");
        this.LoadConfiguration();

        Plugin.Instance = this;
        HarmonyFileLog.Enabled = Constants.DEBUG_MODE;
        Plugin.HarmonyInstance = new Harmony(Data.ModGuid);

        Plugin.LogDebug($"Harmony patch starting.");
        try {
            Plugin.HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
        catch (HarmonyException ex) {
            Plugin.LogException(ex);
            Plugin.LogError($"Harmony failed to patch! Please press {Utilities.GetConsoleKeyCode()} and run 'slib report'!");
        }
        finally {
            Plugin.LogDebug($"Harmony patch finished.");
        }
    }

    public void LoadConfiguration() {

    }

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
    public const string ModGuid = "betterhydroponics";
    public const string ModName = "BetterHydroponics";
    public const string ModVersion = "1.0.0";
    public const ulong ModHandle = 3449149492;

    public static readonly Dictionary<LogicSlotType, Func<Plant, int, double>> PlantReadDictionary = new() {
        { LogicSlotType.Temperature, (plant, slotId) => plant?.PlantStatus.TemperatureEfficiency ?? 0.0},
        { LogicSlotType.Pressure, (plant, slotId) => plant?.PlantStatus.PressureEfficiency ?? 0.0},
        { LogicSlotType.PressureAir, (plant, slotId) => plant?.PlantStatus.BreathingEfficiency ?? 0.0},
        { LogicSlotType.Volume, (plant, slotId) => plant?.PlantStatus.HydrationEfficiency ?? 0.0},
        { LogicSlotType.Charge, (plant, slotId) => plant?.PlantStatus.LightEfficiency ?? 0.0},
        { LogicSlotType.On, (plant, slotId) => plant?.PlantRecord.Age ?? 0.0},
        { LogicSlotType.Lock, (plant, slotId) => plant?.PlantRecord.LightStress ?? 0.0},
        { LogicSlotType.Open, (plant, slotId) => plant?.PlantRecord.TimeLitRatio ?? 0.0},
        { LogicSlotType.Mode, (plant, slotId) => plant?.PlantRecord.TimeDarknessRatio ?? 0.0},
    };

    /*public static readonly Dictionary<LogicType, Func<Plant, double>> LogicReadDictionary = new() {
        { LogicType.TemperatureSetting, (plant) => plant?.PlantStatus.TemperatureEfficiency ?? 0.0 },
        { LogicType.PressureEfficiency, (plant) => plant?.PlantStatus.PressureEfficiency ?? 0.0 },
        { LogicType.PressureSetting, (plant) => plant?.PlantStatus.BreathingEfficiency ?? 0.0 },
        { LogicType.RatioWaterInput, (plant) => plant?.PlantStatus.HydrationEfficiency ?? 0.0 },
        { LogicType.Charge, (plant) => plant?.PlantStatus.LightEfficiency ?? 0.0 },
        { LogicType.Setting, (plant) => plant?.PlantRecord.LightStress ?? 0.0 },
        { LogicType.SettingInput, (plant) => plant?.PlantRecord.TimeLitRatio ?? 0.0 },
        { LogicType.SettingOutput, (plant) => plant?.PlantRecord.TimeDarknessRatio ?? 0.0 },
        { LogicType.Time, (plant) => plant?.PlantRecord.Age ?? 0.0 },
    };*/
}