using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ChestScript : MonoBehaviour
{
	[Header("Rewards")]
	[SerializeField] private int goldAmount = 50;
	[SerializeField] private string keyId;

	[Header("Interaction")]
	[SerializeField] private Animator animator;
	[SerializeField] private GameObject interactText;

	private bool playerInRange;
	private bool isOpened;
	private PlayerMovement player;

	void Awake()
	{
		if (animator == null)
			animator = GetComponent<Animator>();

		if (interactText != null)
			interactText.SetActive(false);
	}

	void Update()
	{
		if (isOpened)
			return;

		if (playerInRange && interactText != null)
			interactText.SetActive(true);

		if (!playerInRange && interactText != null)
			interactText.SetActive(false);

		if (!playerInRange)
			return;

		if (Keyboard.current.eKey.wasPressedThisFrame)
		{
			OpenChest();
		}
	}

	private void OpenChest()
	{
		isOpened = true;

		if (interactText != null)
			interactText.SetActive(false);

		animator.SetBool("IsOpened", true);

		player.AddGold(goldAmount);

		BiomeManager.Instance.UnlockBiome(keyId);

		Debug.Log($"Chest opened! +{goldAmount} gold, key: {keyId}");
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.collider.CompareTag("Player"))
		{
			playerInRange = true;
			player = collision.collider.GetComponent<PlayerMovement>();
		}
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		if (collision.collider.CompareTag("Player"))
		{
			playerInRange = false;
			player = null;

			if (interactText != null)
				interactText.SetActive(false);
		}
	}
}