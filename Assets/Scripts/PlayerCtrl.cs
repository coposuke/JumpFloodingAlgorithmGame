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


	private void Update()
	{
		this.velocity.x -= Mathf.Sign(this.velocity.x) * 0.2f; // 適当な減衰
		this.velocity.y -= 9.81f * 0.5f * Time.deltaTime * Time.deltaTime * 100f;

		if (Input.GetKeyDown(KeyCode.UpArrow))
			this.velocity.y += jumpPower * 0.016f;

		if (Input.GetKey(KeyCode.DownArrow))
			this.velocity.y -= movePower * Time.deltaTime;

		if (Input.GetKey(KeyCode.LeftArrow))
			this.velocity.x = -movePower * Time.deltaTime;

		if (Input.GetKey(KeyCode.RightArrow))
			this.velocity.x = movePower * Time.deltaTime;

		var estimate = this.transform.position + this.velocity;

		var point = Vector2.zero;
		var normal = Vector2.zero;
		var distance = 0f;
		JumpFloodingManager.Get(estimate / 1024f, out point, out normal, out distance);
		point *= 1024f;
		normal *= Mathf.Sign(distance);
		distance *= 1024f;

		if (distance < radius)
		{
			// 押し返し
			this.transform.localPosition = point + normal * (radius + 1e-3f);
			
			// 再取得
			JumpFloodingManager.Get(this.transform.localPosition / 1024f, out point, out normal, out distance);
			point *= 1024f;
			normal *= Mathf.Sign(distance);
			distance *= 1024f;

			// 反発
			const float e = 1.5f;
			Vector3 vn = Vector2.Dot(this.velocity, normal) * normal;
			vn = this.velocity - vn * e;
			this.velocity.x = vn.x;
			this.velocity.y = vn.y;
		}
		else
		{
			this.transform.localPosition += this.velocity;
		}
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		var estimate = this.transform.position + this.velocity;
		var point = Vector2.zero;
		var normal = Vector2.zero;
		var distance = 0f;
		JumpFloodingManager.Get(estimate / 1024f, out point, out normal, out distance);
		point *= 1024f;
		distance *= 1024f;

		using (new UnityEditor.Handles.DrawingScope(new Color(0.72f, 0.60f, 0.84f)))
		{
			UnityEngine.Gizmos.DrawWireSphere(point, 10f);
			UnityEditor.Handles.DrawWireDisc(this.transform.position, Vector3.forward, distance);
			if (0 < normal.magnitude)
				UnityEditor.Handles.Slider(point, normal);
		}
	}
#endif
}
