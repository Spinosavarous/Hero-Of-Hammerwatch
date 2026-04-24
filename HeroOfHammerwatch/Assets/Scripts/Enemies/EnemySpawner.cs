using System.Collections;
using TMPro;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
	[Header("Nest Stats")]
	[SerializeField] private float maxHP = 150f;
	[SerializeField] private float currentHP;

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
	[SerializeField] private GameObject[] glowObjects; // Assign the two children here

	[Header("Effects")]
	[SerializeField] private ParticleSystem destroyEffect;

	[Header("UI")]
	[SerializeField] private TextMeshPro killCounterText;

	[Header("Activation")]
	[SerializeField] private float activationRange = 15f;
	[SerializeField] private bool showActivationRange = true;

	private bool playerInRange = false;

	private GameObject Player;
	private int aliveEnemies = 0;
	private int enemiesKilled = 0;
	private bool destroyed = false;
	private bool isRespawning = false;

	private Coroutine spawnRoutine;
	private Coroutine respawnRoutine;

	// ---------------- START ----------------

	void Awake()
	{
		currentHP = maxHP;
	}

	void Start()
	{
		Player = GameObject.FindGameObjectWithTag("Player");
		spawnRoutine = StartCoroutine(SpawnLoop());

		UpdateKillCounterText();
	}

	void Update()
	{
		if (Player == null) return;
		if (destroyed || isRespawning) return;

		float distance = Vector2.Distance(transform.position, Player.transform.position);
		playerInRange = distance <= activationRange;

		if (killCounterText != null)
		{
			// Only show counter when player is in range and nest is active
			killCounterText.gameObject.SetActive(playerInRange);
		}
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
			spawnPoint != null
			? spawnPoint.position
			: transform.position;

		GameObject enemy =
			Instantiate(prefab, pos, Quaternion.identity);

		enemy.tag = "Enemy";

		aliveEnemies++;

		EnemyMovement em =
			enemy.GetComponent<EnemyMovement>();

		if (em != null)
			em.SetSpawner(this);
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
	}

	// ---------------- DESTROY (Start Respawn) ----------------

	private void DestroyNest()
	{
		if (destroyed || isRespawning)
			return;

		destroyed = true;

		if (spawnRoutine != null)
			StopCoroutine(spawnRoutine);

		CheckAreaClear();
	}

	// ---------------- CALLED BY ENEMY ----------------

	public void EnemyDied()
	{
		aliveEnemies--;
		enemiesKilled++;

		if (aliveEnemies < 0)
			aliveEnemies = 0;

		UpdateKillCounterText();

		if (destroyWhenEnemiesKilled && !destroyed && !isRespawning && enemiesKilled >= enemiesToKillForDestruction)
		{
			DestroyNest();
		}

		CheckAreaClear();
	}

	private void UpdateKillCounterText()
	{
		if (killCounterText != null)
		{
			killCounterText.text = $"{enemiesKilled} / {enemiesToKillForDestruction}";
		}
	}

	// ---------------- CLEAR CHECK ----------------

	private void CheckAreaClear()
	{
		if (destroyed && aliveEnemies <= 0)
		{
			OnAreaCleared();
		}
	}

	// ---------------- CLEARED (Start Respawn Timer) ----------------

	private void OnAreaCleared()
	{
		Debug.Log(name + " area cleared! Respawning in " + string.Format("{0} seconds.", respawnTime));

		if (destroyEffect != null)
			destroyEffect.Play();

		// Disable glow visuals
		SetGlowActive(false);

		// Start respawn timer
		if (respawnRoutine != null)
			StopCoroutine(respawnRoutine);
		respawnRoutine = StartCoroutine(RespawnTimer());
	}

	private IEnumerator RespawnTimer()
	{
		isRespawning = true;
		float timer = respawnTime;

		// Update UI to show respawn timer
		while (timer > 0)
		{
			if (killCounterText != null)
			{
				int totalSeconds = Mathf.CeilToInt(timer);

				int minutes = totalSeconds / 60;
				int seconds = totalSeconds % 60;

				killCounterText.text = $"Respawn: {minutes:00}:{seconds:00}";
				killCounterText.gameObject.SetActive(true); // Show even if player out of range
			}

			yield return new WaitForSeconds(1f);
			timer -= 1f;
		}

		RespawnNest();
	}

	private void RespawnNest()
	{
		Debug.Log(name + " respawned!");

		// Reset state
		destroyed = false;
		isRespawning = false;
		currentHP = maxHP;
		enemiesKilled = 0;
		aliveEnemies = 0;

		// Re-enable glow visuals
		SetGlowActive(true);

		// Update UI
		UpdateKillCounterText();

		// Restart spawning
		if (spawnRoutine != null)
			StopCoroutine(spawnRoutine);
		spawnRoutine = StartCoroutine(SpawnLoop());
	}

	private void SetGlowActive(bool active)
	{
		foreach (GameObject obj in glowObjects)
		{
			if (obj != null)
				obj.SetActive(active);
		}
	}

	// ---------------- DEBUG ----------------

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
}