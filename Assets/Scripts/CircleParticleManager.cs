using System.Collections.Generic;
using UnityEngine;

public class CircleParticleManager : MonoBehaviour
{
    private static CircleParticleManager instance;
    private const int ParticleCount = 1024;
    private static readonly int ShaderParam_Particles = Shader.PropertyToID("_Particles");
    private static readonly int ShaderParam_DeltaTime = Shader.PropertyToID("_DeltaTime");
    private static readonly int ShaderParam_OutputTexture = Shader.PropertyToID("_OutputTexture");
    private static readonly int ShaderParam_OutputNormalTexture  = Shader.PropertyToID("_OutputNormalTexture");

    private struct ParticleData
    {
        public float active;
        public float radius;
        public Vector2 position;
        public Vector2 velocity;

        static public int GetSize()
        {
            return sizeof(float) * 6;
        }
    }

    [SerializeField]
    private RenderTexture outputRenderTexture = default;

    [SerializeField]
    private RenderTexture outputNormalRenderTexture = default;

    [SerializeField]
    private ComputeShader shader = default;

    [SerializeField]
    private Mesh mesh = default;

    [SerializeField]
    private Material meshMaterial = default;

    private ParticleData[] data = default;
    private ComputeBuffer buffer = default;
    private int kernelIndex = 0;
    private int activeIndex = 0;
    private RenderTexture tempRenderTexture = default;
    private RenderTexture tempNormalRenderTexture = default;
    private ComputeBuffer argBuffer = default;


    /// <summary>
    /// Unity Override Awake
    /// </summary>
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            DestroyImmediate(this.gameObject);
    }

    private ComputeBuffer positionBuffer;

    /// <summary>
    /// Unity Override Start
    /// </summary>
    void Start()
    {
        // 本来はoutputRenderTextureはJumpFloodingManagerのプログラム内で作成されるべき
        // 今回は面倒だったのでtemp(enableRandomWrite…UAVにして)にBlitします
        var rtDesc = new RenderTextureDescriptor()
        {
            width = this.outputRenderTexture.width,
            height = this.outputRenderTexture.height,
            depthBufferBits = this.outputRenderTexture.depth,
            volumeDepth = 1,
            msaaSamples = 1,
            dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
            colorFormat = this.outputRenderTexture.format,
            autoGenerateMips = false,
            useMipMap = false,
            mipCount = 1,
            graphicsFormat = this.outputRenderTexture.graphicsFormat,
            memoryless = this.outputRenderTexture.memorylessMode,
            enableRandomWrite = true,
        };
        this.tempRenderTexture = new RenderTexture(rtDesc);
        this.tempNormalRenderTexture = new RenderTexture(rtDesc);

        this.data = new ParticleData[ParticleCount];
        this.buffer = new ComputeBuffer(ParticleCount, ParticleData.GetSize(), ComputeBufferType.Structured);
        this.buffer.SetData(this.data);
        this.kernelIndex = this.shader.FindKernel("CSUpdate");
        this.shader.SetBuffer(this.kernelIndex, ShaderParam_Particles, this.buffer);
        this.shader.SetTexture(this.kernelIndex, ShaderParam_OutputTexture, this.tempRenderTexture);
        this.shader.SetTexture(this.kernelIndex, ShaderParam_OutputNormalTexture, this.tempNormalRenderTexture);

        var arg = new uint[5] {
            (uint)this.mesh.GetIndexCount(0),
            (uint)ParticleCount,
            (uint)this.mesh.GetIndexStart(0),
            (uint)this.mesh.GetBaseVertex(0),
            0,
        };
        this.argBuffer = new ComputeBuffer(1, sizeof(uint) * arg.Length, ComputeBufferType.IndirectArguments);
        this.argBuffer.SetData(arg);
    }

    private void OnDestroy()
    {
        if (this.buffer != null)
            this.buffer.Release();

        if (this.argBuffer != null)
            this.argBuffer.Release();

        if (this.tempRenderTexture != null)
            this.tempRenderTexture.Release();

        if (this.tempNormalRenderTexture != null)
            this.tempNormalRenderTexture.Release();

        this.buffer = null;
        this.argBuffer = null;
        this.tempRenderTexture = null;
        this.tempNormalRenderTexture = null;
    }

    private void Update()
    {
        // 本来はoutputRenderTextureはJumpFloodingManagerのプログラム内で作成されるべき
        // 今回は面倒だったのでtemp(enableRandomWrite…UAVにして)にBlitします
        Graphics.Blit(this.outputRenderTexture, this.tempRenderTexture);
        Graphics.Blit(this.outputNormalRenderTexture, this.tempNormalRenderTexture);

        this.shader.SetTexture(this.kernelIndex, ShaderParam_OutputTexture, this.tempRenderTexture);
        this.shader.SetTexture(this.kernelIndex, ShaderParam_OutputNormalTexture, this.tempNormalRenderTexture);

        this.shader.SetFloat(ShaderParam_DeltaTime, Time.deltaTime);
        this.shader.Dispatch(this.kernelIndex, Mathf.CeilToInt(ParticleCount / 128), 1, 1);

        this.meshMaterial.SetBuffer("_ParticleData", this.buffer);
        Graphics.DrawMeshInstancedIndirect(this.mesh, 0, this.meshMaterial, new Bounds(Vector3.one * 512f, Vector3.one * 1024f), this.argBuffer);
    }

    static public void Emit(Vector2 [] positions, Vector2 [] velocities)
    {
        if (instance == null)
            return;

        instance.buffer.GetData(instance.data, 0, 0, instance.data.Length);

        for (int i = 0; i < positions.Length; ++i)
        {
            int idx = (instance.activeIndex + i) % ParticleCount;
            instance.data[idx] = new ParticleData() {
                active = 1f,
                radius = 2.5f,
                position = positions[i],
                velocity = velocities[i],
            };
        }

        instance.buffer.SetData(instance.data);
        instance.activeIndex += positions.Length;
        instance.activeIndex %= ParticleCount;
    }
}
