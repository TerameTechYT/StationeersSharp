#region

using static BepInEx.BepInDependency;
using MainMenuUI = Assets.Scripts.UI.MainMenu;

#endregion

namespace BetterWasteTank;

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
        Data.wasteCriticalRatio = Config.Bind(new ConfigDefinition("Configurables", "Waste Critical Ratio"),
            0.975f,
            new ConfigDescription("Ratio when \"Waste Tank Critical!\" alarm goes off.", new AcceptableValueRange<float>(0.0f, 1.0f)));

        Data.wasteCautionRatio = Config.Bind(new ConfigDefinition("Configurables", "Waste Caution Ratio"),
            0.75f,
            new ConfigDescription("Ratio when \"Waste Tank Caution\" alarm goes off.", new AcceptableValueRange<float>(0.0f, 1.0f)));

        Data.airCountOnlyBreathable = Config.Bind(new ConfigDefinition("Configurables", "Air Count Only Breathable"),
            true,
            new ConfigDescription("Should Air Tank warnings count moles of only breathable gas or total moles"));

        Data.airCriticalMoles = Config.Bind(new ConfigDefinition("Configurables", "Air Critical Moles"),
            7.5f,
            new ConfigDescription("Quantity of moles when \"Air Tank Critical!\" alarm goes off. (this number will be multiplied by how many moles a human breathes per tick)"));

        Data.airCautionMoles = Config.Bind(new ConfigDefinition("Configurables", "Air Caution Moles"),
            50f,
            new ConfigDescription("Quanitity of moles when \"Air Tank Caution\" alarm goes off. (this number will be multiplied by how many moles a human breathes per tick)"));
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
    public const string ModGuid = "betterwastetank";
    public const string ModName = "BetterWasteTank";
    public const string ModVersion = "1.5.0";
    public const ulong ModHandle = 3071913936;

    // Config Data
    public static ConfigEntry<float> wasteCriticalRatio;
    public static float WasteCriticalRatio => wasteCriticalRatio?.Value ?? 0.75f;

    public static ConfigEntry<float> wasteCautionRatio;
    public static float WasteCautionRatio => wasteCautionRatio?.Value ?? 0.975f;

    public static ConfigEntry<bool> airCountOnlyBreathable;
    public static bool AirCountOnlyBreathable => airCountOnlyBreathable?.Value ?? false;

    public static ConfigEntry<float> airCautionMoles;
    public static float AirCautionMoles => airCautionMoles?.Value ?? 7.5f;

    public static ConfigEntry<float> airCriticalMoles;
    public static float AirCriticalMoles => airCriticalMoles?.Value ?? 50f;

    internal static float AirTankMolesCritical => Human.MolesPerMinute.ToFloat() * AirCriticalMoles;
    internal static float AirTankMolesCaution => Human.MolesPerMinute.ToFloat() * AirCautionMoles;
}