#region

#endregion

namespace SEGI;

public class Plugin : Mod<Plugin> {
    public static Plugin? Instance { get; private set; }

    public static SEGI? SEGIInstance { get; private set; }

    public override bool UseLogger => true;
    public override bool UseConfig => true;
    public override bool UseHarmony => false;

    public override ModInfo Data => new ModInfo() {
        Name = "SEGIMod",
        Guid = "segimod",
        Version = new Version(1, 6, 0, 344),
        WorkshopId = 3281346086ul,
        GameType = GameType.Client,
    };

    public Plugin() : base() => Plugin.Instance = this;

    public override void OnLoaded(List<GameObject> prefabs) => base.OnLoaded(prefabs);

    public override void OnStart() { }

    public override void OnUpdate(float deltaTime) => Plugin.SEGIInstance?.enabled = ConfigData.Enabled;

    public override UniTask OnMainMenuPageEnabled(MenuPageEnabledArgs args) {
        Plugin.SEGIInstance = Camera.main.gameObject.AddComponent<SEGI>();
        Plugin.SEGIInstance.sun = WorldManager.Instance.WorldSun.TargetLight;
        GameObject.DontDestroyOnLoad(SEGIInstance);

        return UniTask.CompletedTask;
    }


    public override void OnConfigLoad() {
        ConfigData.enabled = Config.Bind(
                new ConfigDefinition("General", "Enabled"),
                true
        );

        // Voxel
        ConfigData.voxelResolution = Config.Bind(
                new ConfigDefinition("Voxel", "Resolution"),
                SEGI.VoxelResolution.High
        );

        ConfigData.halfResolution = Config.Bind(
                new ConfigDefinition("Voxel", "Half Resolution"),
                true
        );

        ConfigData.voxelSpaceSize = Config.Bind(
                new ConfigDefinition("Voxel", "Space Size"),
                25f,
                new ConfigDescription("1.0 to 100.0",
                new AcceptableValueRange<float>(1f, 100f)
        ));

        ConfigData.voxelAntiAliasing = Config.Bind(
                new ConfigDefinition("Voxel", "Anti Aliasing"),
                true
        );

        // Occlusion
        ConfigData.innerOcclusionLayers = Config.Bind(
                new ConfigDefinition("Occlusion", "Inner Occlusion Layers"),
                1,
                new ConfigDescription("0 to 2",
                new AcceptableValueRange<int>(0, 2)
        ));

        ConfigData.occlusionPower = Config.Bind(
                new ConfigDefinition("Occlusion", "Occlusion Power"),
                1f,
                new ConfigDescription("0.001 to 4.0",
                new AcceptableValueRange<float>(0.001f, 4f)
        ));

        ConfigData.occlusionStrength = Config.Bind(
                new ConfigDefinition("Occlusion", "Occlusion Strenth"),
                1f,
                new ConfigDescription("0.0 to 4.0",
                new AcceptableValueRange<float>(0f, 4f)
        ));

        ConfigData.secondaryOcclusionStrength = Config.Bind(
                new ConfigDefinition("Occlusion", "Secondary Occlusion Strenth"),
                1f,
                new ConfigDescription("0.1 to 4.0",
                new AcceptableValueRange<float>(0.1f, 4f)
        ));

        ConfigData.nearOcclusionStrength = Config.Bind(
                new ConfigDefinition("Occlusion", "Near Occlusion Strenth"),
                0.5f,
                new ConfigDescription("0 to 4.0",
                new AcceptableValueRange<float>(0f, 4f)
        ));

        ConfigData.farOcclusionStrength = Config.Bind(
                new ConfigDefinition("Occlusion", "Far Occlusion Strenth"),
                1f,
                new ConfigDescription("0.1 to 4.0",
                new AcceptableValueRange<float>(0.1f, 4f)
        ));

        ConfigData.farthestOcclusionStrength = Config.Bind(
                new ConfigDefinition("Occlusion", "Farthest Occlusion Strenth"),
                1f,
                new ConfigDescription("0.1 to 4.0",
                new AcceptableValueRange<float>(0.1f, 4f)
        ));

        // Reflection
        ConfigData.doReflections = Config.Bind(
                new ConfigDefinition("Refections", "Do Reflections"),
                true
        );

        ConfigData.infiniteBounces = Config.Bind(
                new ConfigDefinition("Refections", "Infinite Bounces"),
                true
        );

        ConfigData.reflectionSteps = Config.Bind(
                new ConfigDefinition("Refections", "Reflection Steps"),
                32,
                new ConfigDescription("12 to 128",
                new AcceptableValueRange<int>(12, 128)
        ));

        ConfigData.reflectionOcclusionPower = Config.Bind(
                new ConfigDefinition("Refections", "Reflection Occlusion Power"),
                1f,
                new ConfigDescription("0.001 to 4.0",
                new AcceptableValueRange<float>(0.001f, 4f)
         ));

        ConfigData.secondaryBounceGain = Config.Bind(
                new ConfigDefinition("Refections", "Secondary Bounce Gain"),
                0.75f,
                new ConfigDescription("0.1 to 4.0",
                new AcceptableValueRange<float>(0.1f, 4f)
        ));

        ConfigData.skyReflectionIntensity = Config.Bind(
                new ConfigDefinition("Refections", "Sky Reflection Intensity"),
                0.5f,
                new ConfigDescription("0.0 to 1.0f",
                new AcceptableValueRange<float>(0f, 1f)
        ));

        ConfigData.skyIntensity = Config.Bind(
                new ConfigDefinition("Refections", "Sky Intensity"),
                1f,
                new ConfigDescription("0 to 8.0",
                new AcceptableValueRange<float>(0f, 8f)
        ));

        ConfigData.softSunlight = Config.Bind(
                new ConfigDefinition("Refections", "Soft Sunlight"),
                1f,
                new ConfigDescription("0 to 16.0",
                new AcceptableValueRange<float>(0f, 16f)
        ));

        // Cones
        ConfigData.cones = Config.Bind(
                new ConfigDefinition("Cones", "Cones"),
                6,
                new ConfigDescription("1 to 128",
                new AcceptableValueRange<int>(1, 128)
        ));

        ConfigData.secondaryCones = Config.Bind(
                new ConfigDefinition("Cones", "Secondary Cones"),
                3,
                new ConfigDescription("3 to 16",
                new AcceptableValueRange<int>(3, 16)
        ));

        ConfigData.coneTraceSteps = Config.Bind(
                new ConfigDefinition("Cones", "Cone Trace Steps"),
                14,
                new ConfigDescription("1 to 32",
                new AcceptableValueRange<int>(1, 32)
        ));

        ConfigData.coneTraceBias = Config.Bind(
                new ConfigDefinition("Cones", "Cone Trace Bias"),
                1f,
                new ConfigDescription("0.0 to 4.0",
                new AcceptableValueRange<float>(0f, 4f)
        ));

        ConfigData.coneLength = Config.Bind(
                new ConfigDefinition("Cones", "Cone Length"),
                1f,
                new ConfigDescription("0.1 to 2.0",
                new AcceptableValueRange<float>(0.1f, 2f)
        ));

        ConfigData.coneWidth = Config.Bind(
                new ConfigDefinition("Cones", "Cone Width"),
                2.25f,
                new ConfigDescription("0.5 to 6.0",
                new AcceptableValueRange<float>(0.5f, 6f)
        ));

        // Light
        ConfigData.nearLightGain = Config.Bind(
                new ConfigDefinition("Light", "Near Light Gain"),
                1f,
                new ConfigDescription("0.0 to 4.0",
                new AcceptableValueRange<float>(0f, 8f)
        ));

        ConfigData.giGain = Config.Bind(
                new ConfigDefinition("Light", "Global Illumination Gain"),
                0.5f,
                new ConfigDescription("0.0 to 4.0",
                new AcceptableValueRange<float>(0f, 8f)
        ));

        ConfigData.shadowSpaceSize = Config.Bind(
                new ConfigDefinition("Light", "Shadow Space Size"),
                1f,
                new ConfigDescription("1.0 to 100.0",
                new AcceptableValueRange<float>(0f, 100f)
        ));


        // Sampling & Filtering
        ConfigData.gaussianMipFilter = Config.Bind(
                new ConfigDefinition("Sampling & Filtering", "Gaussian Mip Filter"),
                true
        );

        ConfigData.useBilateralFiltering = Config.Bind(
                new ConfigDefinition("Sampling & Filtering", "Use Bilateral Filtering"),
                true
        );

        ConfigData.stochasticSampling = Config.Bind(
                new ConfigDefinition("Sampling & Filtering", "Stochastic Sampling"),
                true
        );

        ConfigData.temporalBlendWeight = Config.Bind(
                new ConfigDefinition("Sampling & Filtering", "Temporal Blend Weight"),
                0.1f,
                new ConfigDescription("0.01 to 1.0",
                new AcceptableValueRange<float>(0.01f, 1f)
        ));
    }
}

internal struct ConfigData {
    public static ConfigEntry<bool>? enabled;
    public static bool Enabled => enabled?.Value ?? false;

    // Voxel
    public static ConfigEntry<SEGI.VoxelResolution>? voxelResolution;
    public static SEGI.VoxelResolution VoxelResolution => voxelResolution?.Value ?? SEGI.VoxelResolution.High;

    public static ConfigEntry<bool>? halfResolution;
    public static bool HalfResolution => halfResolution?.Value ?? false;

    public static ConfigEntry<float>? voxelSpaceSize;
    public static float VoxelSpaceSize => voxelSpaceSize?.Value ?? 25f;

    public static ConfigEntry<bool>? voxelAntiAliasing;
    public static bool VoxelAntiAliasing => voxelAntiAliasing?.Value ?? false;

    // Occlusion
    public static ConfigEntry<int>? innerOcclusionLayers;
    public static int InnerOcclusionLayers => innerOcclusionLayers?.Value ?? 1;

    public static ConfigEntry<float>? occlusionPower;
    public static float OcclusionPower => occlusionPower?.Value ?? 1f;

    public static ConfigEntry<float>? occlusionStrength;
    public static float OcclusionStrength => occlusionStrength?.Value ?? 1f;

    public static ConfigEntry<float>? secondaryOcclusionStrength;
    public static float SecondaryOcclusionStrength => secondaryOcclusionStrength?.Value ?? 1f;

    public static ConfigEntry<float>? nearOcclusionStrength;
    public static float NearOcclusionStrength => nearOcclusionStrength?.Value ?? 0.5f;

    public static ConfigEntry<float>? farOcclusionStrength;
    public static float FarOcclusionStrength => farOcclusionStrength?.Value ?? 1f;

    public static ConfigEntry<float>? farthestOcclusionStrength;
    public static float FarthestOcclusionStrength => farthestOcclusionStrength?.Value ?? 1f;


    // Reflections
    public static ConfigEntry<bool>? doReflections;
    public static bool DoReflections => doReflections?.Value ?? true;

    public static ConfigEntry<bool>? infiniteBounces;
    public static bool InfiniteBounces => infiniteBounces?.Value ?? false;

    public static ConfigEntry<int>? reflectionSteps;
    public static int ReflectionSteps => reflectionSteps?.Value ?? 32;

    public static ConfigEntry<float>? reflectionOcclusionPower;
    public static float ReflectionOcclusionPower => reflectionOcclusionPower?.Value ?? 1f;

    public static ConfigEntry<float>? secondaryBounceGain;
    public static float SecondaryBounceGain => secondaryBounceGain?.Value ?? 0.75f;

    public static ConfigEntry<float>? skyReflectionIntensity;
    public static float SkyReflectionIntensity => skyReflectionIntensity?.Value ?? 0.25f;

    public static ConfigEntry<float>? skyIntensity;
    public static float SkyIntensity => skyIntensity?.Value ?? 1f;

    public static ConfigEntry<float>? softSunlight;
    public static float SoftSunlight => skyIntensity?.Value ?? 0f;

    // Cones
    public static ConfigEntry<int>? cones;
    public static int Cones => cones?.Value ?? 6;

    public static ConfigEntry<int>? secondaryCones;
    public static int SecondaryCones => secondaryCones?.Value ?? 3;

    public static ConfigEntry<int>? coneTraceSteps;
    public static int ConeTraceSteps => coneTraceSteps?.Value ?? 14;

    public static ConfigEntry<float>? coneTraceBias;
    public static float ConeTraceBias => coneTraceBias?.Value ?? 1f;

    public static ConfigEntry<float>? coneLength;
    public static float ConeLength => coneLength?.Value ?? 1f;

    public static ConfigEntry<float>? coneWidth;
    public static float ConeWidth => coneWidth?.Value ?? 2.25f;


    // Light
    public static ConfigEntry<float>? nearLightGain;
    public static float NearLightGain => nearLightGain?.Value ?? 1f;

    public static ConfigEntry<float>? giGain;
    public static float GIGain => giGain?.Value ?? 0.5f;

    public static ConfigEntry<float>? shadowSpaceSize;
    public static float ShadowSpaceSize => shadowSpaceSize?.Value ?? 1f;


    // Sampling & Filtering
    public static ConfigEntry<bool>? gaussianMipFilter;
    public static bool GaussianMipFilter => gaussianMipFilter?.Value ?? true;

    public static ConfigEntry<bool>? useBilateralFiltering;
    public static bool UseBilateralFiltering => useBilateralFiltering?.Value ?? true;

    public static ConfigEntry<bool>? stochasticSampling;
    public static bool StochasticSampling => stochasticSampling?.Value ?? true;

    public static ConfigEntry<float>? temporalBlendWeight;
    public static float TemporalBlendWeight => temporalBlendWeight?.Value ?? 0.1f;
}