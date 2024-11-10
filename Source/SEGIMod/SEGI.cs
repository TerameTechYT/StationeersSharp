#region

using System;
using UnityEngine;
using Resources = SEGI.Properties.Resources;

#endregion

namespace SEGI;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Sonic Ether/SEGI")]
public class SEGI : MonoBehaviour {
    [Flags]
    public enum VoxelResolution {
        Low = 128,
        High = 256
    }

    private static AssetBundle _bundle;

    [Range(0.1f, 2.0f)] public float coneLength = 1.0f;

    [Range(1, 128)] public int cones = 6;

    [Range(0.0f, 4.0f)] public float coneTraceBias = 1.0f;

    [Range(1, 32)] public int coneTraceSteps = 14;

    [Range(0.5f, 6.0f)] public float coneWidth = 5.5f;

    public bool doReflections = true;


    [Range(0.1f, 4.0f)] public float farOcclusionStrength = 1.0f;

    [Range(0.1f, 4.0f)] public float farthestOcclusionStrength = 1.0f;

    public Transform followTransform;

    public bool gaussianMipFilter;
    public LayerMask giCullingMask = 2147483647;

    [Range(0.0f, 4.0f)] public float giGain = 1.0f;

    public bool halfResolution = true;
    public bool infiniteBounces;

    [Range(0, 2)] public int innerOcclusionLayers = 1;

    [Range(0.0f, 4.0f)] public float nearLightGain = 1.0f;

    [Range(0.0f, 4.0f)] public float nearOcclusionStrength = 0.5f;

    [Range(0.001f, 4.0f)] public float occlusionPower = 1.5f;

    [Range(0.0f, 4.0f)] public float occlusionStrength = 1.0f;

    [Range(0.001f, 4.0f)] public float reflectionOcclusionPower = 1.0f;

    [Range(12, 128)] public int reflectionSteps = 64;

    [Range(0.0f, 4.0f)] public float secondaryBounceGain = 1.0f;

    [Range(3, 16)] public int secondaryCones = 6;

    [Range(0.1f, 4.0f)] public float secondaryOcclusionStrength = 1.0f;

    public float shadowSpaceSize = 50.0f;

    public Color skyColor;

    [Range(0.0f, 8.0f)] public float skyIntensity = 1.0f;

    [Range(0.0f, 1.0f)] public float skyReflectionIntensity = 1.0f;

    [Range(0.0f, 16.0f)] public float softSunlight = 0.0f;

    public bool sphericalSkylight;
    public bool stochasticSampling = true;
    public Light sun;


    [Range(0.01f, 1.0f)] public float temporalBlendWeight = 0.1f;

    public bool updateGI = true;

    public bool useBilateralFiltering;
    public bool visualizeGI;

    public bool visualizeSunDepthTexture;
    public bool visualizeVoxels;

    public bool voxelAA;


    public VoxelResolution voxelResolution = VoxelResolution.High;

    [Range(1.0f, 100.0f)] public float voxelSpaceSize = 25.0f;

    public static AssetBundle Bundle => _bundle ??= AssetBundle.LoadFromMemory(Resources.SEGI);

    #region InternalVariables
    private RenderTexture sunDepthTexture;
    private RenderTexture previousGIResult;
    private RenderTexture previousCameraDepth;

    /// <summary>
    ///     This is a volume texture that is immediately written to in the voxelization shader. The RInt format enables
    ///     atomic writes to avoid issues where multiple fragments are trying to write to the same voxel in the volume.
    /// </summary>
    private RenderTexture integerVolume;

    /// <summary>
    ///     An array of volume textures where each element is a mip/LOD level. Each volume is half the resolution of the
    ///     previous volume. Separate textures for each mip level are required for manual mip-mapping of the main GI volume
    ///     texture.
    /// </summary>
    private RenderTexture[] volumeTextures;

    /// <summary>
    ///     The secondary volume texture that holds irradiance calculated during the in-volume GI tracing that occurs when
    ///     Infinite Bounces is enabled.
    /// </summary>
    private RenderTexture secondaryIrradianceVolume;

    /// <summary>
    ///     The alternate mip level 0 main volume texture needed to avoid simultaneous read/write errors while performing
    ///     temporal stabilization on the main voxel volume.
    /// </summary>
    private RenderTexture volumeTextureB;

    /// <summary>
    ///     A 2D texture with the size of [voxel resolution, voxel resolution] that must be used as the active render
    ///     texture when rendering the scene for voxelization. This texture scales depending on whether Voxel AA is enabled to
    ///     ensure correct voxelization.
    /// </summary>
    private RenderTexture dummyVoxelTextureAAScaled;

    /// <summary>
    ///     A 2D texture with the size of [voxel resolution, voxel resolution] that must be used as the active render
    ///     texture when rendering the scene for voxelization. This texture is always the same size whether Voxel AA is enabled
    ///     or not.
    /// </summary>
    private RenderTexture dummyVoxelTextureFixed;

    private enum RenderState {
        Voxelize,
        Bounce
    }

    #endregion

    #region SupportingObjectsAndProperties

    private struct Pass {
        public static int DiffuseTrace;
        public const int BilateralBlur = 1;
        public const int BlendWithScene = 2;
        public const int TemporalBlend = 3;
        public const int SpecularTrace = 4;
        public const int GetCameraDepthTexture = 5;
        public const int GetWorldNormals = 6;
        public const int VisualizeGI = 7;
        public static int WriteBlack = 8;
        public const int VisualizeVoxels = 10;
        public const int BilateralUpsample = 11;
    }

    public struct SystemSupported : IEquatable<SystemSupported> {
        public bool hdrTextures;
        public bool rIntTextures;
        public bool dx11;
        public bool volumeTextures;
        public bool postShader;
        public bool sunDepthShader;
        public bool voxelizationShader;
        public bool tracingShader;

        public bool fullFunctionality => this.hdrTextures && this.rIntTextures && this.dx11 && this.volumeTextures && this.postShader &&
                                         this.sunDepthShader && this.voxelizationShader && this.tracingShader;

        public override bool Equals(object obj) => throw new NotImplementedException();

        public override int GetHashCode() => throw new NotImplementedException();

        public static bool operator ==(SystemSupported left, SystemSupported right) => left.Equals(right);

        public static bool operator !=(SystemSupported left, SystemSupported right) => !(left == right);

        public bool Equals(SystemSupported other) => throw new NotImplementedException();
    }

    /// <summary>
    ///     Contains info on system compatibility of required hardware functionality
    /// </summary>
    public SystemSupported systemSupported;

    /// <summary>
    ///     Estimates the VRAM usage of all the render textures used to render GI.
    /// </summary>
    public float vramUsage {
        get {
            long v = 0;

            if (this.sunDepthTexture != null)
                v += this.sunDepthTexture.width * this.sunDepthTexture.height * 16;

            if (this.previousGIResult != null)
                v += this.previousGIResult.width * this.previousGIResult.height * 16 * 4;

            if (this.previousCameraDepth != null)
                v += this.previousCameraDepth.width * this.previousCameraDepth.height * 32;

            if (this.integerVolume != null)
                v += this.integerVolume.width * this.integerVolume.height * this.integerVolume.volumeDepth * 32;

            if (this.volumeTextures != null) {
                for (int i = 0; i < this.volumeTextures.Length; i++) {
                    if (this.volumeTextures[i] != null) {
                        v += this.volumeTextures[i].width * this.volumeTextures[i].height * this.volumeTextures[i].volumeDepth * 16 *
                             4;
                    }
                }
            }

            if (this.secondaryIrradianceVolume != null) {
                v += this.secondaryIrradianceVolume.width * this.secondaryIrradianceVolume.height *
                     this.secondaryIrradianceVolume.volumeDepth * 16 * 4;
            }

            if (this.volumeTextureB != null)
                v += this.volumeTextureB.width * this.volumeTextureB.height * this.volumeTextureB.volumeDepth * 16 * 4;

            if (this.dummyVoxelTextureAAScaled != null)
                v += this.dummyVoxelTextureAAScaled.width * this.dummyVoxelTextureAAScaled.height * 8;

            if (this.dummyVoxelTextureFixed != null)
                v += this.dummyVoxelTextureFixed.width * this.dummyVoxelTextureFixed.height * 8;

            float vram = v / 8388608.0f;

            return vram;
        }
    }

    #endregion
}