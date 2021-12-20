using UnityEngine;


public class PlayerFreeWalkCtrl : MonoBehaviour
{
	[SerializeField]
	private float movePower = 100.0f;

	[SerializeField]
	private RenderTexture outputRenderTexture = default;

	private Texture2D outputTexture = default;
	

	/// <summary>
	/// Unity Override Start
	/// </summary>
	private void Start()
	{
		this.outputTexture = new Texture2D(
			this.outputRenderTexture.width,
			this.outputRenderTexture.height,
			TextureFormat.RGBA32, false, false);
	}

	/// <summary>
	/// Unity Override Update
	/// </summary>
	private void Update()
	{
		UpdateColliderTexture();

		Vector3 velocity = default;

		if (Input.GetKey(KeyCode.UpArrow))
			velocity.y += movePower;

		if (Input.GetKey(KeyCode.DownArrow))
			velocity.y -= movePower;

		if (Input.GetKey(KeyCode.LeftArrow))
			velocity.x -= movePower;

		if (Input.GetKey(KeyCode.RightArrow))
			velocity.x += movePower;

		var estimate = this.transform.position + velocity * Time.deltaTime;
		var seedColor = this.outputTexture.GetPixel(Mathf.RoundToInt(estimate.x), Mathf.RoundToInt(estimate.y), 0);

		this.seed = new Vector3(seedColor.r, seedColor.g, 0f);
		this.dist = seedColor.b;

		this.transform.localPosition += velocity * Time.deltaTime;
	}

	private void UpdateColliderTexture()
	{
		var temp = RenderTexture.active;

		RenderTexture.active = this.outputRenderTexture;
		this.outputTexture.ReadPixels(new Rect(0, 0, this.outputTexture.width, this.outputTexture.height), 0, 0);
		RenderTexture.active = temp;
	}

	private Vector3 seed;
	private float dist;

#if UNITY_EDITOR
	/// <summary>
	/// Unity Override OnDrawGizmos
	/// </summary>
	private void OnDrawGizmos()
	{
		using (new UnityEditor.Handles.DrawingScope(new Color(0.72f, 0.60f, 0.84f)))
		{
			UnityEditor.Handles.SphereHandleCap(0, this.seed * 1024f, Quaternion.identity, 10f, EventType.Repaint);
			UnityEditor.Handles.DrawWireDisc(this.transform.position, Vector3.forward, this.dist * 1024f);
		}
	}
#endif
}
