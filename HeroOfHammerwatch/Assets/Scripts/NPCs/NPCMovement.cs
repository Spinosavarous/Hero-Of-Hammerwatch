using UnityEngine;
using System.Collections;

public class NPCMovement : MonoBehaviour
{
	[Header("Movement Zone")]
	[SerializeField] private BoxCollider2D movementArea;

	[Header("Movement")]
	[SerializeField] private float moveSpeed = 2f;
	[SerializeField] private float idleMinTime = 1f;
	[SerializeField] private float idleMaxTime = 3f;

	private Animator animator;
	private SpriteRenderer spriteRenderer;
	private Vector3 targetPos;

	private void Start()
	{
		animator = GetComponent<Animator>();
		spriteRenderer = GetComponent<SpriteRenderer>();

		StartCoroutine(WanderRoutine());
	}

	private IEnumerator WanderRoutine()
	{
		while (true)
		{

			SetWalking(false);

			yield return new WaitForSeconds(
				Random.Range(idleMinTime, idleMaxTime)
			);

			targetPos = GetRandomPointInsideBounds();

			SetWalking(true);

			while (Vector2.Distance(
				transform.position,
				targetPos) > 0.05f)
			{
				Vector3 dir =
					(targetPos - transform.position).normalized;

				FaceDirection(dir);

				transform.position =
					Vector2.MoveTowards(
						transform.position,
						targetPos,
						moveSpeed * Time.deltaTime
					);

				yield return null;
			}

			SetWalking(false);
		}
	}

	private Vector3 GetRandomPointInsideBounds()
	{
		Bounds bounds = movementArea.bounds;

		float x = Random.Range(bounds.min.x, bounds.max.x);
		float y = Random.Range(bounds.min.y, bounds.max.y);

		return new Vector3(x, y, transform.position.z);
	}

	private void FaceDirection(Vector3 dir)
	{
		if (spriteRenderer == null)
			return;

		// Look left/right
		if (dir.x > 0.05f)
			spriteRenderer.flipX = true;
		else if (dir.x < -0.05f)
			spriteRenderer.flipX = false;
	}

	private void SetWalking(bool value)
	{
		if (animator != null)
			animator.SetBool("isWalking", value);
	}
}
