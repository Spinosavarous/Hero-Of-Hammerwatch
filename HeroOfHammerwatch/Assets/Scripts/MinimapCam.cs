using UnityEngine;

public class MinimapCam : MonoBehaviour
{
	public Transform player;
	public bool followRotation = false;

	void LateUpdate()
	{
		if (player != null)
		{
			transform.position = new Vector3(player.position.x, player.position.y,-10);

			if (followRotation)
			{
				transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
			}
		}
	}
}
