// ReSharper disable InconsistentNaming

#pragma warning disable CA2243

using Assets.Scripts;
using BepInEx;
using BepInEx.Configuration;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using JetBrains.Annotations;
using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace SEGI;

[BepInPlugin(Data.Guid, Data.Name, Data.Version)]
[BepInProcess("rocketstation.exe")]
public class SEGIPlugin : BaseUnityPlugin
{
    public static SEGIPlugin Instance { get; private set; }
    public static Harmony HarmonyInstance { get; private set; }

    public static GameObject SEGIGameObject { get; private set; }
    public static SEGIManager SEGIManagerInstance { get; private set; }

    [UsedImplicitly]
    public void Awake()
    {
        Logger.LogInfo(Data.Name + " successfully loaded!");
        Instance = this;
        HarmonyInstance = new Harmony(Data.Guid);
        HarmonyInstance.PatchAll();
        Logger.LogInfo(Data.Name + " successfully patched!");

        Logger.LogInfo(Data.Name + " loading segi object...");
        SEGIGameObject = new GameObject("SEGIManager");
        DontDestroyOnLoad(SEGIGameObject);
        SEGIManagerInstance = SEGIGameObject.AddComponent<SEGIManager>();
        Logger.LogInfo(Data.Name + " loaded segi object!");

        Logger.LogInfo(Data.Name + " loading segi settings...");
        LoadConfiguration();
        Logger.LogInfo(Data.Name + " loaded segi settings!");

        // Thx jixxed for awesome code :)

        SceneManager.sceneLoaded += (scene, _) =>
        {
            if (scene.name == "Base")
            {
                CheckVersion().Forget();

                SEGIManagerInstance.SetSEGIEnabled(true);
            }
        };
    }

    public void LoadConfiguration()
    {
        // Voxel
        Data.VoxelResolution = Config.Bind("Voxel", "Resolution", SEGI.VoxelResolution.High, "High or Low");
        Data.HalfResolution = Config.Bind("Voxel", "Half Resolution", false, "true or false");
        Data.VoxelSpaceSize = Config.Bind("Voxel", "Space Size", 25f, "1.0 to 100.0");
        Data.VoxelAntiAliasing = Config.Bind("Voxel", "Anti Aliasing", false, "true or false");

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
        Data.InfiniteBounces = Config.Bind("Refections", "Infinite Bounces", false, "true or false");
        Data.ReflectionSteps = Config.Bind("Refections", "Reflection Steps", 64, "12 to 128");
        Data.ReflectionOcclusionPower = Config.Bind("Refections", "Reflection Occlusion Power", 1f, "0.001 to 4.0");
        Data.SecondaryBounceGain = Config.Bind("Refections", "Secondary Bounce Gain", 1f, "0.1 to 4.0");
        Data.SkyReflectionIntensity = Config.Bind("Refections", "Sky Reflection Intensity", 1f, "0.0 to 1.0f");

        // Cones
        Data.Cones = Config.Bind("Cones", "Cones", 6, "1 to 128");
        Data.SecondaryCones = Config.Bind("Cones", "Secondary Cones", 6, "3 to 16");
        Data.ConeTraceSteps = Config.Bind("Cones", "Cone Trace Steps", 14, "1 to 32");
        Data.ConeTraceBias = Config.Bind("Cones", "Cone Trace Bias", 1f, "0.0 to 4.0");
        Data.ConeLength = Config.Bind("Cones", "Cone Length", 1f, "0.1 to 2.0");
        Data.ConeWidth = Config.Bind("Cones", "Cone Width", 5.5f, "0.5 to 6.0");

        // Light
        Data.NearLightGain = Config.Bind("Light", "Near Light Gain", 1f, "0.0 to 4.0");
        Data.GIGain = Config.Bind("Light", "Global Illumination Gain", 1f, "0.0 to 4.0");
        Data.ShadowSpaceSize = Config.Bind("Light", "Shadow Space Size", 1f, "1.0 to 100.0");

        // Sampling & Filtering
        Data.GaussianMipFilter = Config.Bind("Sampling & Filtering", "Gaussian Mip Filter", false, "true or false");
        Data.UseBilateralFiltering = Config.Bind("Sampling & Filtering", "Use Bilateral Filtering", false, "true or false");
        Data.StochasticSampling = Config.Bind("Sampling & Filtering", "Stochastic Sampling", true, "true or false");
        Data.TemporalBlendWeight = Config.Bind("Sampling & Filtering", "Temporal Blend Weight", 0.1f, "0.01 to 1.0");

    }

    public async UniTaskVoid CheckVersion()
    {
        UnityWebRequest webRequest = await UnityWebRequest.Get(new Uri(Data.GitVersion)).SendWebRequest();
        Logger.LogInfo("Awaiting send web request...");

        string currentVersion = webRequest.downloadHandler.text.Trim();
        Logger.LogInfo("Await complete!");

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            Logger.LogInfo($"Latest version is {currentVersion}. Installed {Data.Version}");
            ConsoleWindow.Print($"[{Data.Name}]: v{Data.Version} is installed.");

            if (Data.Version == currentVersion)
            {
                return;
            }

            Logger.LogInfo("User does not have latest version, printing to console.");
            ConsoleWindow.PrintAction($"[{Data.Name}]: New version v{currentVersion} is available");
        }
        else
        {
            Logger.LogError(
                $"Failed to request latest version. Result: {webRequest.result} Error: '\"{webRequest.error}\""
            );
            ConsoleWindow.PrintError($"[{Data.Name}]: Failed to request latest version! Check log for more info.");
        }

        webRequest.Dispose();
    }
}

internal struct Data
{
    public const string Guid = "segimod";
    public const string Name = "SEGIMod";
    public const string Version = "1.0.0";
    public const string WorkshopHandle = "";
    public const string GitRaw = "https://raw.githubusercontent.com/TerameTechYT/RocketMods/development/Source/";
    public const string GitVersion = GitRaw + Name + "/VERSION";

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