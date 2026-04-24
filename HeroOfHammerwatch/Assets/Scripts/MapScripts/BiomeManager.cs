using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class BiomeManager : MonoBehaviour
{
	public static BiomeManager Instance;

	[System.Serializable]
	public class BiomeEntry
	{
		public string id;
		public GameObject biomeObject;
		public PlayableDirector director;
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
				StartCoroutine(ShowDirector(biome.director, biome.biomeObject));
				return;
			}
		}

		Debug.LogWarning("Biome not found: " + id);
	}

	IEnumerator ShowDirector(PlayableDirector director, GameObject biome)
	{	
		director.Play();

		while (director.state == PlayState.Playing)
		{
			yield return null;
		}

		director.gameObject.SetActive(false);

		yield return new WaitForSeconds(4.5f);
		
		biome.SetActive(false);
	}
}