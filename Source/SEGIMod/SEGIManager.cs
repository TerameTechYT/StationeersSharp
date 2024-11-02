#region

using JetBrains.Annotations;
using UnityEngine;

#endregion

namespace SEGI;

public class SEGIManager : MonoBehaviour {
    public SEGI SEGInstance { get; private set; }

    [UsedImplicitly]
    private void Awake() {
        SEGInstance = Camera.main.gameObject.AddComponent<SEGI>();
    }

    [UsedImplicitly]
    private void Update() {
        if (SEGInstance != null && SEGInstance.enabled) {
            SEGInstance.sun = WorldManager.Instance.WorldSun.TargetLight;

            // Voxel
            SEGInstance.voxelResolution = Data.VoxelResolution.Value;
            SEGInstance.halfResolution = Data.HalfResolution.Value;
            SEGInstance.voxelSpaceSize = Data.VoxelSpaceSize.Value;
            SEGInstance.voxelAA = Data.VoxelAntiAliasing.Value;

            // Occlusion
            SEGInstance.innerOcclusionLayers = Data.InnerOcclusionLayers.Value;
            SEGInstance.occlusionPower = Data.OcclusionPower.Value;
            SEGInstance.occlusionStrength = Data.OcclusionStrenth.Value;
            SEGInstance.secondaryOcclusionStrength = Data.SecondaryOcclusionStrenth.Value;
            SEGInstance.nearOcclusionStrength = Data.NearOcclusionStrenth.Value;
            SEGInstance.farOcclusionStrength = Data.FarOcclusionStrenth.Value;
            SEGInstance.farthestOcclusionStrength = Data.FarthestOcclusionStrenth.Value;

            // Reflection
            SEGInstance.doReflections = Data.DoReflections.Value;
            SEGInstance.infiniteBounces = Data.InfiniteBounces.Value;
            SEGInstance.reflectionSteps = Data.ReflectionSteps.Value;
            SEGInstance.reflectionOcclusionPower = Data.ReflectionOcclusionPower.Value;
            SEGInstance.secondaryBounceGain = Data.SecondaryBounceGain.Value;
            SEGInstance.skyReflectionIntensity = Data.SkyReflectionIntensity.Value;

            // Cones
            SEGInstance.cones = Data.Cones.Value;
            SEGInstance.secondaryCones = Data.SecondaryCones.Value;
            SEGInstance.coneTraceSteps = Data.ConeTraceSteps.Value;
            SEGInstance.coneTraceBias = Data.ConeTraceBias.Value;
            SEGInstance.coneLength = Data.ConeLength.Value;
            SEGInstance.coneWidth = Data.ConeWidth.Value;

            // Light
            SEGInstance.nearLightGain = Data.NearLightGain.Value;
            SEGInstance.giGain = Data.GIGain.Value;
            SEGInstance.shadowSpaceSize = Data.ShadowSpaceSize.Value;

            // Sampling & Filtering
            SEGInstance.gaussianMipFilter = Data.GaussianMipFilter.Value;
            SEGInstance.useBilateralFiltering = Data.UseBilateralFiltering.Value;
            SEGInstance.stochasticSampling = Data.StochasticSampling.Value;
            SEGInstance.temporalBlendWeight = Data.TemporalBlendWeight.Value;
        }
    }
}