using System.Collections;
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

	[Header("Effects")]
	[SerializeField] private ParticleSystem destroyEffect;

	private int aliveEnemies = 0;
	private bool destroyed = false;

	private Coroutine spawnRoutine;

	// ---------------- START ----------------

	void Awake()
	{
		currentHP = maxHP;
	}

	void Start()
	{
		spawnRoutine = StartCoroutine(SpawnLoop());
	}

	// ---------------- SPAWN LOOP ----------------

	private IEnumerator SpawnLoop()
	{
		while (!destroyed)
		{
			yield return new WaitForSeconds(spawnInterval);

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
		if (destroyed)
			return;

		currentHP -= damage;

		if (currentHP <= 0)
		{
			currentHP = 0;
			DestroyNest();
		}
	}

	// ---------------- DESTROY ----------------

	private void DestroyNest()
	{
		destroyed = true;

		if (spawnRoutine != null)
			StopCoroutine(spawnRoutine);

		if (destroyEffect != null)
			destroyEffect.Play();

		CheckAreaClear();
	}

	// ---------------- CALLED BY ENEMY ----------------

	public void EnemyDied()
	{
		aliveEnemies--;

		if (aliveEnemies < 0)
			aliveEnemies = 0;

		CheckAreaClear();
	}

	// ---------------- CLEAR CHECK ----------------

	private void CheckAreaClear()
	{
		if (destroyed && aliveEnemies <= 0)
		{
			OnAreaCleared();
		}
	}

	// ---------------- CLEARED ----------------

	private void OnAreaCleared()
	{
		Debug.Log(name + " area cleared!");

		// Example:
		// spawn loot
		// open gate
		// give xp
		// destroy object

		Destroy(gameObject, 1f);
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
	}
}
