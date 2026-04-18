using UnityEngine;

public class CamMovement : MonoBehaviour
{
	[Header("Target")]
	public Transform target;

	[Header("Follow Settings")]
	public Vector3 offset = new Vector3(0f, 0f, -10f);
	public float smoothSpeed = 5f;

	[Header("Bounds (optional)")]
	public bool useBounds = false;
	public Vector2 minBounds;
	public Vector2 maxBounds;

	void FixedUpdate()
	{
		if (target == null) return;

		Vector3 desiredPosition = target.position + offset;
		Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

		if (useBounds)
		{
			smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minBounds.x, maxBounds.x);
			smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minBounds.y, maxBounds.y);
		}

		transform.position = smoothedPosition;
	}
}
