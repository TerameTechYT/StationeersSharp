﻿#region

using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using MainMenuUI = Assets.Scripts.UI.MainMenu;

#endregion

namespace SEGI;

[BepInPlugin(Data.ModGuid, Data.ModName, Data.ModVersion)]
[BepInProcess(Data.ExecutableName)]
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
        if (Chainloader.PluginInfos.TryGetValue(Data.ModGuid, out _))
            throw new Data.AlreadyLoadedException($"Mod {Data.ModName} ({Data.ModGuid}) - {Data.ModVersion} has already been loaded!");

        LoadConfiguration();

        Instance = this;
        HarmonyInstance = new Harmony(Data.ModGuid);
        HarmonyInstance.PatchAll();

        // Thx jixxed for awesome code :)
        SceneManager.sceneLoaded += (scene, _) => {
            if (scene.name == "Base")
                OnBaseLoaded().Forget();
        };
    }

    public void LoadConfiguration() {
        // Voxel
        Data.VoxelResolution = Config.Bind("Voxel", "Resolution", SEGI.VoxelResolution.High, "High or Low");
        Data.HalfResolution = Config.Bind("Voxel", "Half Resolution", true, "true or false");
        Data.VoxelSpaceSize = Config.Bind("Voxel", "Space Size", 25f, "1.0 to 100.0");
        Data.VoxelAntiAliasing = Config.Bind("Voxel", "Anti Aliasing", true, "true or false");

        // Occlusion
        Data.InnerOcclusionLayers = Config.Bind("Occlusion", "Inner Occlusion Layers", 1, "0 to 2");
        Data.OcclusionPower = Config.Bind("Occlusion", "Occlusion Power", 1f, "0.001 to 4.0");
        Data.OcclusionStrenth = Config.Bind("Occlusion", "Occlusion Strenth", 1f, "0.0 to 4.0");
        Data.SecondaryOcclusionStrenth = Config.Bind("Occlusion", "Secondary Occlusion Strenth", 1f, "0.1 to 4.0");
        Data.NearOcclusionStrenth = Config.Bind("Occlusion", "Near Occlusion Strenth", 0.5f, "0 to 4.0");
        Data.FarOcclusionStrenth = Config.Bind("Occlusion", "Far Occlusion Strenth", 1f, "0.1 to 4.0");
        Data.FarthestOcclusionStrenth = Config.Bind("Occlusion", "Farthest Occlusion Strenth", 1f, "0.1 to 4.0");

        // Reflection
        Data.DoReflections = Config.Bind("Refections", "Do Reflections", true, "true or false");
        Data.InfiniteBounces = Config.Bind("Refections", "Infinite Bounces", true, "true or false");
        Data.ReflectionSteps = Config.Bind("Refections", "Reflection Steps", 32, "12 to 128");
        Data.ReflectionOcclusionPower = Config.Bind("Refections", "Reflection Occlusion Power", 1f, "0.001 to 4.0");
        Data.SecondaryBounceGain = Config.Bind("Refections", "Secondary Bounce Gain", 0.75f, "0.1 to 4.0");
        Data.SkyReflectionIntensity = Config.Bind("Refections", "Sky Reflection Intensity", 0.5f, "0.0 to 1.0f");

        // Cones
        Data.Cones = Config.Bind("Cones", "Cones", 6, "1 to 128");
        Data.SecondaryCones = Config.Bind("Cones", "Secondary Cones", 3, "3 to 16");
        Data.ConeTraceSteps = Config.Bind("Cones", "Cone Trace Steps", 14, "1 to 32");
        Data.ConeTraceBias = Config.Bind("Cones", "Cone Trace Bias", 1f, "0.0 to 4.0");
        Data.ConeLength = Config.Bind("Cones", "Cone Length", 1f, "0.1 to 2.0");
        Data.ConeWidth = Config.Bind("Cones", "Cone Width", 2.25f, "0.5 to 6.0");

        // Light
        Data.NearLightGain = Config.Bind("Light", "Near Light Gain", 1f, "0.0 to 4.0");
        Data.GIGain = Config.Bind("Light", "Global Illumination Gain", 0.5f, "0.0 to 4.0");
        Data.ShadowSpaceSize = Config.Bind("Light", "Shadow Space Size", 1f, "1.0 to 100.0");

        // Sampling & Filtering
        Data.GaussianMipFilter = Config.Bind("Sampling & Filtering", "Gaussian Mip Filter", true, "true or false");
        Data.UseBilateralFiltering =
            Config.Bind("Sampling & Filtering", "Use Bilateral Filtering", true, "true or false");
        Data.StochasticSampling = Config.Bind("Sampling & Filtering", "Stochastic Sampling", true, "true or false");
        Data.TemporalBlendWeight = Config.Bind("Sampling & Filtering", "Temporal Blend Weight", 0.1f, "0.01 to 1.0");
    }

    public async UniTask OnBaseLoaded() {
        // Wait until game has loaded into main menu
        await UniTask.WaitUntil(() => MainMenuUI.Instance.IsVisible);

        // Print version after main menu is visible
        LogInfo("is installed.");

        SetModVersion();

        EnableSEGI();
    }

    public static void EnableSEGI() {
        SEGIGameObject = GameObject.Find("SEGIManager") ?? new GameObject("SEGIManager");
        _ = SEGIGameObject.AddComponent<SEGIManager>();
        DontDestroyOnLoad(SEGIGameObject);
    }

    private void SetModVersion() {
        ModData mod = WorkshopMenu.ModsConfig.Mods.Find((mod) => mod.GetAboutData().WorkshopHandle == Data.ModHandle);
        if (mod == null) {
            return;
        }

        ModAbout aboutData = mod.GetAboutData();
        aboutData.Version = Data.ModVersion;

        Traverse.Create(mod).Field("_modAboutData").SetValue(aboutData);
    }

    public static void LogError(Exception ex) => Log($"[{ex.Source} - {ex.StackTrace}]: {ex.Message}", Data.Severity.Error);
    public static void LogError(string message) => Log(message, Data.Severity.Error);
    public static void LogWarning(string message) => Log(message, Data.Severity.Warning);
    public static void LogInfo(string message) => Log(message, Data.Severity.Info);

    private static void Log(string message, Data.Severity severity) {
        string newMessage = $"[{Data.ModName} - v{Data.ModVersion}]: {message}";

        switch (severity) {
            case Data.Severity.Error: {
                ConsoleWindow.PrintError(newMessage);
                break;
            }
            case Data.Severity.Warning: {
                ConsoleWindow.PrintAction(newMessage);
                break;
            }
            case Data.Severity.Info:
            default: {
                ConsoleWindow.Print(newMessage);
                break;
            }
        }
    }
}

internal struct Data {
    // Mod Data
    public const string ModGuid = "segimod";
    public const string ModName = "SEGIMod";
    public const string ModVersion = "1.1.0";
    public const ulong ModHandle = 3281346086;

    // Game Data
    public const string ExecutableName = "rocketstation.exe";
    public const string DSExecutableName = "rocketstation_DedicatedServer.exe";

    // Log Data
    internal enum Severity {
        Error,
        Warning,
        Info
    }

    public sealed class AlreadyLoadedException : Exception {
        public AlreadyLoadedException(string message) : base(message) {
        }

        public AlreadyLoadedException(string message, Exception innerException) : base(message, innerException) {
        }

        public AlreadyLoadedException() {
        }
    }

    // Voxel
    public static ConfigEntry<SEGI.VoxelResolution> VoxelResolution;
    public static ConfigEntry<bool> HalfResolution;
    public static ConfigEntry<float> VoxelSpaceSize;
    public static ConfigEntry<bool> VoxelAntiAliasing;

    // Occlusion
    public static ConfigEntry<int> InnerOcclusionLayers;
    public static ConfigEntry<float> OcclusionPower;
    public static ConfigEntry<float> OcclusionStrenth;
    public static ConfigEntry<float> SecondaryOcclusionStrenth;
    public static ConfigEntry<float> NearOcclusionStrenth;
    public static ConfigEntry<float> FarOcclusionStrenth;
    public static ConfigEntry<float> FarthestOcclusionStrenth;

    // Reflections
    public static ConfigEntry<bool> DoReflections;
    public static ConfigEntry<bool> InfiniteBounces;
    public static ConfigEntry<int> ReflectionSteps;
    public static ConfigEntry<float> ReflectionOcclusionPower;
    public static ConfigEntry<float> SecondaryBounceGain;
    public static ConfigEntry<float> SkyReflectionIntensity;

    // Cones
    public static ConfigEntry<int> Cones;
    public static ConfigEntry<int> SecondaryCones;
    public static ConfigEntry<int> ConeTraceSteps;
    public static ConfigEntry<float> ConeTraceBias;
    public static ConfigEntry<float> ConeLength;
    public static ConfigEntry<float> ConeWidth;

    // Light
    public static ConfigEntry<float> NearLightGain;
    public static ConfigEntry<float> GIGain;
    public static ConfigEntry<float> ShadowSpaceSize;

    // Sampling & Filtering
    public static ConfigEntry<bool> GaussianMipFilter;
    public static ConfigEntry<bool> UseBilateralFiltering;
    public static ConfigEntry<bool> StochasticSampling;
    public static ConfigEntry<float> TemporalBlendWeight;
}