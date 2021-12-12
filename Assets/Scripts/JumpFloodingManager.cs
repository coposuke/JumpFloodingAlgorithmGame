using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Jump Flooding Algorithm Manager
/// </summary>
public class JumpFloodingManager : MonoBehaviour
{
	private static JumpFloodingManager instance;
	private readonly int RT0 = Shader.PropertyToID("temp0");
	private readonly int RT1 = Shader.PropertyToID("temp1");
	private readonly int ShaderParam_StepLength = Shader.PropertyToID("_StepLength");

	[SerializeField]
	private Camera targetCamera;

	[SerializeField]
	private RenderTexture inputRenderTexture;

	[SerializeField]
	private RenderTexture outputRenderTexture;

	[SerializeField]
	private Material material;

	private Texture2D outputTexture = default;

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
			float stepLength = Mathf.Clamp(Mathf.Pow(2.0f, (JumpCount - i) - 1), 1f, 1024f);
			commandBuffer.SetGlobalFloat(ShaderParam_StepLength, stepLength);
			commandBuffer.Blit(rtArray[i % rtCount], rtArray[(i + 1) % rtCount], this.material, 1);
		}

		commandBuffer.Blit(rtArray[JumpCount % rtCount], outputRenderTexture);
		commandBuffer.ReleaseTemporaryRT(RT0);
		commandBuffer.ReleaseTemporaryRT(RT1);

		this.targetCamera.AddCommandBuffer(CameraEvent.BeforeDepthTexture, commandBuffer);
		this.targetCamera.AddCommandBuffer(CameraEvent.BeforeGBuffer, commandBuffer);

		this.outputTexture = new Texture2D(
			this.outputRenderTexture.width,
			this.outputRenderTexture.height,
			TextureFormat.RGBA32, false, false);
	}

	/// <summary>
	/// Unity Override OnPostRender
	/// </summary>
	private void OnPostRender()
	{
		var temp = RenderTexture.active;

		RenderTexture.active = this.outputRenderTexture;
		this.outputTexture.ReadPixels(new Rect(0, 0, this.outputTexture.width, this.outputTexture.height), 0, 0);
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

		int x = Mathf.RoundToInt(uv.x * instance.outputTexture.width);
		int y = Mathf.RoundToInt(uv.y * instance.outputTexture.height);

		var color = instance.outputTexture.GetPixel(x, y, 0);
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

		int x = Mathf.RoundToInt(uv.x * instance.outputTexture.width);
		int y = Mathf.RoundToInt(uv.y * instance.outputTexture.height);

		var color = instance.outputTexture.GetPixel(x, y, 0);
		point = new Vector2(color.r, color.g);
		distance = color.b * (1f - color.a);
	}

	/// <summary>
	/// 一番近い地点と向きと距離を返す
	/// </summary>
	public static void Get(Vector2 uv, out Vector2 point, out Vector2 dir, out float distance)
	{
		point = dir = Vector2.zero;
		distance = 0f;

		if (instance == null)
			return;

		int x = Mathf.RoundToInt(uv.x * instance.outputTexture.width);
		int y = Mathf.RoundToInt(uv.y * instance.outputTexture.height);

		var color = instance.outputTexture.GetPixel(x, y, 0);
		point = new Vector2(color.r, color.g);
		dir = (uv - point).normalized;
		distance = color.b * -(color.a * 2f - 1f);
	}
}
