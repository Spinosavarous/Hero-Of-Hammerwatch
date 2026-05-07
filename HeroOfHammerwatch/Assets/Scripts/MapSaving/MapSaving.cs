using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSaving : MonoBehaviour
{
	public static MapSaving Instance;

	[Header("Auto Save")]
	[SerializeField] private float autoSaveEvery = 15f;

	[Header("Opened Chests")]
	public List<string> openedChests = new List<string>();

	[Header("Enemy Prefabs")]
	[SerializeField] private GameObject[] enemyPrefabs;

	[Header("Regions")]
	[SerializeField] private List<GameObject> regions;

	private float timer;
	private PlayerMovement player;

	// =====================================
	// INIT
	// =====================================

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
			return;
		}
	}

	void Start()
	{
		LoadMap();
	}

	void Update()
	{
		timer += Time.deltaTime;

		if (timer >= autoSaveEvery)
		{
			timer = 0f;
			SaveMap();
		}
	}

	void OnApplicationQuit()
	{
		StartCoroutine(SaveBeforeQuit());
	}

	// =====================================
	// SAFE SAVE BEFORE QUIT
	// =====================================

	private IEnumerator SaveBeforeQuit()
	{
		yield return SaveMapCoroutine();
	}

	// =====================================
	// SAVE WHOLE MAP
	// =====================================

	public void SaveMap()
	{
		StartCoroutine(SaveMapCoroutine());
	}

	private IEnumerator SaveMapCoroutine()
	{
		player = FindAnyObjectByType<PlayerMovement>();

		if (player == null)
		{
			Debug.LogWarning("No player found - cannot save");
			yield break;
		}

		Debug.Log("Saving map...");

		EnemySpawner[] spawners = FindObjectsOfType<EnemySpawner>(true);
		EnemyMovement[] enemies = FindObjectsOfType<EnemyMovement>(true);

		foreach (var spawner in spawners)
			spawner?.SaveSpawner();

		foreach (var enemy in enemies)
			enemy?.SaveEnemy();

		var worldData = new WorldSaveData
		{
			level = player.playerLevel,
			currentXP = Convert.ToInt32(player.currentXP),
			gold = Convert.ToInt32(player.gold),

			hp = player.currentHp,
			maxHp = player.playerStats.maxHealth,

			posX = player.transform.position.x,
			posY = player.transform.position.y,

			currentRegion = "",

			openedChests = openedChests,

			clearedRegions = regions
				.Where(r => !r.activeSelf)
				.Select(r => r.name)
				.ToList(),

			destroyedSpawners = spawners
				.Where(s => s.destroyed)
				.Select(s => s.spawnerId)
				.ToList(),

			enemies = enemies.Select(e => new EnemyData
			{
				id = e.enemyId,
				type = e.transform.name.Replace("(Clone)", "").Trim(),
				hp = e.IsDead() ? 0 : e.currentHp,
				posX = e.transform.position.x,
				posY = e.transform.position.y,
				dead = e.IsDead()
			}).ToList(),

			spawners = spawners.Select(s => new SpawnerData
			{
				id = s.spawnerId,
				destroyed = s.destroyed,
				respawnTimer = s.destroyed ? GetSecondsFromText(s.killCounterText.text) : 0f,
				aliveCount = s.destroyed ? 0 : s.aliveEnemies,
				currentHP = Convert.ToInt32(s.currentHP)
			}).ToList()
		};

		// 🔥 CRITICAL FIX → sync global memory
		PlayerDataManager.Instance.worldData = worldData;

		// 🔥 SAVE TO SERVER (wait for it)
		yield return APIManager.Instance.SaveWorldCoroutine(worldData);

		Debug.Log("Map Saved to SERVER");
	}

	// =====================================
	// LOAD MAP
	// =====================================

	public void LoadMap()
	{
		StartCoroutine(WaitForDataThenLoad());
	}

	private IEnumerator WaitForDataThenLoad()
	{
		yield return new WaitUntil(() =>
			PlayerDataManager.Instance != null &&
			PlayerDataManager.Instance.isLoaded
		);

		player = FindAnyObjectByType<PlayerMovement>();

		if (player == null)
		{
			Debug.LogWarning("Player not found during load");
			yield break;
		}

		var data = PlayerDataManager.Instance.worldData;

		if (data == null)
		{
			Debug.LogWarning("No world data found.");
			yield break;
		}

		ApplyWorld(data);
	}

	// =====================================
	// APPLY WORLD
	// =====================================

	private void ApplyWorld(WorldSaveData data)
	{
		//----------------------------------
		// SPAWNERS
		//----------------------------------
		var spawners = FindObjectsOfType<EnemySpawner>(true);

		foreach (var spawner in spawners)
		{
			var saveData = data.spawners
				.FirstOrDefault(s => s.id == spawner.spawnerId);

			if (saveData == null) continue;

			spawner.currentHP = saveData.currentHP;
			spawner.destroyed = saveData.destroyed;

			if (saveData.destroyed)
				spawner.SpawnerTimeLoad(saveData.respawnTimer);
		}

		//----------------------------------
		// REMOVE OLD ENEMIES
		//----------------------------------
		foreach (var enemy in FindObjectsOfType<EnemyMovement>(true))
		{
			if (enemy.CompareTag("Enemy"))
				Destroy(enemy.gameObject);
		}

		//----------------------------------
		// SPAWN ENEMIES
		//----------------------------------
		foreach (var enemyData in data.enemies)
		{
			GameObject prefab = enemyPrefabs
				.FirstOrDefault(p => p.name == enemyData.type);

			if (prefab == null)
			{
				Debug.LogWarning("Missing prefab: " + enemyData.type);
				continue;
			}

			var obj = Instantiate(prefab,
				new Vector2(enemyData.posX, enemyData.posY),
				Quaternion.identity);

			var enemy = obj.GetComponent<EnemyMovement>();

			enemy.enemyId = enemyData.id;
			enemy.currentHp = enemyData.hp;

			if (obj.name.Contains("Clone") || obj.name.ToLower().Contains("spawner"))
			{
				obj.tag = "Enemy";
			}
			
			obj.SetActive(!(enemyData.dead || enemyData.hp <= 0));
		}

		//----------------------------------
		// REGIONS
		//----------------------------------
		foreach (var region in regions)
		{
			if (data.clearedRegions.Contains(region.name))
				region.SetActive(false);
		}

		Debug.Log("Map Loaded from GLOBAL DATA");
	}

	// =====================================
	// HELPERS
	// =====================================

	private float GetSecondsFromText(string text)
	{
		string time = text.Replace("Respawn: ", "");
		string[] parts = time.Split(':');

		int minutes = int.Parse(parts[0]);
		int seconds = int.Parse(parts[1]);

		return minutes * 60 + seconds;
	}

	// =====================================
	// MANUAL CONTROLS
	// =====================================

	public void ManualSave()
	{
		SaveMap();
	}

	public void ManualLoad()
	{
		LoadMap();
	}

	public void ResetMap()
	{
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();

		SceneManager.LoadScene(
			SceneManager.GetActiveScene().buildIndex
		);
	}
}