// =====================================
// MapSaving.cs
// Put on empty GameObject in scene
// =====================================

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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

	void Awake()
	{
		player = FindAnyObjectByType<PlayerMovement>();
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
		SaveMap();
	}

	// =====================================
	// SAVE WHOLE MAP
	// =====================================

	public void SaveMap()
	{
		EnemySpawner[] spawners =
			FindObjectsOfType<EnemySpawner>(true);

		foreach (EnemySpawner spawner in spawners)
		{
			if (spawner != null)
				spawner.SaveSpawner();
		}

		EnemyMovement[] enemies =
			FindObjectsOfType<EnemyMovement>(true);

		foreach (EnemyMovement enemy in enemies)
		{
			if (enemy != null)
				enemy.SaveEnemy();
		}

		PlayerPrefs.Save();

		SaveManager.Instance.SaveLocal(
			new SaveData {
				level = player.playerLevel,
				currentXP = Convert.ToInt32(player.currentXP),
				gold = Convert.ToInt32(player.gold),
				hp = Convert.ToInt32(player.currentHp),
				maxHp = Convert.ToInt32(player.playerStats.maxHealth),
				posX = player.transform.position.x,
				posY = player.transform.position.y,
				currentRegion = "",
				openedChests = openedChests,
				clearedRegions = regions.Where(r => !r.activeSelf).Select(r => r.name).ToList(),
				destroyedSpawners = spawners.All(s => s.spawnerId == null || s.destroyed) ? spawners.Select(s => s.spawnerId).ToList() : new List<string>(),
				enemies = enemies.Select(e => new EnemySave {
					id = e.enemyId,
					type = e.transform.name.Replace("(Clone)", "").Trim(),
					hp = Convert.ToInt32(e.IsDead() ? 0 : e.currentHp),
					posX = e.transform.position.x,
					posY = e.transform.position.y
				}).ToList(),
				spawners = spawners.Select(s => new SpawnerSave {
					id = s.spawnerId,
					respawnTimer = s.destroyed ? GetSecondsFromText(s.killCounterText.text) : 0f,
					aliveCount = s.destroyed ? 0 : s.aliveEnemies,
					destroyed = s.destroyed,
					currentHP = Convert.ToInt32(s.currentHP)
				}).ToList()
			}
		);

		Debug.Log("Map Saved");
	}

	// =====================================
	// LOAD WHOLE MAP
	// =====================================

	public void LoadMap()
	{
		var data = SaveManager.Instance.LoadLocal();

		if (data == null)
		{
			Debug.LogWarning("No save data found.");
			return;
		}

		//----------------------------------
		// PLAYER LOAD
		//----------------------------------
		player.playerLevel = data.level;

		player.transform.position =
			new Vector2(data.posX, data.posY);

		openedChests = data.openedChests;

		player.LoadPlayerStats(player.playerLevel);

		player.currentXP = data.currentXP;
		player.currentHp = data.hp;
		player.gold = data.gold;

		if (player.currentHp <= 0)
		{
			player.currentHp = player.playerStats.maxHealth;
			player.transform.position =
				new Vector2(112, -60);
		}

		//----------------------------------
		// FIND MAP SPAWNERS (fixed objects)
		//----------------------------------
		EnemySpawner[] spawners =
			FindObjectsOfType<EnemySpawner>(true);

		//----------------------------------
		// LOAD SPAWNER STATS ONLY
		//----------------------------------
		foreach (EnemySpawner spawner in spawners)
		{
			if (spawner == null)
				continue;

			var saveData =
				data.spawners.FirstOrDefault(s =>
					s.id == spawner.spawnerId);

			if (saveData == null)
				continue;

			spawner.currentHP = saveData.currentHP;
			spawner.destroyed = saveData.destroyed;

			if (saveData.destroyed)
			{
				spawner.SpawnerTimeLoad(saveData.respawnTimer);
			}
		}

		//----------------------------------
		// REMOVE EXISTING ENEMIES
		//----------------------------------
		EnemyMovement[] oldEnemies =
			FindObjectsOfType<EnemyMovement>(true);

		foreach (EnemyMovement enemy in oldEnemies)
		{
			if (enemy != null && enemy.tag == "Enemy")
				Destroy(enemy.gameObject);
		}

		//----------------------------------
		// SPAWN SAVED ENEMIES
		//----------------------------------
		foreach (var enemyData in data.enemies)
		{
			if (!enemyData.id.ToLower().Contains("spawner"))
			{
				Debug.LogWarning(
					"Enemy ID does not contain 'spawner': " +
					enemyData.id);

				return;
			}

			GameObject prefab =
				enemyPrefabs.FirstOrDefault(p =>
					p.name == enemyData.type);

			if (prefab == null)
			{
				Debug.LogWarning(
					"Enemy prefab not found: " +
					enemyData.type);
				continue;
			}

			GameObject obj =
				Instantiate(
					prefab,
					new Vector2(
						enemyData.posX,
						enemyData.posY),
					Quaternion.identity
				);

			EnemyMovement enemy =
				obj.GetComponent<EnemyMovement>();

			if (enemy == null)
			{
				Debug.LogWarning(
					"Prefab missing EnemyMovement: " +
					prefab.name);
				Destroy(obj);
				continue;
			}

			enemy.enemyId = enemyData.id;
			enemy.currentHp = enemyData.hp;

			obj.tag = "Enemy";

			if (enemyData.dead || enemyData.hp <= 0)
				obj.SetActive(false);

			obj.GetComponent<EnemyMovement>().enabled = true;
			obj.GetComponent<Collider2D>().enabled = true;
			obj.GetComponent<Rigidbody2D>().simulated = true;
			obj.GetComponent<Animator>().enabled = true;

			for (int i = 0; i < data.clearedRegions.Count; i ++)
			{
				for (int j = 0; j < regions.Count; j ++)
				{
					if (regions[j].name.Trim().Equals(data.clearedRegions[i].Trim()))
					{
						regions[j].SetActive(false);
						break;
					}
				}
			}
		}

		Debug.Log("Map Loaded");
	}

	// =====================================
	// FORCE SAVE BUTTON
	// =====================================

	public void ManualSave()
	{
		SaveMap();
	}

	// =====================================
	// FORCE LOAD BUTTON
	// =====================================

	public void ManualLoad()
	{
		LoadMap();
	}

	// =====================================
	// CLEAR SAVE
	// =====================================

	public void DeleteAllSave()
	{
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();

		Debug.Log("All Save Deleted");
	}

	// =====================================
	// RELOAD SCENE AFTER DELETE
	// =====================================

	public void ResetMap()
	{
		DeleteAllSave();

		SceneManager.LoadScene(
			SceneManager.GetActiveScene().buildIndex
		);
	}

	private float GetSecondsFromText(string text)
	{
		// Remove "Respawn: "
		string time = text.Replace("Respawn: ", "");

		string[] parts = time.Split(':');

		int minutes = int.Parse(parts[0]);
		int seconds = int.Parse(parts[1]);

		return minutes * 60 + seconds;
	}
}