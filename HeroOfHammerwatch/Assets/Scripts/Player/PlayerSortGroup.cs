using UnityEngine;

public class PlayerSortGroup : MonoBehaviour
{
	public int basePriority = 10;

	private Renderer[] rends;
	private int[] baseOffsets;

	void Awake()
	{
		rends = GetComponentsInChildren<Renderer>();
		baseOffsets = new int[rends.Length];

		for (int i = 0; i < rends.Length; i++)
			baseOffsets[i] = rends[i].sortingOrder;
	}

	void LateUpdate()
	{
		int ySort = Mathf.RoundToInt(-transform.position.y * 100);
		int rootOrder = basePriority + ySort;

		for (int i = 0; i < rends.Length; i++)
			rends[i].sortingOrder = rootOrder + baseOffsets[i];
	}
}