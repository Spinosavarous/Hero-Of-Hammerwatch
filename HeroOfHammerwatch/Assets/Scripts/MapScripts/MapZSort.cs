using UnityEngine;

public class MapZSort : MonoBehaviour
{
	public int offset = 0;
	public int basePriority = 10;

	private Renderer rend;

	void Awake()
	{
		rend = GetComponent<Renderer>();
	}

	void LateUpdate()
	{
		int ySort = Mathf.RoundToInt(-transform.position.y * 100);
		rend.sortingOrder = basePriority + ySort + offset;
	}
}