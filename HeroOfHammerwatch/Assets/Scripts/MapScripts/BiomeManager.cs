using System.Collections.Generic;
using UnityEngine;

public class BiomeManager : MonoBehaviour
{
	public static BiomeManager Instance;

	[System.Serializable]
	public class BiomeEntry
	{
		public string id;
		public GameObject biomeObject;
	}

	[SerializeField] private List<BiomeEntry> biomes = new List<BiomeEntry>();

	private HashSet<string> unlockedBiomes = new HashSet<string>();

	private void Awake()
	{
		Instance = this;
	}

	public void UnlockBiome(string id)
	{
		if (unlockedBiomes.Contains(id))
			return;

		unlockedBiomes.Add(id);

		Debug.Log("Unlocked biome: " + id);

		foreach (var biome in biomes)
		{
			if (biome.id == id && biome.biomeObject != null)
			{
				biome.biomeObject.SetActive(false);
				return;
			}
		}

		Debug.LogWarning("Biome not found: " + id);
	}
}