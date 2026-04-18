using UnityEngine;

public class MapZSort : MonoBehaviour
{
	public int offset = 0; // optional fine-tuning

	private Renderer rend;

	void Awake()
	{
		rend = GetComponent<Renderer>();
	}

	void LateUpdate()
	{
		rend.sortingOrder = (int)(-transform.position.y * 100) + offset;
	}
}
