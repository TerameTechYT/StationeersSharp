#region

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

    public void LoadConfiguration() {

    }

    public async UniTask OnBaseLoaded() {
        // Wait until game has loaded into main menu
        await UniTask.WaitUntil(() => MainMenuUI.Instance.IsVisible);

        // Print version after main menu is visible
        LogInfo($"{Data.ModVersion} is installed.");

        Utilities.SetModVersion(Data.ModHandle, Data.ModVersion);
    }

    public static void LogException(Exception ex) => StationeersLog.LogException(Data.ModName, ex);
    public static void LogError(string message) => StationeersLog.LogError(Data.ModName, message);
    public static void LogWarning(string message) => StationeersLog.LogWarning(Data.ModName, message);
    public static void LogInfo(string message) => StationeersLog.LogInfo(Data.ModName, message);
    public static void LogDebug(string message) => StationeersLog.LogDebug(Data.ModName, message);
}

internal struct Data {
    // Mod Data
    public const string ModGuid = "betterhydroponics";
    public const string ModName = "BetterHydroponics";
    public const string ModVersion = "1.0.0";
    public const ulong ModHandle = 0;

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