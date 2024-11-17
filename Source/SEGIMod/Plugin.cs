#region

using Assets.Scripts;
using Assets.Scripts.UI;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

#endregion

namespace SEGI;

[BepInPlugin(Data.ModGuid, Data.ModName, Data.ModVersion)]
[BepInProcess("rocketstation.exe")]
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

        this.LoadConfiguration();

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
        Data.VoxelResolution = this.Config.Bind("Voxel", "Resolution", SEGI.VoxelResolution.High, "High or Low");
        Data.HalfResolution = this.Config.Bind("Voxel", "Half Resolution", true, "true or false");
        Data.VoxelSpaceSize = this.Config.Bind("Voxel", "Space Size", 25f, "1.0 to 100.0");
        Data.VoxelAntiAliasing = this.Config.Bind("Voxel", "Anti Aliasing", true, "true or false");

        // Occlusion
        Data.InnerOcclusionLayers = this.Config.Bind("Occlusion", "Inner Occlusion Layers", 1, "0 to 2");
        Data.OcclusionPower = this.Config.Bind("Occlusion", "Occlusion Power", 1f, "0.001 to 4.0");
        Data.OcclusionStrenth = this.Config.Bind("Occlusion", "Occlusion Strenth", 1f, "0.0 to 4.0");
        Data.SecondaryOcclusionStrenth = this.Config.Bind("Occlusion", "Secondary Occlusion Strenth", 1f, "0.1 to 4.0");
        Data.NearOcclusionStrenth = this.Config.Bind("Occlusion", "Near Occlusion Strenth", 0.5f, "0 to 4.0");
        Data.FarOcclusionStrenth = this.Config.Bind("Occlusion", "Far Occlusion Strenth", 1f, "0.1 to 4.0");
        Data.FarthestOcclusionStrenth = this.Config.Bind("Occlusion", "Farthest Occlusion Strenth", 1f, "0.1 to 4.0");

        // Reflection
        Data.DoReflections = this.Config.Bind("Refections", "Do Reflections", true, "true or false");
        Data.InfiniteBounces = this.Config.Bind("Refections", "Infinite Bounces", true, "true or false");
        Data.ReflectionSteps = this.Config.Bind("Refections", "Reflection Steps", 32, "12 to 128");
        Data.ReflectionOcclusionPower = this.Config.Bind("Refections", "Reflection Occlusion Power", 1f, "0.001 to 4.0");
        Data.SecondaryBounceGain = this.Config.Bind("Refections", "Secondary Bounce Gain", 0.75f, "0.1 to 4.0");
        Data.SkyReflectionIntensity = this.Config.Bind("Refections", "Sky Reflection Intensity", 0.5f, "0.0 to 1.0f");

        // Cones
        Data.Cones = this.Config.Bind("Cones", "Cones", 6, "1 to 128");
        Data.SecondaryCones = this.Config.Bind("Cones", "Secondary Cones", 3, "3 to 16");
        Data.ConeTraceSteps = this.Config.Bind("Cones", "Cone Trace Steps", 14, "1 to 32");
        Data.ConeTraceBias = this.Config.Bind("Cones", "Cone Trace Bias", 1f, "0.0 to 4.0");
        Data.ConeLength = this.Config.Bind("Cones", "Cone Length", 1f, "0.1 to 2.0");
        Data.ConeWidth = this.Config.Bind("Cones", "Cone Width", 2.25f, "0.5 to 6.0");

        // Light
        Data.NearLightGain = this.Config.Bind("Light", "Near Light Gain", 1f, "0.0 to 4.0");
        Data.GIGain = this.Config.Bind("Light", "Global Illumination Gain", 0.5f, "0.0 to 4.0");
        Data.ShadowSpaceSize = this.Config.Bind("Light", "Shadow Space Size", 1f, "1.0 to 100.0");

        // Sampling & Filtering
        Data.GaussianMipFilter = this.Config.Bind("Sampling & Filtering", "Gaussian Mip Filter", true, "true or false");
        Data.UseBilateralFiltering =
            this.Config.Bind("Sampling & Filtering", "Use Bilateral Filtering", true, "true or false");
        Data.StochasticSampling = this.Config.Bind("Sampling & Filtering", "Stochastic Sampling", true, "true or false");
        Data.TemporalBlendWeight = this.Config.Bind("Sampling & Filtering", "Temporal Blend Weight", 0.1f, "0.01 to 1.0");
    }

    public static async UniTask OnBaseLoaded() {
        // Wait until game has loaded into main menu
        await UniTask.WaitUntil(() => MainMenu.Instance.IsVisible);

        // Print version after main menu is visible
        LogInfo("is installed.");

        EnableSEGI();
    }

    public static void EnableSEGI() {
        SEGIGameObject = GameObject.Find("SEGIManager") ?? new GameObject("SEGIManager");
        _ = SEGIGameObject.AddComponent<SEGIManager>();
        DontDestroyOnLoad(SEGIGameObject);
    }


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
    public const string ModVersion = "1.0.5";
    public const string ModHandle = "3281346086";

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