using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Jump Flooding Algorithm Manager
/// マップの距離情報を作成、および情報管理します
/// </summary>
public class JumpFloodingManager : MonoBehaviour
{
	private static JumpFloodingManager instance;
	private static readonly int RT0 = Shader.PropertyToID("temp0");
	private static readonly int RT1 = Shader.PropertyToID("temp1");
	private static readonly int ShaderParam_StepLength = Shader.PropertyToID("_StepLength");

	[SerializeField]
	private Camera targetCamera;

	[SerializeField]
	private RenderTexture inputRenderTexture;

	[SerializeField]
	private RenderTexture outputRenderTexture;

	[SerializeField]
	private RenderTexture outputNormalRenderTexture;

	[SerializeField]
	private Material material;

	private Texture2D outputTexture = default;
	private Texture2D outputNormalTexture = default;


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

	/// <summary>
	/// Unity Override OnDestroy
	/// </summary>
	private void OnDestroy()
	{
		if (instance == this)
			instance = null;
	}

	/// <summary>
	/// Unity Override Start
	/// </summary>
	private void Start()
    {
		int height = outputRenderTexture.height;
		int width = outputRenderTexture.width;

		var commandBuffer = new CommandBuffer();
		commandBuffer.GetTemporaryRT(RT0, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);
		commandBuffer.GetTemporaryRT(RT1, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);
		commandBuffer.Blit(inputRenderTexture, RT0, this.material, 0);

		int[] rtArray = { RT0, RT1 };
		int rtCount = rtArray.Length;
		const int JumpCount = 8;
		for (int i = 0; i < JumpCount; ++i)
		{
			float stepLength = Mathf.Pow(2.0f, (JumpCount - i) - 1);
			commandBuffer.SetGlobalFloat(ShaderParam_StepLength, stepLength);
			commandBuffer.Blit(rtArray[i % rtCount], rtArray[(i + 1) % rtCount], this.material, 1);
		}

		commandBuffer.Blit(rtArray[JumpCount % rtCount], outputRenderTexture);
		commandBuffer.ReleaseTemporaryRT(RT0);
		commandBuffer.ReleaseTemporaryRT(RT1);
		commandBuffer.Blit(outputRenderTexture, outputNormalRenderTexture, this.material, 2);

        this.targetCamera.AddCommandBuffer(CameraEvent.BeforeDepthTexture, commandBuffer);
		this.targetCamera.AddCommandBuffer(CameraEvent.BeforeGBuffer, commandBuffer);

		this.outputTexture = new Texture2D(
			this.outputRenderTexture.width,
			this.outputRenderTexture.height,
			TextureFormat.RGBA32, false, false);

		this.outputNormalTexture = new Texture2D(
			this.outputNormalRenderTexture.width,
			this.outputNormalRenderTexture.height,
			TextureFormat.RGBA32, false, false);

		OnPostRender();
	}

    /// <summary>
    /// Unity Override OnPostRender
    /// </summary>
    private void OnPostRender()
    {
        var temp = RenderTexture.active;

        // すごい重い処理してます
        // 都度取得するでもいいかもしれない

        RenderTexture.active = this.outputRenderTexture;
        this.outputTexture.ReadPixels(new Rect(0, 0, this.outputTexture.width, this.outputTexture.height), 0, 0);

        RenderTexture.active = this.outputNormalRenderTexture;
        this.outputNormalTexture.ReadPixels(new Rect(0, 0, this.outputNormalTexture.width, this.outputNormalTexture.height), 0, 0);

        RenderTexture.active = temp;
    }

    /// <summary>
    /// 一番近い地点を返す
    /// </summary>
    public static void Get(Vector2 uv, out Vector2 point)
	{
		point = Vector2.zero;

		if (instance == null)
			return;

		uv.x = Mathf.Clamp01(uv.x);
		uv.y = Mathf.Clamp01(uv.y);

		var color = instance.outputTexture.GetPixelBilinear(uv.x, uv.y, 0);
		point = new Vector2(color.r, color.g);
	}

	/// <summary>
	/// 一番近い地点と距離を返す
	/// </summary>
	public static void Get(Vector2 uv, out Vector2 point, out float distance)
	{
		point = Vector2.zero;
		distance = 0f;

		if (instance == null)
			return;

		uv.x = Mathf.Clamp01(uv.x);
		uv.y = Mathf.Clamp01(uv.y);

		var color = instance.outputTexture.GetPixelBilinear(uv.x, uv.y, 0);
		point = new Vector2(color.r, color.g);
		distance = color.b * (1f - color.a);
	}

	/// <summary>
	/// 一番近い地点と向きと距離を返す
	/// </summary>
	public static void Get(Vector2 uv, out Vector2 point, out Vector2 normal, out float distance)
	{
		point = normal = Vector2.zero;
		distance = 0f;

		if (instance == null)
			return;

		uv.x = Mathf.Clamp01(uv.x);
		uv.y = Mathf.Clamp01(uv.y);

		var color = Color.clear;
		
		color = instance.outputTexture.GetPixelBilinear(uv.x, uv.y, 0);
		point = new Vector2(color.r, color.g);

		//distance = color.b * -(color.a * 2f - 1f); // color.bは精度が低かったので再計算する
		distance = Vector2.Distance(uv, point) * -(color.a * 2f - 1f);

		color = instance.outputNormalTexture.GetPixelBilinear(uv.x, uv.y, 0);
		normal = new Vector2(color.r * 2.0f - 1.0f, color.g * 2.0f - 1.0f);
        normal.Normalize();
        normal *= Mathf.Sign(distance);
    }
}
