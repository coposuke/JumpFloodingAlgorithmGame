using UnityEngine;


public class PlayerCtrl : MonoBehaviour
{
	[SerializeField]
	private float movePower = 100.0f;

	[SerializeField]
	private float jumpPower = 500.0f;

	[SerializeField]
	private float radius = 1f;

	private Vector3 velocity = default;


	/// <summary>
	/// Unity Override Start
	/// </summary>
    private void Start()
    {
        var renderer = this.GetComponent<MeshRenderer>();
        renderer.material.color = new Color(0.52f, 0.40f, 0.64f);
    }

	/// <summary>
	/// Unity Override Update
	/// </summary>
	private void Update()
    {
        this.velocity.x -= Mathf.Sign(this.velocity.x) * 0.2f; // 適当な減衰
        this.velocity.y -= 9.81f * 0.5f * Time.deltaTime * Time.deltaTime * 100f;

        // 開始早々はdeltaTimeが大きい値が入るので止めている
        if (Time.frameCount < 10)
            this.velocity = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.UpArrow))
            this.velocity.y += jumpPower * 0.016f;

        if (Input.GetKey(KeyCode.DownArrow))
            this.velocity.y -= movePower * Time.deltaTime;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            this.velocity.x = -movePower * Time.deltaTime;
            this.transform.localRotation = Quaternion.LookRotation(Vector3.left, Vector3.up);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            this.velocity.x = movePower * Time.deltaTime;
            this.transform.localRotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var bombObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bombObject.name = "PlayerBomb";
            bombObject.transform.localPosition = this.transform.localPosition;
            bombObject.transform.localScale = Vector3.one * 10f;

            var bomb = bombObject.AddComponent<PlayerBomb>();
            bomb.velocity.x = Mathf.Sign(this.transform.forward.x);
            bomb.velocity.y = 0.75f;
            bomb.velocity = bomb.velocity.normalized * 800f * 0.016f;
            bomb.radius = 1f;
        }

        var estimate = this.transform.position + this.velocity;

		var point = Vector2.zero;
		var normal = Vector2.zero;
		var distance = 0f;
		JumpFloodingManager.Get(estimate / 1024f, out point, out normal, out distance);
		point *= 1024f;
		distance *= 1024f;

		if (distance < radius)
		{
			// 押し返し
			this.transform.localPosition = point + normal * (radius + 1e-3f);
			
			// 再取得
			JumpFloodingManager.Get(this.transform.localPosition / 1024f, out point, out normal, out distance);

			// 反発
			const float e = 1.5f;
			Vector3 vn = Vector2.Dot(this.velocity, normal) * normal;
            this.velocity = this.velocity - vn * e;
		}
		else
		{
			this.transform.localPosition += this.velocity;
		}
	}

#if UNITY_EDITOR
	/// <summary>
	/// Unity Override OnDrawGizmos
	/// </summary>
	private void OnDrawGizmos()
	{
		var estimate = this.transform.position + this.velocity;
		var point = Vector2.zero;
		var normal = Vector2.zero;
		var distance = 0f;
		JumpFloodingManager.Get(estimate / 1024f, out point, out normal, out distance);
		point *= 1024f;
		distance *= 1024f;

		using (new UnityEditor.Handles.DrawingScope(new Color(0.82f, 0.70f, 0.94f)))
		{
			UnityEngine.Gizmos.DrawWireSphere(point, 10f);
			UnityEditor.Handles.DrawWireDisc(this.transform.position, Vector3.forward, distance);
			if (0 < normal.magnitude)
				UnityEditor.Handles.Slider(point, normal);
		}
	}
#endif
}
