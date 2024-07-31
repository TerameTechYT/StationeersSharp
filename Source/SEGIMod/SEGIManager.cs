using JetBrains.Annotations;
using UnityEngine;

namespace SEGI;
public class SEGIManager : MonoBehaviour {
    public SEGI SEGInstance {
        get; private set;
    }

    [UsedImplicitly]
    private void Awake() => this.SEGInstance = Camera.main.gameObject.AddComponent<SEGI>();

    [UsedImplicitly]
    private void Update() {
        if (this.SEGInstance != null && this.SEGInstance.enabled) {
            this.SEGInstance.sun = WorldManager.Instance.WorldSun.TargetLight;

            // Voxel
            this.SEGInstance.voxelResolution = Data.VoxelResolution.Value;
            this.SEGInstance.halfResolution = Data.HalfResolution.Value;
            this.SEGInstance.voxelSpaceSize = Data.VoxelSpaceSize.Value;
            this.SEGInstance.voxelAA = Data.VoxelAntiAliasing.Value;

            // Occlusion
            this.SEGInstance.innerOcclusionLayers = Data.InnerOcclusionLayers.Value;
            this.SEGInstance.occlusionPower = Data.OcclusionPower.Value;
            this.SEGInstance.occlusionStrength = Data.OcclusionStrenth.Value;
            this.SEGInstance.secondaryOcclusionStrength = Data.SecondaryOcclusionStrenth.Value;
            this.SEGInstance.nearOcclusionStrength = Data.NearOcclusionStrenth.Value;
            this.SEGInstance.farOcclusionStrength = Data.FarOcclusionStrenth.Value;
            this.SEGInstance.farthestOcclusionStrength = Data.FarthestOcclusionStrenth.Value;

            // Reflection
            this.SEGInstance.doReflections = Data.DoReflections.Value;
            this.SEGInstance.infiniteBounces = Data.InfiniteBounces.Value;
            this.SEGInstance.reflectionSteps = Data.ReflectionSteps.Value;
            this.SEGInstance.reflectionOcclusionPower = Data.ReflectionOcclusionPower.Value;
            this.SEGInstance.secondaryBounceGain = Data.SecondaryBounceGain.Value;
            this.SEGInstance.skyReflectionIntensity = Data.SkyReflectionIntensity.Value;

            // Cones
            this.SEGInstance.cones = Data.Cones.Value;
            this.SEGInstance.secondaryCones = Data.SecondaryCones.Value;
            this.SEGInstance.coneTraceSteps = Data.ConeTraceSteps.Value;
            this.SEGInstance.coneTraceBias = Data.ConeTraceBias.Value;
            this.SEGInstance.coneLength = Data.ConeLength.Value;
            this.SEGInstance.coneWidth = Data.ConeWidth.Value;

            // Light
            this.SEGInstance.nearLightGain = Data.NearLightGain.Value;
            this.SEGInstance.giGain = Data.GIGain.Value;
            this.SEGInstance.shadowSpaceSize = Data.ShadowSpaceSize.Value;

            // Sampling & Filtering
            this.SEGInstance.gaussianMipFilter = Data.GaussianMipFilter.Value;
            this.SEGInstance.useBilateralFiltering = Data.UseBilateralFiltering.Value;
            this.SEGInstance.stochasticSampling = Data.StochasticSampling.Value;
            this.SEGInstance.temporalBlendWeight = Data.TemporalBlendWeight.Value;
        }
    }
}