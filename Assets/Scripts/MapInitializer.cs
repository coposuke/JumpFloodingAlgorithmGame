using UnityEngine;

public class MapInitializer : MonoBehaviour
{
	[SerializeField]
	private Texture initializeTexture;

	[SerializeField]
	private RenderTexture targetRenderTexture;

	void Awake()
    {
		Graphics.Blit(initializeTexture, targetRenderTexture);
    }
}
