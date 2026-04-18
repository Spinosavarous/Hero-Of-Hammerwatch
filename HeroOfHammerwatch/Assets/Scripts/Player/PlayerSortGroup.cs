using UnityEngine;

public class PlayerSortGroup : MonoBehaviour
{
	private Renderer[] rends;
	private int[] baseOffsets;

	void Awake()
	{
		rends = GetComponentsInChildren<Renderer>();
		baseOffsets = new int[rends.Length];

		for (int i = 0; i < rends.Length; i++)
		{
			baseOffsets[i] = rends[i].sortingOrder;
		}
	}

	void LateUpdate()
	{
		int baseOrder = (int)(-transform.position.y * 100);

		for (int i = 0; i < rends.Length; i++)
		{
			rends[i].sortingOrder = baseOrder + baseOffsets[i];
		}
	}
}
