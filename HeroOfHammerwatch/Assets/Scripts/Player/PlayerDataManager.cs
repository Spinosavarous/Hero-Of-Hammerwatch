using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
	public static PlayerDataManager Instance;

	public WorldSaveData worldData;
	public Upgrades upgrades;
	public Currency currency;

	public bool isLoaded = false;

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}
}
