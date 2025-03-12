#region

using MainMenuUI = Assets.Scripts.UI.MainMenu;

#endregion

namespace SEGI;

[BepInPlugin(Data.ModGuid, Data.ModName, Data.ModVersion)]
[BepInProcess(Constants.CLIENT_EXECUTABLE_NAME)]
public class Plugin : BaseUnityPlugin {
    public static Plugin Instance {
        get; private set;
    }

    public static Harmony HarmonyInstance {
        get; private set;
    }

    public static GameObject SEGIGameObject {
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
        Data.enabled = Config.Bind(
            new ConfigDefinition("General", "Enabled"),
            true
        );

        // Voxel
        Data.voxelResolution = Config.Bind(
            new ConfigDefinition("Voxel", "Resolution"),
            SEGI.VoxelResolution.High
        );

        Data.halfResolution = Config.Bind(
            new ConfigDefinition("Voxel", "Half Resolution"),
            true
        );

        Data.voxelSpaceSize = Config.Bind(
            new ConfigDefinition("Voxel", "Space Size"),
            25f,
            new ConfigDescription("1.0 to 100.0",
            new AcceptableValueRange<float>(1f, 100f)
        ));

        Data.voxelAntiAliasing = Config.Bind(
            new ConfigDefinition("Voxel", "Anti Aliasing"),
            true
        );


        // Occlusion
        Data.innerOcclusionLayers = Config.Bind(
            new ConfigDefinition("Occlusion", "Inner Occlusion Layers"),
            1,
            new ConfigDescription("0 to 2",
            new AcceptableValueRange<int>(0, 2)
        ));

        Data.occlusionPower = Config.Bind(
            new ConfigDefinition("Occlusion", "Occlusion Power"),
            1f,
            new ConfigDescription("0.001 to 4.0",
            new AcceptableValueRange<float>(0.001f, 4f)
        ));

        Data.occlusionStrength = Config.Bind(
            new ConfigDefinition("Occlusion", "Occlusion Strenth"),
            1f,
            new ConfigDescription("0.0 to 4.0",
            new AcceptableValueRange<float>(0f, 4f)
        ));

        Data.secondaryOcclusionStrength = Config.Bind(
            new ConfigDefinition("Occlusion", "Secondary Occlusion Strenth"),
            1f,
            new ConfigDescription("0.1 to 4.0",
            new AcceptableValueRange<float>(0.1f, 4f)
        ));

        Data.nearOcclusionStrength = Config.Bind(
            new ConfigDefinition("Occlusion", "Near Occlusion Strenth"),
            0.5f,
            new ConfigDescription("0 to 4.0",
            new AcceptableValueRange<float>(0f, 4f)
        ));

        Data.farOcclusionStrength = Config.Bind(
            new ConfigDefinition("Occlusion", "Far Occlusion Strenth"),
            1f,
            new ConfigDescription("0.1 to 4.0",
            new AcceptableValueRange<float>(0.1f, 4f)
        ));

        Data.farthestOcclusionStrength = Config.Bind(
            new ConfigDefinition("Occlusion", "Farthest Occlusion Strenth"),
            1f,
            new ConfigDescription("0.1 to 4.0",
            new AcceptableValueRange<float>(0.1f, 4f)
        ));

        // Reflection
        Data.doReflections = Config.Bind(
            new ConfigDefinition("Refections", "Do Reflections"),
            true
        );

        Data.infiniteBounces = Config.Bind(
            new ConfigDefinition("Refections", "Infinite Bounces"),
            true
        );

        Data.reflectionSteps = Config.Bind(
            new ConfigDefinition("Refections", "Reflection Steps"),
            32,
            new ConfigDescription("12 to 128",
            new AcceptableValueRange<int>(12, 128)
        ));

        Data.reflectionOcclusionPower = Config.Bind(
            new ConfigDefinition("Refections", "Reflection Occlusion Power"),
            1f,
            new ConfigDescription("0.001 to 4.0",
            new AcceptableValueRange<float>(0.001f, 4f)
         ));

        Data.secondaryBounceGain = Config.Bind(
            new ConfigDefinition("Refections", "Secondary Bounce Gain"),
            0.75f,
            new ConfigDescription("0.1 to 4.0",
            new AcceptableValueRange<float>(0.1f, 4f)
        ));

        Data.skyReflectionIntensity = Config.Bind(
            new ConfigDefinition("Refections", "Sky Reflection Intensity"),
            0.5f,
            new ConfigDescription("0.0 to 1.0f",
            new AcceptableValueRange<float>(0f, 1f)
        ));

        Data.skyIntensity = Config.Bind(
            new ConfigDefinition("Refections", "Sky Intensity"),
            1f,
            new ConfigDescription("0 to 8.0",
            new AcceptableValueRange<float>(0f, 8f)
        ));

        Data.softSunlight = Config.Bind(
            new ConfigDefinition("Refections", "Soft Sunlight"),
            1f,
            new ConfigDescription("0 to 16.0",
            new AcceptableValueRange<float>(0f, 16f)
        ));

        // Cones
        Data.cones = Config.Bind(
            new ConfigDefinition("Cones", "Cones"),
            6,
            new ConfigDescription("1 to 128",
            new AcceptableValueRange<int>(1, 128)
        ));

        Data.secondaryCones = Config.Bind(
            new ConfigDefinition("Cones", "Secondary Cones"),
            3,
            new ConfigDescription("3 to 16",
            new AcceptableValueRange<int>(3, 16)
        ));

        Data.coneTraceSteps = Config.Bind(
            new ConfigDefinition("Cones", "Cone Trace Steps"),
            14,
            new ConfigDescription("1 to 32",
            new AcceptableValueRange<int>(1, 32)
        ));

        Data.coneTraceBias = Config.Bind(
            new ConfigDefinition("Cones", "Cone Trace Bias"),
            1f,
            new ConfigDescription("0.0 to 4.0",
            new AcceptableValueRange<float>(0f, 4f)
        ));

        Data.coneLength = Config.Bind(
            new ConfigDefinition("Cones", "Cone Length"),
            1f,
            new ConfigDescription("0.1 to 2.0",
            new AcceptableValueRange<float>(0.1f, 2f)
        ));

        Data.coneWidth = Config.Bind(
            new ConfigDefinition("Cones", "Cone Width"),
            2.25f,
            new ConfigDescription("0.5 to 6.0",
            new AcceptableValueRange<float>(0.5f, 6f)
        ));

        // Light
        Data.nearLightGain = Config.Bind(
            new ConfigDefinition("Light", "Near Light Gain"),
            1f,
            new ConfigDescription("0.0 to 4.0",
            new AcceptableValueRange<float>(0f, 8f)
        ));

        Data.giGain = Config.Bind(
            new ConfigDefinition("Light", "Global Illumination Gain"),
            0.5f,
            new ConfigDescription("0.0 to 4.0",
            new AcceptableValueRange<float>(0f, 8f)
        ));

        Data.shadowSpaceSize = Config.Bind(
            new ConfigDefinition("Light", "Shadow Space Size"),
            1f,
            new ConfigDescription("1.0 to 100.0",
            new AcceptableValueRange<float>(0f, 100f)
        ));


        // Sampling & Filtering
        Data.gaussianMipFilter = Config.Bind(
            new ConfigDefinition("Sampling & Filtering", "Gaussian Mip Filter"),
            true
        );

        Data.useBilateralFiltering = Config.Bind(
            new ConfigDefinition("Sampling & Filtering", "Use Bilateral Filtering"),
            true
        );

        Data.stochasticSampling = Config.Bind(
            new ConfigDefinition("Sampling & Filtering", "Stochastic Sampling"),
            true
        );

        Data.temporalBlendWeight = Config.Bind(
            new ConfigDefinition("Sampling & Filtering", "Temporal Blend Weight"),
            0.1f,
            new ConfigDescription("0.01 to 1.0",
            new AcceptableValueRange<float>(0.01f, 1f)
        ));

        Config.SettingChanged += this.ConfigChanged;
    }

    private void ConfigChanged(object sender, SettingChangedEventArgs e) {
        if (e.ChangedSetting.Definition.Key == "Enabled") {
            Plugin.LogInfo($"SEGI is now {(Data.Enabled ? "Enabled" : "Disabled")}");
        }
        else {
            Plugin.LogInfo($"SEGI will use approximately {SEGIManager.SEGIInstance?.VRamUsage ?? -1f}kb of vram");
        }
    }

    public async UniTask OnBaseLoaded() {
        // Wait until game has loaded into main menu
        await UniTask.WaitUntil(() => MainMenuUI.Instance.IsVisible);

        // Print version after main menu is visible
        Plugin.LogInfo($"{Data.ModVersion} is installed.");

        Utilities.SetModVersion(Data.ModHandle, Data.ModVersion);

        Plugin.SEGIGameObject = GameObject.Find("SEGIManager") ?? new GameObject("SEGIManager");
        Plugin.SEGIGameObject.AddComponent<SEGIManager>();
        GameObject.DontDestroyOnLoad(SEGIGameObject);
    }

    public static void LogError(Exception ex) => Log($"[{ex.Source} - {ex.StackTrace}]: {ex.Message}", Severity.Error);
    public static void LogError(string message) => Log(message, Severity.Error);
    public static void LogWarning(string message) => Log(message, Severity.Warning);
    public static void LogInfo(string message) => Log(message, Severity.Info);

#if DEBUG
    public static void LogDebug(string message) => Log(message, Severity.Debug);
#else
    public static void LogDebug(string message) {
    }
#endif

    private static void Log(string message, Severity severity) {
        string newMessage = $"[{Data.ModName}]: {message}";

        switch (severity) {
            case Severity.Error: {
                ConsoleWindow.PrintError(newMessage);
                break;
            }
            case Severity.Warning: {
                ConsoleWindow.PrintAction(newMessage);
                break;
            }
            case Severity.Info: {
                ConsoleWindow.Print(newMessage);
                break;
            }
            default:
            case Severity.Debug: {
                ConsoleWindow.Print(newMessage, color: ConsoleColor.Gray, aged: false);
            }
            break;
        }
    }
}

internal struct Data {
    // Mod Data
    public const string ModGuid = "segimod";
    public const string ModName = "SEGIMod";
    public const string ModVersion = "1.3.0";
    public const ulong ModHandle = 3281346086;

    public static ConfigEntry<bool> enabled;
    public static bool Enabled => enabled?.Value ?? false;

    // Voxel
    public static ConfigEntry<SEGI.VoxelResolution> voxelResolution;
    public static SEGI.VoxelResolution VoxelResolution => voxelResolution?.Value ?? SEGI.VoxelResolution.High;

    public static ConfigEntry<bool> halfResolution;
    public static bool HalfResolution => halfResolution?.Value ?? false;

    public static ConfigEntry<float> voxelSpaceSize;
    public static float VoxelSpaceSize => voxelSpaceSize?.Value ?? 25f;

    public static ConfigEntry<bool> voxelAntiAliasing;
    public static bool VoxelAntiAliasing => voxelAntiAliasing?.Value ?? false;

    // Occlusion
    public static ConfigEntry<int> innerOcclusionLayers;
    public static int InnerOcclusionLayers => innerOcclusionLayers?.Value ?? 1;

    public static ConfigEntry<float> occlusionPower;
    public static float OcclusionPower => occlusionPower?.Value ?? 1f;

    public static ConfigEntry<float> occlusionStrength;
    public static float OcclusionStrength => occlusionStrength?.Value ?? 1f;

    public static ConfigEntry<float> secondaryOcclusionStrength;
    public static float SecondaryOcclusionStrength => secondaryOcclusionStrength?.Value ?? 1f;

    public static ConfigEntry<float> nearOcclusionStrength;
    public static float NearOcclusionStrength => nearOcclusionStrength?.Value ?? 0.5f;

    public static ConfigEntry<float> farOcclusionStrength;
    public static float FarOcclusionStrength => farOcclusionStrength?.Value ?? 1f;

    public static ConfigEntry<float> farthestOcclusionStrength;
    public static float FarthestOcclusionStrength => farthestOcclusionStrength?.Value ?? 1f;


    // Reflections
    public static ConfigEntry<bool> doReflections;
    public static bool DoReflections => doReflections?.Value ?? true;

    public static ConfigEntry<bool> infiniteBounces;
    public static bool InfiniteBounces => infiniteBounces?.Value ?? false;

    public static ConfigEntry<int> reflectionSteps;
    public static int ReflectionSteps => reflectionSteps?.Value ?? 32;

    public static ConfigEntry<float> reflectionOcclusionPower;
    public static float ReflectionOcclusionPower => reflectionOcclusionPower?.Value ?? 1f;

    public static ConfigEntry<float> secondaryBounceGain;
    public static float SecondaryBounceGain => secondaryBounceGain?.Value ?? 0.75f;

    public static ConfigEntry<float> skyReflectionIntensity;
    public static float SkyReflectionIntensity => skyReflectionIntensity?.Value ?? 0.25f;

    public static ConfigEntry<float> skyIntensity;
    public static float SkyIntensity => skyIntensity?.Value ?? 1f;

    public static ConfigEntry<float> softSunlight;
    public static float SoftSunlight => skyIntensity?.Value ?? 0f;

    // Cones
    public static ConfigEntry<int> cones;
    public static int Cones => cones?.Value ?? 6;

    public static ConfigEntry<int> secondaryCones;
    public static int SecondaryCones => secondaryCones?.Value ?? 3;

    public static ConfigEntry<int> coneTraceSteps;
    public static int ConeTraceSteps => coneTraceSteps?.Value ?? 14;

    public static ConfigEntry<float> coneTraceBias;
    public static float ConeTraceBias => coneTraceBias?.Value ?? 1f;

    public static ConfigEntry<float> coneLength;
    public static float ConeLength => coneLength?.Value ?? 1f;

    public static ConfigEntry<float> coneWidth;
    public static float ConeWidth => coneWidth?.Value ?? 2.25f;


    // Light
    public static ConfigEntry<float> nearLightGain;
    public static float NearLightGain => nearLightGain?.Value ?? 1f;

    public static ConfigEntry<float> giGain;
    public static float GIGain => giGain?.Value ?? 0.5f;

    public static ConfigEntry<float> shadowSpaceSize;
    public static float ShadowSpaceSize => shadowSpaceSize?.Value ?? 1f;


    // Sampling & Filtering
    public static ConfigEntry<bool> gaussianMipFilter;
    public static bool GaussianMipFilter => gaussianMipFilter?.Value ?? true;

    public static ConfigEntry<bool> useBilateralFiltering;
    public static bool UseBilateralFiltering => useBilateralFiltering?.Value ?? true;

    public static ConfigEntry<bool> stochasticSampling;
    public static bool StochasticSampling => stochasticSampling?.Value ?? true;

    public static ConfigEntry<float> temporalBlendWeight;
    public static float TemporalBlendWeight => temporalBlendWeight?.Value ?? 0.1f;
}