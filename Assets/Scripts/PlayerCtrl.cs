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
		this.velocity.x = 0f;
		this.velocity.y -= 98.1f * 0.5f * Time.deltaTime * Time.deltaTime;

		if (Input.GetKey(KeyCode.UpArrow))
			this.velocity.y += jumpPower * Time.deltaTime;

		if (Input.GetKey(KeyCode.LeftArrow))
			this.velocity.x = -movePower * Time.deltaTime;

		if (Input.GetKey(KeyCode.RightArrow))
			this.velocity.x = movePower * Time.deltaTime;

		var estimate = this.transform.position + this.velocity;

		var point = Vector2.zero;
		var direction = Vector2.zero;
		var distance = 0f;
		JumpFloodingManager.Get(estimate / 1024f, out point, out direction, out distance);
		point *= 1024f;
		direction *= 1024f;
		distance *= 1024f;

		if (distance < radius)
			this.velocity.y = 0f;

		this.transform.localPosition += this.velocity;
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		var estimate = this.transform.position + this.velocity;
		var point = Vector2.zero;
		var direction = Vector2.zero;
		var distance = 0f;
		JumpFloodingManager.Get(estimate / 1024f, out point, out direction, out distance);

		using (new UnityEditor.Handles.DrawingScope(new Color(0.72f, 0.60f, 0.84f)))
		{
			UnityEditor.Handles.SphereHandleCap(0, point * 1024f, Quaternion.identity, 10f, EventType.Repaint);
			UnityEditor.Handles.DrawWireDisc(this.transform.position, Vector3.forward, distance * 1024f);
		}
	}
#endif
}
