using System.Collections;
using TMPro;
using UnityEngine;

[System.Serializable]
public class SpawnerSaveData
{
	public string id;
	public int spawnCounter;
	public bool destroyed;
	public int aliveEnemies;
	public int enemiesKilled;
	public float currentHP;
}

public class EnemySpawner : MonoBehaviour
{
	[Header("Save")]
	[SerializeField] public string spawnerId;

	[Header("Nest Stats")]
	[SerializeField] private float maxHP = 150f;
	[SerializeField] public float currentHP;

	[Header("Spawning")]
	[SerializeField] private GameObject[] enemyPrefabs;
	[SerializeField] private Transform spawnPoint;
	[SerializeField] private float spawnInterval = 3f;
	[SerializeField] private int maxAliveEnemies = 5;

	[Header("Destroy Settings")]
	[SerializeField] private int enemiesToKillForDestruction = 10;
	[SerializeField] private bool destroyWhenEnemiesKilled = true;

	[Header("Respawn Settings")]
	[SerializeField] private float respawnTime = 10f;
	[SerializeField] private GameObject[] glowObjects;

	[Header("Effects")]
	[SerializeField] private ParticleSystem destroyEffect;

	[Header("UI")]
	[SerializeField] public TextMeshPro killCounterText;

	[Header("Activation")]
	[SerializeField] private float activationRange = 15f;
	[SerializeField] private bool showActivationRange = true;

	private GameObject Player;

	public int aliveEnemies = 0;
	private int enemiesKilled = 0;
	private int spawnCounter = 0;

	public bool destroyed = false;
	private bool isRespawning = false;
	private bool playerInRange = false;

	private Coroutine spawnRoutine;
	private Coroutine respawnRoutine;

	// ---------------- AWAKEN ----------------

	void Awake()
	{
		if (string.IsNullOrEmpty(spawnerId))
			spawnerId = gameObject.scene.name + "_" + gameObject.name;

		currentHP = maxHP;
	}

	void Start()
	{
		Player = GameObject.FindGameObjectWithTag("Player");

		if (!destroyed)
			spawnRoutine = StartCoroutine(SpawnLoop());

		UpdateKillCounterText();
	}

	void Update()
	{
		if (Player == null) return;
		if (isRespawning) return;

		float distance = Vector2.Distance(transform.position, Player.transform.position);
		playerInRange = distance <= activationRange;

		/*if (killCounterText != null)
			killCounterText.gameObject.SetActive(playerInRange && !destroyed);*/
	}

	// ---------------- SPAWN LOOP ----------------

	private IEnumerator SpawnLoop()
	{
		while (!destroyed && !isRespawning)
		{
			yield return new WaitForSeconds(spawnInterval);

			if (!playerInRange)
				continue;

			if (aliveEnemies >= maxAliveEnemies)
				continue;

			SpawnEnemy();
		}
	}

	// ---------------- SPAWN ----------------

	private void SpawnEnemy()
	{
		if (enemyPrefabs.Length == 0)
			return;

		GameObject prefab =
			enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

		Vector3 pos =
			spawnPoint != null ? spawnPoint.position : transform.position;

		GameObject enemy =
			Instantiate(prefab, pos, Quaternion.identity);

		enemy.tag = "Enemy";

		aliveEnemies++;
		spawnCounter++;

		string id = spawnerId + "_enemy_" + spawnCounter;

		EnemyMovement em = enemy.GetComponent<EnemyMovement>();
		if (em != null)
		{
			em.SetSpawner(this);
			em.SetEnemyId(id);
		}

		SaveSpawner();
	}

	// ---------------- DAMAGE ----------------

	public void TakeDamage(float damage)
	{
		if (destroyed || isRespawning)
			return;

		currentHP -= damage;

		if (currentHP <= 0)
		{
			currentHP = 0;
			DestroyNest();
		}

		SaveSpawner();
	}

	// ---------------- DESTROY ----------------

	public void DestroyNest()
	{
		if (destroyed || isRespawning)
			return;

		destroyed = true;

		if (spawnRoutine != null)
			StopCoroutine(spawnRoutine);

		CheckAreaClear();

		SaveSpawner();
	}

	// ---------------- ENEMY DEATH ----------------

	public void EnemyDied()
	{
		aliveEnemies--;
		enemiesKilled++;

		if (aliveEnemies < 0)
			aliveEnemies = 0;

		UpdateKillCounterText();
		SaveSpawner();

		if (destroyWhenEnemiesKilled &&
			!destroyed &&
			enemiesKilled >= enemiesToKillForDestruction)
		{
			DestroyNest();
		}

		CheckAreaClear();
	}

	// ---------------- UI ----------------

	private void UpdateKillCounterText()
	{
		if (killCounterText != null)
			killCounterText.text = $"{enemiesKilled} / {enemiesToKillForDestruction}";
	}

	// ---------------- AREA CHECK ----------------

	private void CheckAreaClear()
	{
		if (destroyed && aliveEnemies <= 0)
			OnAreaCleared();
	}

	private void OnAreaCleared()
	{
		if (destroyEffect != null)
			destroyEffect.Play();

		SetGlowActive(false);

		if (respawnRoutine != null)
			StopCoroutine(respawnRoutine);

		respawnRoutine = StartCoroutine(RespawnTimer(respawnTime));
	}

	public void SpawnerTimeLoad(float time)
	{
		SetGlowActive(false);

		if (respawnRoutine != null)
			StopCoroutine(respawnRoutine);

		respawnRoutine = StartCoroutine(RespawnTimer(time));
	}

	// ---------------- RESPAWN ----------------

	private IEnumerator RespawnTimer(float time)
	{
		isRespawning = true;

		float timer = time;
		while (timer > 0)
		{
			if (killCounterText != null)
			{
				int t = Mathf.CeilToInt(timer);
				killCounterText.text = $"Respawn: {t / 60:00}:{t % 60:00}";
				killCounterText.gameObject.SetActive(true);
			}

			yield return new WaitForSeconds(1f);
			timer--;
		}

		RespawnNest();
	}

	private void RespawnNest()
	{
		destroyed = false;
		isRespawning = false;

		currentHP = maxHP;
		enemiesKilled = 0;
		aliveEnemies = 0;

		SetGlowActive(true);
		UpdateKillCounterText();

		if (spawnRoutine != null)
			StopCoroutine(spawnRoutine);

		spawnRoutine = StartCoroutine(SpawnLoop());

		SaveSpawner();
	}

	private void SetGlowActive(bool active)
	{
		foreach (GameObject obj in glowObjects)
			if (obj != null)
				obj.SetActive(active);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;

		Vector3 pos =
			spawnPoint != null
			? spawnPoint.position
			: transform.position;

		Gizmos.DrawWireSphere(pos, 0.4f);

		if (showActivationRange)
		{
			Gizmos.color = new Color(0, 1, 0, 0.3f);
			Gizmos.DrawWireSphere(transform.position, activationRange);
		}
	}

	// ---------------- SAVE / LOAD ----------------

	public void SaveSpawner()
	{
		SpawnerSaveData data = new SpawnerSaveData
		{
			id = spawnerId,
			spawnCounter = spawnCounter,
			destroyed = destroyed,
			aliveEnemies = aliveEnemies,
			enemiesKilled = enemiesKilled,
			currentHP = currentHP
		};

		PlayerPrefs.SetString(
			"spawner_" + spawnerId,
			JsonUtility.ToJson(data)
		);
	}
}