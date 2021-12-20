using UnityEngine;

/// <summary>
/// Map Manager
/// マップのコリジョンを制御します
/// </summary>
public class MapManager : MonoBehaviour
{
    private static MapManager instance;
    private static readonly int ShaderParam_ScrapePoint = Shader.PropertyToID("_ScrapePoint");
    private static readonly int ShaderParam_ScrapeRadius = Shader.PropertyToID("_ScrapeRadius");

	[SerializeField]
	private Texture initializeTexture = default;

	[SerializeField]
	private RenderTexture targetRenderTexture = default;

    [SerializeField]
    private Material material = default;


    /// <summary>
    /// Unity Override Awake
    /// </summary>
	private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Graphics.Blit(initializeTexture, targetRenderTexture);
        }
        else
        {
            DestroyImmediate(this.gameObject);
        }
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
    /// 地形を削る
    /// </summary>
    /// <param name="uv">削る中心点</param>
    /// <param name="radius">削る半径</param>
    public static void Scrape(Vector2 uv, float radius)
    {
        if (instance == null)
            return;

        instance.material.SetVector(ShaderParam_ScrapePoint, uv);
        instance.material.SetFloat(ShaderParam_ScrapeRadius, radius);

        var rt = instance.targetRenderTexture;
        rt = RenderTexture.GetTemporary(rt.width, rt.height, rt.depth, rt.format);
        Graphics.Blit(instance.targetRenderTexture, rt);
        Graphics.Blit(rt, instance.targetRenderTexture, instance.material, 0);
        RenderTexture.ReleaseTemporary(rt);
    }
}
