using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSavings : MonoBehaviour
{
	private PlayerMovement player;

	[SerializeField] private GameObject loading_parent;

	private TextMeshProUGUI loading_text;
	void Start()
    {
		loading_parent.SetActive(true);
        player = GetComponent<PlayerMovement>();

		loading_text = loading_parent.GetComponentInChildren<TextMeshProUGUI>();

		LoadPlayer();

		StartCoroutine(StartWait());
	}

    void Update()
    {
        
    }

	public void LoadPlayer()
	{
		var world = PlayerDataManager.Instance.worldData;
		var upgrades = PlayerDataManager.Instance.upgrades;
		var currency = PlayerDataManager.Instance.currency;

		if (world == null)
		{
			Debug.LogWarning("World not loaded yet.");
			return;
		}

		// -------------------
		// WORLD PLAYER DATA
		// -------------------
		player.playerLevel = world.level;
		player.playerStats.LoadPlayerStats(player.playerLevel);

		player.currentXP = world.currentXP;
		player.currentHp = world.hp;
		player.gold = world.gold;

		if (player.currentHp <= 0)
		{
			player.currentHp = player.playerStats.maxHealth;

			if (SceneManager.GetActiveScene().name.Equals("GameScene"))
			{
				player.transform.position = new Vector2(112, -60);

			}
		}

		// -------------------
		// UPGRADES
		// -------------------
		if (upgrades != null)
		{
			player.playerStats.maxHealth += 10 * upgrades.healthLevel;
			player.playerStats.maxStamina += 10 * upgrades.staminaLevel;
			player.playerStats.attack += 5f * upgrades.attackLevel;
			player.playerStats.defense += 1f * upgrades.armorLevel;
		}

		PlayerDataManager.Instance.isLoaded = true;
	}

	//--------------------------------------------------
	// SAVE PLAYER
	//--------------------------------------------------
	public void SavePlayer()
	{
		var data = SaveManager.Instance.LoadLocal();

		// if no save exists, create new one
		if (data == null)
			data = new SaveData();

		data.level = player.playerLevel;
		data.currentXP = (int)player.currentXP;
		data.hp = player.currentHp;
		data.maxHp = player.playerStats.maxHealth;

		data.gold = player.gold;


		if (SceneManager.GetActiveScene().name.Equals("GameScene"))
		{
			data.posX = 112;
			data.posY = -60;
		}

		SaveManager.Instance.SaveLocal(data);

		Debug.Log("Player Saved");
	}

	//--------------------------------------------------
	// OPTIONAL AUTO SAVE
	//--------------------------------------------------
	private void OnApplicationQuit()
	{

	}


	IEnumerator StartWait()
	{
		loading_parent.SetActive(true);
		loading_text.text = "Loading";

		yield return new WaitForSeconds(1);
		loading_text.text = "Loading .";

		yield return new WaitForSeconds(1);

		loading_text.text = "Loading ..";

		yield return new WaitForSeconds(1);

		loading_text.text = "Loading ...";

		yield return new WaitForSeconds(1.2f);

		loading_parent.SetActive(false);

	}
}
