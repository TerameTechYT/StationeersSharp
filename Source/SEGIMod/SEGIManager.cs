#region

#endregion

namespace SEGI;

public class SEGIManager : MonoBehaviour {
    public SEGI SEGIInstance {
        get; private set;
    }

    [UsedImplicitly]
    private void Awake() => SEGIInstance = Camera.main.gameObject.AddComponent<SEGI>();

    [UsedImplicitly]
    private void Update() {
        SEGIInstance.enabled = Data.Enabled?.Value ?? false;

        if (SEGIInstance != null && SEGIInstance.enabled) {
            SEGIInstance.sun = WorldManager.Instance.WorldSun.TargetLight;

            // Voxel
            SEGIInstance.voxelResolution = Data.VoxelResolution.Value;
            SEGIInstance.halfResolution = Data.HalfResolution.Value;
            SEGIInstance.voxelSpaceSize = Data.VoxelSpaceSize.Value;
            SEGIInstance.voxelAA = Data.VoxelAntiAliasing.Value;

            // Occlusion
            SEGIInstance.innerOcclusionLayers = Data.InnerOcclusionLayers.Value;
            SEGIInstance.occlusionPower = Data.OcclusionPower.Value;
            SEGIInstance.occlusionStrength = Data.OcclusionStrenth.Value;
            SEGIInstance.secondaryOcclusionStrength = Data.SecondaryOcclusionStrenth.Value;
            SEGIInstance.nearOcclusionStrength = Data.NearOcclusionStrenth.Value;
            SEGIInstance.farOcclusionStrength = Data.FarOcclusionStrenth.Value;
            SEGIInstance.farthestOcclusionStrength = Data.FarthestOcclusionStrenth.Value;

            // Reflection
            SEGIInstance.doReflections = Data.DoReflections.Value;
            SEGIInstance.infiniteBounces = Data.InfiniteBounces.Value;
            SEGIInstance.reflectionSteps = Data.ReflectionSteps.Value;
            SEGIInstance.reflectionOcclusionPower = Data.ReflectionOcclusionPower.Value;
            SEGIInstance.secondaryBounceGain = Data.SecondaryBounceGain.Value;
            SEGIInstance.skyReflectionIntensity = Data.SkyReflectionIntensity.Value;

            // Cones
            SEGIInstance.cones = Data.Cones.Value;
            SEGIInstance.secondaryCones = Data.SecondaryCones.Value;
            SEGIInstance.coneTraceSteps = Data.ConeTraceSteps.Value;
            SEGIInstance.coneTraceBias = Data.ConeTraceBias.Value;
            SEGIInstance.coneLength = Data.ConeLength.Value;
            SEGIInstance.coneWidth = Data.ConeWidth.Value;

            // Light
            SEGIInstance.nearLightGain = Data.NearLightGain.Value;
            SEGIInstance.giGain = Data.GIGain.Value;
            SEGIInstance.shadowSpaceSize = Data.ShadowSpaceSize.Value;

            // Sampling & Filtering
            SEGIInstance.gaussianMipFilter = Data.GaussianMipFilter.Value;
            SEGIInstance.useBilateralFiltering = Data.UseBilateralFiltering.Value;
            SEGIInstance.stochasticSampling = Data.StochasticSampling.Value;
            SEGIInstance.temporalBlendWeight = Data.TemporalBlendWeight.Value;
        }
    }
}