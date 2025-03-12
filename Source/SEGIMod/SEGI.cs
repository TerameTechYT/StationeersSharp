#region
using SEGIResources = SEGI.Properties.Resources;
#endregion

namespace SEGI;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Sonic Ether/SEGI")]
public class SEGI : MonoBehaviour {
    private static AssetBundle _bundle;
    public static AssetBundle Bundle => _bundle ??= AssetBundle.LoadFromMemory(SEGIResources.SEGI);

    #region Parameters
    [Serializable]
    [Flags]
    public enum VoxelResolution {
        Potato = 32,
        VeryLow = 64,
        Low = 128,
        Medium = 256,
        High = 512,
        VeryHigh = 1024,
        Ultra = 2048,
    }

    public bool sphericalSkylight;
    public bool visualizeSunDepthTexture;
    public bool visualizeGI;
    public bool visualizeVoxels;

    public bool updateGI = true;
    public LayerMask giCullingMask = int.MaxValue;
    public Light sun;
    public Color skyColor;
    public Transform followTransform;

    #endregion

    #region InternalVariables
    private enum RenderState {
        Voxelize,
        Bounce
    }

    private RenderState renderState = RenderState.Voxelize;

    private bool initalized;
    private bool notReadyToRender;

    private const int mipLevels = 6;
    private int sunShadowResolution = 256;
    private int frameCounter;
    private int voxelFlipFlop;
    private int prevSunShadowResolution;

    private float shadowSpaceDepthRatio = 10.0f;
    private float VoxelScaleFactor => (float) Data.VoxelResolution / 256.0f;

    private Material material;
    private Camera attachedCamera;
    private Transform shadowCameraTransform;
    private Camera shadowCamera;
    private GameObject shadowCameraGameObject;
    private Texture2D[] blueNoise;
    private Shader sunDepthShader;
    private RenderTexture sunDepthTexture;
    private RenderTexture previousGIResult;
    private RenderTexture previousCameraDepth;
    private RenderTexture integerVolume;
    private RenderTexture[] volumeTextures;
    private RenderTexture secondaryIrradianceVolume;
    private RenderTexture volumeTextureB;
    private RenderTexture activeVolume;
    private RenderTexture previousActiveVolume;
    private RenderTexture dummyVoxelTextureAAScaled;
    private RenderTexture dummyVoxelTextureFixed;

    private Shader voxelizationShader;
    private Shader voxelTracingShader;
    private ComputeShader clearCompute;
    private ComputeShader transferIntsCompute;
    private ComputeShader mipFilterCompute;

    private Camera voxelCamera;
    private GameObject voxelCameraGameObject;
    private GameObject leftViewPoint;
    private GameObject topViewPoint;
    private Vector3 voxelSpaceOrigin;
    private Vector3 previousVoxelSpaceOrigin;
    private Vector3 voxelSpaceOriginDelta;

    private Quaternion rotationFront = new(0.0f, 0.0f, 0.0f, 1.0f);
    private Quaternion rotationLeft = new(0.0f, 0.7f, 0.0f, 0.7f);
    private Quaternion rotationTop = new(0.7f, 0.0f, 0.0f, 0.7f);

    #endregion

    #region SupportingObjectsAndProperties

    private struct Pass {
        public static int DiffuseTrace;
        public static int BilateralBlur = 1;
        public static int BlendWithScene = 2;
        public static int TemporalBlend = 3;
        public static int SpecularTrace = 4;
        public static int GetCameraDepthTexture = 5;
        public static int GetWorldNormals = 6;
        public static int VisualizeGI = 7;
        public static int WriteBlack = 8;
        public static int VisualizeVoxels = 10;
        public static int BilateralUpsample = 11;
    }

    public SystemSupported systemSupported;
    public struct SystemSupported : IEquatable<SystemSupported> {
        public bool HDRTextures;
        public bool RIntTextures;
        public bool DirectX11;
        public bool VolumeTextures;
        public bool PostShader;
        public bool SunDepthShader;
        public bool VoxelizationShader;
        public bool TracingShader;

        public readonly bool FullFunctionality => HDRTextures && RIntTextures && DirectX11 && VolumeTextures && PostShader && SunDepthShader && VoxelizationShader && TracingShader;

        public override bool Equals(object obj) => obj is SystemSupported systemSupported && this.Equals(systemSupported);
        public bool Equals(SystemSupported other) => this.FullFunctionality == other.FullFunctionality;
        public override int GetHashCode() => throw new NotImplementedException();
        public static bool operator ==(SystemSupported left, SystemSupported right) => left.Equals(right);
        public static bool operator !=(SystemSupported left, SystemSupported right) => !left.Equals(right);
    }


    /// <summary>
    /// Estimates the VRAM usage of all the render textures used to render GI.
    /// </summary>
    public float VRamUsage {
        get {
            long vram = 0;

            if (sunDepthTexture != null) {
                vram += sunDepthTexture.width * sunDepthTexture.height * 16;
            }

            if (previousGIResult != null) {
                vram += previousGIResult.width * previousGIResult.height * 16 * 4;
            }

            if (previousCameraDepth != null) {
                vram += previousCameraDepth.width * previousCameraDepth.height * 32;
            }

            if (integerVolume != null) {
                vram += integerVolume.width * integerVolume.height * integerVolume.volumeDepth * 32;
            }

            if (volumeTextures != null) {
                for (int i = 0; i < volumeTextures.Length; i++) {
                    if (volumeTextures[i] != null) {
                        vram += volumeTextures[i].width * volumeTextures[i].height * volumeTextures[i].volumeDepth * 16 * 4;
                    }
                }
            }

            if (secondaryIrradianceVolume != null) {
                vram += secondaryIrradianceVolume.width * secondaryIrradianceVolume.height * secondaryIrradianceVolume.volumeDepth * 16 * 4;
            }

            if (volumeTextureB != null) {
                vram += volumeTextureB.width * volumeTextureB.height * volumeTextureB.volumeDepth * 16 * 4;
            }

            if (dummyVoxelTextureAAScaled != null) {
                vram += dummyVoxelTextureAAScaled.width * dummyVoxelTextureAAScaled.height * 8;
            }

            if (dummyVoxelTextureFixed != null) {
                vram += dummyVoxelTextureFixed.width * dummyVoxelTextureFixed.height * 8;
            }

            return vram / (8 * (1024 ^ 2));
        }
    }

    private int MipFilterKernel => Data.GaussianMipFilter ? 1 : 0;
    private int DummyVoxelResolution => (int) Data.VoxelResolution * (Data.VoxelAntiAliasing ? 2 : 1);
    private int GIRenderRes => Data.HalfResolution ? 2 : 1;

    #endregion

    [UsedImplicitly]
    private void Start() => InitCheck();

    [UsedImplicitly]
    private void InitCheck() {
        if (initalized) {
            return;
        }

        Init();
    }

    [UsedImplicitly]
    private void CreateVolumeTextures() {
        if (volumeTextures != null) {
            for (int i = 0; i < mipLevels; i++) {
                if (volumeTextures[i] != null) {
                    CleanupTexture(ref volumeTextures[i]);
                }
            }
        }

        volumeTextures = new RenderTexture[mipLevels];
        for (int i = 0; i < mipLevels; i++) {
            int resolution = (int) Data.VoxelResolution / Mathf.RoundToInt(Mathf.Pow(2, i));
            volumeTextures[i] = new(resolution, resolution, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear) {
                dimension = TextureDimension.Tex3D,
                volumeDepth = resolution,
                enableRandomWrite = true,
                filterMode = FilterMode.Bilinear,
                autoGenerateMips = false,
                useMipMap = false,
            };
            volumeTextures[i].Create();
            volumeTextures[i].hideFlags = HideFlags.HideAndDontSave;
        }

        if (volumeTextureB) {
            CleanupTexture(ref volumeTextureB);
        }

        volumeTextureB = new((int) Data.VoxelResolution, (int) Data.VoxelResolution, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear) {
            dimension = TextureDimension.Tex3D,
            volumeDepth = (int) Data.VoxelResolution,
            enableRandomWrite = true,
            filterMode = FilterMode.Bilinear,
            autoGenerateMips = false,
            useMipMap = false,
        };
        volumeTextureB.Create();
        volumeTextureB.hideFlags = HideFlags.HideAndDontSave;

        if (secondaryIrradianceVolume) {
            CleanupTexture(ref secondaryIrradianceVolume);
        }

        secondaryIrradianceVolume = new RenderTexture((int) Data.VoxelResolution, (int) Data.VoxelResolution, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear) {
            dimension = TextureDimension.Tex3D,
            volumeDepth = (int) Data.VoxelResolution,
            enableRandomWrite = true,
            filterMode = FilterMode.Point,
            autoGenerateMips = false,
            useMipMap = false,
            antiAliasing = 1,
        };
        secondaryIrradianceVolume.Create();
        secondaryIrradianceVolume.hideFlags = HideFlags.HideAndDontSave;

        if (integerVolume) {
            CleanupTexture(ref integerVolume);
        }

        integerVolume = new RenderTexture((int) Data.VoxelResolution, (int) Data.VoxelResolution, 0, RenderTextureFormat.RInt, RenderTextureReadWrite.Linear) {
            dimension = TextureDimension.Tex3D,
            volumeDepth = (int) Data.VoxelResolution,
            enableRandomWrite = true,
            filterMode = FilterMode.Point,
            hideFlags = HideFlags.HideAndDontSave
        };
        integerVolume.Create();
        integerVolume.hideFlags = HideFlags.HideAndDontSave;

        ResizeDummyTexture();
    }

    private void ResizeDummyTexture() {
        if (dummyVoxelTextureAAScaled) {
            CleanupTexture(ref dummyVoxelTextureAAScaled);
        }

        dummyVoxelTextureAAScaled = new RenderTexture(DummyVoxelResolution, DummyVoxelResolution, 0, RenderTextureFormat.R8);
        dummyVoxelTextureAAScaled.Create();
        dummyVoxelTextureAAScaled.hideFlags = HideFlags.HideAndDontSave;

        if (dummyVoxelTextureFixed) {
            CleanupTexture(ref dummyVoxelTextureFixed);
        }

        dummyVoxelTextureFixed = new RenderTexture((int) Data.VoxelResolution, (int) Data.VoxelResolution, 0, RenderTextureFormat.R8);
        dummyVoxelTextureFixed.Create();
        dummyVoxelTextureFixed.hideFlags = HideFlags.HideAndDontSave;
    }

    private void Init() {
        //Setup shaders and materials
        sunDepthShader = Bundle.LoadAsset<Shader>("SEGIRenderSunDepth");
        clearCompute = Bundle.LoadAsset<ComputeShader>("SEGIClear");
        transferIntsCompute = Bundle.LoadAsset<ComputeShader>("SEGITransferInts");
        mipFilterCompute = Bundle.LoadAsset<ComputeShader>("SEGIMipFilter");
        voxelizationShader = Bundle.LoadAsset<Shader>("SEGIVoxelizeScene");
        voxelTracingShader = Bundle.LoadAsset<Shader>("SEGITraceScene");

        material = new Material(Bundle.LoadAsset<Shader>("SEGI")) {
            hideFlags = HideFlags.HideAndDontSave
        };

        //Get the camera attached to this game object
        attachedCamera = this.GetComponent<Camera>();
        attachedCamera.depthTextureMode |= DepthTextureMode.Depth;
        attachedCamera.depthTextureMode |= DepthTextureMode.MotionVectors;

        //Find the proxy shadow rendering camera if it exists
        shadowCameraGameObject = GameObject.Find("SEGI_SHADOWCAM") ?? new GameObject("SEGI_SHADOWCAM") {
            hideFlags = HideFlags.HideAndDontSave
        };

        if (shadowCameraGameObject.GetComponent<Camera>()) {
            shadowCamera = shadowCameraGameObject.GetComponent<Camera>();
            shadowCameraTransform = shadowCameraGameObject.transform;
        }
        else {
            shadowCamera = shadowCameraGameObject.AddComponent<Camera>();
            shadowCamera.enabled = false;
            shadowCamera.depth = attachedCamera.depth - 1;
            shadowCamera.orthographic = true;
            shadowCamera.orthographicSize = Data.ShadowSpaceSize;
            shadowCamera.clearFlags = CameraClearFlags.SolidColor;
            shadowCamera.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
            shadowCamera.farClipPlane = Data.ShadowSpaceSize * 2.0f * shadowSpaceDepthRatio;
            shadowCamera.cullingMask = giCullingMask;
            shadowCamera.useOcclusionCulling = false;
            shadowCameraTransform = shadowCameraGameObject.transform;
        }

        //Create the proxy camera objects responsible for rendering the scene to voxelize the scene. If they already exist, destroy them
        voxelCameraGameObject = GameObject.Find("SEGI_VOXEL_CAMERA") ?? new GameObject("SEGI_VOXEL_CAMERA") {
            hideFlags = HideFlags.HideAndDontSave
        };

        if (voxelCameraGameObject.GetComponent<Camera>()) {
            voxelCamera = voxelCameraGameObject.GetComponent<Camera>();
        }
        else {
            voxelCamera = voxelCameraGameObject.AddComponent<Camera>();
            voxelCamera.enabled = false;
            voxelCamera.orthographic = true;
            voxelCamera.orthographicSize = Data.VoxelSpaceSize * 0.5f;
            voxelCamera.nearClipPlane = 0.0f;
            voxelCamera.farClipPlane = Data.VoxelSpaceSize;
            voxelCamera.depth = -2;
            voxelCamera.renderingPath = RenderingPath.Forward;
            voxelCamera.clearFlags = CameraClearFlags.Color;
            voxelCamera.backgroundColor = Color.black;
            voxelCamera.useOcclusionCulling = false;
        }

        leftViewPoint = GameObject.Find("SEGI_LEFT_VOXEL_VIEW") ?? new GameObject("SEGI_LEFT_VOXEL_VIEW") {
            hideFlags = HideFlags.HideAndDontSave
        };

        topViewPoint = GameObject.Find("SEGI_TOP_VOXEL_VIEW") ?? new GameObject("SEGI_TOP_VOXEL_VIEW") {
            hideFlags = HideFlags.HideAndDontSave
        };

        //Get blue noise textures
        blueNoise = null;
        blueNoise = new Texture2D[64];
        for (int i = 0; i < 64; i++) {
            string fileName = "LDR_RGBA_" + i.ToString();
            Texture2D blueNoiseTexture = Bundle.LoadAsset<Texture2D>(fileName);

            if (blueNoiseTexture == null) {
                Plugin.LogWarning("Unable to find noise texture \"Assets/SEGI/Resources/Noise Textures/" + fileName + "\" for SEGI!");
            }

            blueNoise[i] = blueNoiseTexture;
        }

        //Setup sun depth texture
        if (sunDepthTexture) {
            CleanupTexture(ref sunDepthTexture);
        }

        sunDepthTexture = new RenderTexture(sunShadowResolution, sunShadowResolution, 16, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear) {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point,
        };
        sunDepthTexture.Create();
        sunDepthShader.hideFlags = HideFlags.HideAndDontSave;
        //Create the volume textures
        CreateVolumeTextures();

        initalized = true;
    }

    [UsedImplicitly]
    private void CheckSupport() {
        systemSupported.HDRTextures = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);
        systemSupported.RIntTextures = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RInt);
        systemSupported.DirectX11 = SystemInfo.graphicsShaderLevel >= 50 && SystemInfo.supportsComputeShaders;
        systemSupported.VolumeTextures = SystemInfo.supports3DTextures;

        systemSupported.PostShader = material.shader.isSupported;
        systemSupported.SunDepthShader = sunDepthShader.isSupported;
        systemSupported.VoxelizationShader = voxelizationShader.isSupported;
        systemSupported.TracingShader = voxelTracingShader.isSupported;

        if (!systemSupported.FullFunctionality) {
            Plugin.LogWarning("SEGI is not supported on the current platform.");
            enabled = false;

            DestroyImmediate(this);
        }
    }

    [UsedImplicitly]
    private void OnDrawGizmosSelected() {
        if (!enabled) {
            return;
        }

        Color prevColor = Gizmos.color;
        Gizmos.color = new Color(1.0f, 0.25f, 0.0f, 0.5f);
        Gizmos.DrawCube(voxelSpaceOrigin, new Vector3(Data.VoxelSpaceSize, Data.VoxelSpaceSize, Data.VoxelSpaceSize));
        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.1f);
        Gizmos.color = prevColor;
    }

    [UsedImplicitly]
    private void CleanupTexture(ref RenderTexture texture) {
        if (texture) {
            texture.DiscardContents();
            texture.Release();
            DestroyImmediate(texture);
        }
    }

    [UsedImplicitly]
    private void CleanupTextures() {
        CleanupTexture(ref sunDepthTexture);
        CleanupTexture(ref previousGIResult);
        CleanupTexture(ref previousCameraDepth);
        CleanupTexture(ref integerVolume);

        for (int i = 0; i < volumeTextures.Length; i++) {
            CleanupTexture(ref volumeTextures[i]);
        }

        CleanupTexture(ref secondaryIrradianceVolume);
        CleanupTexture(ref volumeTextureB);
        CleanupTexture(ref dummyVoxelTextureAAScaled);
        CleanupTexture(ref dummyVoxelTextureFixed);
    }

    [UsedImplicitly]
    private void Cleanup() {
        DestroyImmediate(material);
        DestroyImmediate(voxelCameraGameObject);
        DestroyImmediate(leftViewPoint);
        DestroyImmediate(topViewPoint);
        DestroyImmediate(shadowCameraGameObject);
        initalized = false;

        CleanupTextures();
    }

    [UsedImplicitly]
    private void OnEnable() {
        InitCheck();
        ResizeRenderTextures();

        CheckSupport();
    }

    [UsedImplicitly]
    private void OnDisable() => Cleanup();

    [UsedImplicitly]
    private void ResizeRenderTextures() {
        if (previousGIResult) {
            CleanupTexture(ref previousGIResult);
        }

        int width = attachedCamera.pixelWidth == 0 ? 2 : attachedCamera.pixelWidth;
        int height = attachedCamera.pixelHeight == 0 ? 2 : attachedCamera.pixelHeight;

        previousGIResult = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf) {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            useMipMap = true,
            autoGenerateMips = false,
        };
        previousGIResult.Create();
        previousGIResult.hideFlags = HideFlags.HideAndDontSave;

        if (previousCameraDepth) {
            CleanupTexture(ref previousCameraDepth);
        }

        previousCameraDepth = new RenderTexture(width, height, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear) {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
        };
        previousCameraDepth.Create();
        previousCameraDepth.hideFlags = HideFlags.HideAndDontSave;
    }

    private void ResizeSunShadowBuffer() {
        if (sunDepthTexture) {
            CleanupTexture(ref sunDepthTexture);
        }

        sunDepthTexture = new RenderTexture(sunShadowResolution, sunShadowResolution, 16, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear) {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point,
        };
        sunDepthTexture.Create();
        sunDepthShader.hideFlags = HideFlags.HideAndDontSave;
    }

    [UsedImplicitly]
    private void Update() {
        if (notReadyToRender) {
            return;
        }

        if (previousGIResult == null) {
            ResizeRenderTextures();
        }

        if (previousGIResult.width != attachedCamera.pixelWidth || previousGIResult.height != attachedCamera.pixelHeight) {
            ResizeRenderTextures();
        }

        if (sunShadowResolution != prevSunShadowResolution) {
            ResizeSunShadowBuffer();
        }

        prevSunShadowResolution = sunShadowResolution;

        if (volumeTextures[0].width != (int) Data.VoxelResolution) {
            CreateVolumeTextures();
        }

        if (dummyVoxelTextureAAScaled.width != DummyVoxelResolution) {
            ResizeDummyTexture();
        }
    }

    private Matrix4x4 TransformViewMatrix(Matrix4x4 mat) {
        //Since the third column of the view matrix needs to be reversed if using reversed z-buffer, do so here
        if (SystemInfo.usesReversedZBuffer) {
            mat[2, 0] = -mat[2, 0];
            mat[2, 1] = -mat[2, 1];
            mat[2, 2] = -mat[2, 2];
            mat[2, 3] = -mat[2, 3];
        }

        return mat;
    }

    [UsedImplicitly]
    private void OnPreRender() {
        //Force reinitialization to make sure that everything is working properly if one of the cameras was unexpectedly destroyed
        if (!voxelCamera || !shadowCamera) {
            initalized = false;
        }

        InitCheck();

        if (notReadyToRender) {
            return;
        }

        if (!updateGI) {
            return;
        }

        //Cache the previous active render texture to avoid issues with other Unity rendering going on
        RenderTexture previousActive = RenderTexture.active;

        Shader.SetGlobalInt("SEGIVoxelAA", Data.VoxelAntiAliasing ? 1 : 0);

        //Main voxelization work
        if (renderState == RenderState.Voxelize) {
            activeVolume = voxelFlipFlop == 0 ? volumeTextures[0] : volumeTextureB;             //Flip-flopping volume textures to avoid simultaneous read and write errors in shaders
            previousActiveVolume = voxelFlipFlop == 0 ? volumeTextureB : volumeTextures[0];

            //Setup the voxel volume origin position
            float interval = Data.VoxelSpaceSize / 8.0f;                                             //The interval at which the voxel volume will be "locked" in world-space
            Vector3 origin;
            if (followTransform) {
                origin = followTransform.position;
            }
            else {
                //GI is still flickering a bit when the scene view and the game view are opened at the same time
                origin = transform.position + (transform.forward * Data.VoxelSpaceSize / 4.0f);
            }
            //Lock the voxel volume origin based on the interval
            voxelSpaceOrigin = new Vector3(Mathf.Round(origin.x / interval) * interval, Mathf.Round(origin.y / interval) * interval, Mathf.Round(origin.z / interval) * interval);

            //Calculate how much the voxel origin has moved since last voxelization pass. Used for scrolling voxel data in shaders to avoid ghosting when the voxel volume moves in the world
            voxelSpaceOriginDelta = voxelSpaceOrigin - previousVoxelSpaceOrigin;
            Shader.SetGlobalVector("SEGIVoxelSpaceOriginDelta", voxelSpaceOriginDelta / Data.VoxelSpaceSize);

            previousVoxelSpaceOrigin = voxelSpaceOrigin;

            //Set the voxel camera (proxy camera used to render the scene for voxelization) parameters
            voxelCamera.enabled = false;
            voxelCamera.orthographic = true;
            voxelCamera.orthographicSize = Data.VoxelSpaceSize * 0.5f;
            voxelCamera.nearClipPlane = 0.0f;
            voxelCamera.farClipPlane = Data.VoxelSpaceSize;
            voxelCamera.depth = -2;
            voxelCamera.renderingPath = RenderingPath.Forward;
            voxelCamera.clearFlags = CameraClearFlags.Color;
            voxelCamera.backgroundColor = Color.black;
            voxelCamera.cullingMask = giCullingMask;

            //Move the voxel camera game object and other related objects to the above calculated voxel space origin
            voxelCameraGameObject.transform.position = voxelSpaceOrigin - (Vector3.forward * Data.VoxelSpaceSize * 0.5f);
            voxelCameraGameObject.transform.rotation = rotationFront;

            leftViewPoint.transform.position = voxelSpaceOrigin + (Vector3.left * Data.VoxelSpaceSize * 0.5f);
            leftViewPoint.transform.rotation = rotationLeft;
            topViewPoint.transform.position = voxelSpaceOrigin + (Vector3.up * Data.VoxelSpaceSize * 0.5f);
            topViewPoint.transform.rotation = rotationTop;

            //Set matrices needed for voxelization
            Shader.SetGlobalMatrix("WorldToCamera", attachedCamera.worldToCameraMatrix);
            Shader.SetGlobalMatrix("SEGIVoxelViewFront", TransformViewMatrix(voxelCamera.transform.worldToLocalMatrix));
            Shader.SetGlobalMatrix("SEGIVoxelViewLeft", TransformViewMatrix(leftViewPoint.transform.worldToLocalMatrix));
            Shader.SetGlobalMatrix("SEGIVoxelViewTop", TransformViewMatrix(topViewPoint.transform.worldToLocalMatrix));
            Shader.SetGlobalMatrix("SEGIWorldToVoxel", voxelCamera.worldToCameraMatrix);
            Shader.SetGlobalMatrix("SEGIVoxelProjection", voxelCamera.projectionMatrix);
            Shader.SetGlobalMatrix("SEGIVoxelProjectionInverse", voxelCamera.projectionMatrix.inverse);

            Shader.SetGlobalInt("SEGIVoxelResolution", (int) Data.VoxelResolution);

            Matrix4x4 voxelToGIProjection = shadowCamera.projectionMatrix * shadowCamera.worldToCameraMatrix * voxelCamera.cameraToWorldMatrix;
            Shader.SetGlobalMatrix("SEGIVoxelToGIProjection", voxelToGIProjection);
            Shader.SetGlobalVector("SEGISunlightVector", sun ? Vector3.Normalize(sun.transform.forward) : Vector3.up);

            //Set paramteters
            Shader.SetGlobalColor("GISunColor", sun == null ? Color.black : new Color(Mathf.Pow(sun.color.r, 2.2f), Mathf.Pow(sun.color.g, 2.2f), Mathf.Pow(sun.color.b, 2.2f), Mathf.Pow(sun.intensity, 2.2f)));
            Shader.SetGlobalColor("SEGISkyColor", new Color(Mathf.Pow(skyColor.r * Data.SkyIntensity * 0.5f, 2.2f), Mathf.Pow(skyColor.g * Data.SkyIntensity * 0.5f, 2.2f), Mathf.Pow(skyColor.b * Data.SkyIntensity * 0.5f, 2.2f), Mathf.Pow(skyColor.a, 2.2f)));
            Shader.SetGlobalFloat("GIGain", Data.GIGain);
            Shader.SetGlobalFloat("SEGISecondaryBounceGain", Data.InfiniteBounces ? Data.GIGain : 0.0f);
            Shader.SetGlobalFloat("SEGISoftSunlight", Data.SoftSunlight);
            Shader.SetGlobalInt("SEGISphericalSkylight", sphericalSkylight ? 1 : 0);
            Shader.SetGlobalInt("SEGIData.InnerOcclusionLayers", Data.InnerOcclusionLayers);

            //Render the depth texture from the sun's perspective in order to inject sunlight with shadows during voxelization
            if (sun != null) {
                shadowCamera.cullingMask = giCullingMask;

                Vector3 shadowCamPosition = voxelSpaceOrigin + (Vector3.Normalize(-sun.transform.forward) * Data.ShadowSpaceSize * 0.5f * shadowSpaceDepthRatio);
                shadowCameraTransform.position = shadowCamPosition;
                shadowCameraTransform.LookAt(voxelSpaceOrigin, Vector3.up);
                shadowCamera.renderingPath = RenderingPath.Forward;
                shadowCamera.depthTextureMode |= DepthTextureMode.None;
                shadowCamera.orthographicSize = Data.ShadowSpaceSize;
                shadowCamera.farClipPlane = Data.ShadowSpaceSize * 2.0f * shadowSpaceDepthRatio;


                Graphics.SetRenderTarget(sunDepthTexture);
                shadowCamera.SetTargetBuffers(sunDepthTexture.colorBuffer, sunDepthTexture.depthBuffer);
                shadowCamera.RenderWithShader(sunDepthShader, "");

                Shader.SetGlobalTexture("SEGISunDepth", sunDepthTexture);
            }

            //Clear the volume texture that is immediately written to in the voxelization scene shader
            clearCompute.SetTexture(0, "RG0", integerVolume);
            clearCompute.SetInt("Res", (int) Data.VoxelResolution);
            clearCompute.Dispatch(0, (int) Data.VoxelResolution / 16, (int) Data.VoxelResolution / 16, 1);

            //Render the scene with the voxel proxy camera object with the voxelization shader to voxelize the scene to the volume integer texture
            Graphics.SetRandomWriteTarget(1, integerVolume);
            voxelCamera.targetTexture = dummyVoxelTextureAAScaled;
            voxelCamera.RenderWithShader(voxelizationShader, "");
            Graphics.ClearRandomWriteTargets();

            //Transfer the data from the volume integer texture to the main volume texture used for GI tracing. 
            transferIntsCompute.SetTexture(0, "Result", activeVolume);
            transferIntsCompute.SetTexture(0, "PrevResult", previousActiveVolume);
            transferIntsCompute.SetTexture(0, "RG0", integerVolume);
            transferIntsCompute.SetInt("VoxelAA", Data.VoxelAntiAliasing ? 1 : 0);
            transferIntsCompute.SetInt("Resolution", (int) Data.VoxelResolution);
            transferIntsCompute.SetVector("VoxelOriginDelta", voxelSpaceOriginDelta / Data.VoxelSpaceSize * (int) Data.VoxelResolution);
            transferIntsCompute.Dispatch(0, (int) Data.VoxelResolution / 16, (int) Data.VoxelResolution / 16, 1);

            //Manually filter/render mip maps
            Shader.SetGlobalTexture("SEGIVolumeLevel0", activeVolume);
            for (int i = 0; i < mipLevels - 1; i++) {
                RenderTexture source = volumeTextures[i];

                if (i == 0) {
                    source = activeVolume;
                }

                int destinationRes = (int) Data.VoxelResolution / Mathf.RoundToInt(Mathf.Pow(2, i + 1.0f));
                mipFilterCompute.SetInt("destinationRes", destinationRes);
                mipFilterCompute.SetTexture(MipFilterKernel, "Source", source);
                mipFilterCompute.SetTexture(MipFilterKernel, "Destination", volumeTextures[i + 1]);
                mipFilterCompute.Dispatch(MipFilterKernel, destinationRes / 8, destinationRes / 8, 1);
                Shader.SetGlobalTexture("SEGIVolumeLevel" + (i + 1).ToString(), volumeTextures[i + 1]);
            }

            //Advance the voxel flip flop counter
            voxelFlipFlop += 1;
            voxelFlipFlop %= 2;

            if (Data.InfiniteBounces) {
                renderState = RenderState.Bounce;
            }
        }
        else if (renderState == RenderState.Bounce) {

            //Clear the volume texture that is immediately written to in the voxelization scene shader
            clearCompute.SetTexture(0, "RG0", integerVolume);
            clearCompute.Dispatch(0, (int) Data.VoxelResolution / 16, (int) Data.VoxelResolution / 16, 1);

            //Set secondary tracing parameters
            Shader.SetGlobalInt("SEGISecondaryCones", Data.SecondaryCones);
            Shader.SetGlobalFloat("SEGISecondaryOcclusionStrength", Data.SecondaryOcclusionStrength);

            //Render the scene from the voxel camera object with the voxel tracing shader to render a bounce of GI into the irradiance volume
            Graphics.SetRandomWriteTarget(1, integerVolume);
            voxelCamera.targetTexture = dummyVoxelTextureFixed;
            voxelCamera.RenderWithShader(voxelTracingShader, "");
            Graphics.ClearRandomWriteTargets();


            //Transfer the data from the volume integer texture to the irradiance volume texture. This result is added to the next main voxelization pass to create a feedback loop for infinite bounces
            transferIntsCompute.SetTexture(1, "Result", secondaryIrradianceVolume);
            transferIntsCompute.SetTexture(1, "RG0", integerVolume);
            transferIntsCompute.SetInt("Resolution", (int) Data.VoxelResolution);
            transferIntsCompute.Dispatch(1, (int) Data.VoxelResolution / 16, (int) Data.VoxelResolution / 16, 1);

            Shader.SetGlobalTexture("SEGIVolumeTexture1", secondaryIrradianceVolume);

            renderState = RenderState.Voxelize;
        }

        RenderTexture.active = previousActive;
    }

    [ImageEffectOpaque]
    [UsedImplicitly]
    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (notReadyToRender) {
            Graphics.Blit(source, destination);

            return;
        }

        //Set parameters
        Shader.SetGlobalInt("SEGIFrameSwitch", frameCounter);
        Shader.SetGlobalFloat("SEGIVoxelScaleFactor", VoxelScaleFactor);

        material.SetMatrix("CameraToWorld", attachedCamera.cameraToWorldMatrix);
        material.SetMatrix("WorldToCamera", attachedCamera.worldToCameraMatrix);
        material.SetMatrix("ProjectionMatrixInverse", attachedCamera.projectionMatrix.inverse);
        material.SetMatrix("ProjectionMatrix", attachedCamera.projectionMatrix);
        material.SetInt("FrameSwitch", frameCounter);
        material.SetVector("CameraPosition", transform.position);
        material.SetFloat("DeltaTime", Time.deltaTime);

        material.SetInt("StochasticSampling", Data.StochasticSampling ? 1 : 0);
        material.SetInt("TraceDirections", Data.Cones);
        material.SetInt("TraceSteps", Data.ConeTraceSteps);
        material.SetFloat("TraceLength", Data.ConeLength);
        material.SetFloat("ConeSize", Data.ConeWidth);
        material.SetFloat("OcclusionStrength", Data.OcclusionStrength);
        material.SetFloat("OcclusionPower", Data.OcclusionPower);
        material.SetFloat("ConeTraceBias", Data.ConeTraceBias);
        material.SetFloat("GIGain", Data.GIGain);
        material.SetFloat("NearLightGain", Data.NearLightGain);
        material.SetFloat("NearOcclusionStrength", Data.NearOcclusionStrength);
        material.SetInt("DoReflections", Data.DoReflections ? 1 : 0);
        material.SetInt("HalfResolution", Data.HalfResolution ? 1 : 0);
        material.SetInt("ReflectionSteps", Data.ReflectionSteps);
        material.SetFloat("ReflectionOcclusionPower", Data.ReflectionOcclusionPower);
        material.SetFloat("SkyReflectionIntensity", Data.SkyReflectionIntensity);
        material.SetFloat("FarOcclusionStrength", Data.FarOcclusionStrength);
        material.SetFloat("FarthestOcclusionStrength", Data.FarthestOcclusionStrength);
        material.SetTexture("NoiseTexture", blueNoise[frameCounter % 64]);
        material.SetFloat("BlendWeight", Data.TemporalBlendWeight);

        //If Visualize Voxels is enabled, just render the voxel visualization shader pass and return
        if (visualizeVoxels) {
            Graphics.Blit(source, destination, material, Pass.VisualizeVoxels);

            return;
        }

        //Setup temporary textures
        RenderTexture gi1 = RenderTexture.GetTemporary(source.width / GIRenderRes, source.height / GIRenderRes, 0, RenderTextureFormat.ARGBHalf);
        RenderTexture gi2 = RenderTexture.GetTemporary(source.width / GIRenderRes, source.height / GIRenderRes, 0, RenderTextureFormat.ARGBHalf);
        RenderTexture reflections = null;

        //If reflections are enabled, create a temporary render buffer to hold them
        if (Data.DoReflections) {
            reflections = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBHalf);
        }

        //Setup textures to hold the current camera depth and normal
        RenderTexture currentDepth = RenderTexture.GetTemporary(source.width / GIRenderRes, source.height / GIRenderRes, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        currentDepth.filterMode = FilterMode.Point;

        RenderTexture currentNormal = RenderTexture.GetTemporary(source.width / GIRenderRes, source.height / GIRenderRes, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
        currentNormal.filterMode = FilterMode.Point;

        //Get the camera depth and normals
        Graphics.Blit(source, currentDepth, material, Pass.GetCameraDepthTexture);
        material.SetTexture("CurrentDepth", currentDepth);
        Graphics.Blit(source, currentNormal, material, Pass.GetWorldNormals);
        material.SetTexture("CurrentNormal", currentNormal);

        //Set the previous GI result and camera depth textures to access them in the shader
        material.SetTexture("PreviousGITexture", previousGIResult);
        Shader.SetGlobalTexture("PreviousGITexture", previousGIResult);
        material.SetTexture("PreviousDepth", previousCameraDepth);

        //Render diffuse GI tracing result
        Graphics.Blit(source, gi2, material, Pass.DiffuseTrace);
        if (Data.DoReflections) {
            //Render GI reflections result
            Graphics.Blit(source, reflections, material, Pass.SpecularTrace);
            material.SetTexture("Reflections", reflections);
        }

        //Perform bilateral filtering
        if (Data.UseBilateralFiltering) {
            material.SetVector("Kernel", new Vector2(0.0f, 1.0f));
            Graphics.Blit(gi2, gi1, material, Pass.BilateralBlur);

            material.SetVector("Kernel", new Vector2(1.0f, 0.0f));
            Graphics.Blit(gi1, gi2, material, Pass.BilateralBlur);

            material.SetVector("Kernel", new Vector2(0.0f, 1.0f));
            Graphics.Blit(gi2, gi1, material, Pass.BilateralBlur);

            material.SetVector("Kernel", new Vector2(1.0f, 0.0f));
            Graphics.Blit(gi1, gi2, material, Pass.BilateralBlur);
        }

        //If Half Resolution tracing is enabled
        if (GIRenderRes == 2) {
            RenderTexture.ReleaseTemporary(gi1);

            //Setup temporary textures
            RenderTexture gi3 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBHalf);
            RenderTexture gi4 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBHalf);

            //Prepare the half-resolution diffuse GI result to be bilaterally upsampled
            gi2.filterMode = FilterMode.Point;
            Graphics.Blit(gi2, gi4);

            RenderTexture.ReleaseTemporary(gi2);

            gi4.filterMode = FilterMode.Point;
            gi3.filterMode = FilterMode.Point;

            //Perform bilateral upsampling on half-resolution diffuse GI result
            material.SetVector("Kernel", new Vector2(1.0f, 0.0f));
            Graphics.Blit(gi4, gi3, material, Pass.BilateralUpsample);
            material.SetVector("Kernel", new Vector2(0.0f, 1.0f));

            //Perform temporal reprojection and blending
            if (Data.TemporalBlendWeight < 1.0f) {
                Graphics.Blit(gi3, gi4);
                Graphics.Blit(gi4, gi3, material, Pass.TemporalBlend);
                Graphics.Blit(gi3, previousGIResult);
                Graphics.Blit(source, previousCameraDepth, material, Pass.GetCameraDepthTexture);
            }

            //Set the result to be accessed in the shader
            material.SetTexture("GITexture", gi3);

            //Actually apply the GI to the scene using gbuffer data
            Graphics.Blit(source, destination, material, visualizeGI ? Pass.VisualizeGI : Pass.BlendWithScene);

            //Release temporary textures
            RenderTexture.ReleaseTemporary(gi3);
            RenderTexture.ReleaseTemporary(gi4);
        }
        else    //If Half Resolution tracing is disabled
        {
            //Perform temporal reprojection and blending
            if (Data.TemporalBlendWeight < 1.0f) {
                Graphics.Blit(gi2, gi1, material, Pass.TemporalBlend);
                Graphics.Blit(gi1, previousGIResult);
                Graphics.Blit(source, previousCameraDepth, material, Pass.GetCameraDepthTexture);
            }

            //Actually apply the GI to the scene using gbuffer data
            material.SetTexture("GITexture", Data.TemporalBlendWeight < 1.0f ? gi1 : gi2);
            Graphics.Blit(source, destination, material, visualizeGI ? Pass.VisualizeGI : Pass.BlendWithScene);

            //Release temporary textures
            RenderTexture.ReleaseTemporary(gi1);
            RenderTexture.ReleaseTemporary(gi2);
        }

        //Release temporary textures
        RenderTexture.ReleaseTemporary(currentDepth);
        RenderTexture.ReleaseTemporary(currentNormal);

        //Visualize the sun depth texture
        if (visualizeSunDepthTexture) {
            Graphics.Blit(sunDepthTexture, destination);
        }

        //Release the temporary reflections result texture
        if (Data.DoReflections) {
            RenderTexture.ReleaseTemporary(reflections);
        }

        //Set matrices/vectors for use during temporal reprojection
        material.SetMatrix("ProjectionPrev", attachedCamera.projectionMatrix);
        material.SetMatrix("ProjectionPrevInverse", attachedCamera.projectionMatrix.inverse);
        material.SetMatrix("WorldToCameraPrev", attachedCamera.worldToCameraMatrix);
        material.SetMatrix("CameraToWorldPrev", attachedCamera.cameraToWorldMatrix);
        material.SetVector("CameraPositionPrev", transform.position);

        //Advance the frame counter
        frameCounter = (frameCounter + 1) % 64;
    }
}