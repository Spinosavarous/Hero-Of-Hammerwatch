using UnityEngine;

public class MinimapCam : MonoBehaviour
{
	public Transform player;
	public float smoothSpeed = 8f;

	void LateUpdate()
	{
		if (player == null) return;

		Vector3 target =
			new Vector3(
				player.position.x,
				player.position.y,
				-10f
			);

		transform.position =
			Vector3.Lerp(
				transform.position,
				target,
				Time.deltaTime * smoothSpeed
			);
	}
}