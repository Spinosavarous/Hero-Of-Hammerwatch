using UnityEngine;

public class PlayerSavings : MonoBehaviour
{
   private PlayerMovement player;
	void Start()
    {
        player = GetComponent<PlayerMovement>();

		LoadPlayer();
	}

    void Update()
    {
        
    }

    void LoadPlayer()
    {
		var data = SaveManager.Instance.LoadLocal();

		if (data == null)
		{
			Debug.LogWarning("No save data found.");
			return;
		}

		player.playerLevel = data.level;

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

		data.posX = 112;
		data.posY = -60;

		SaveManager.Instance.SaveLocal(data);

		Debug.Log("Player Saved");
	}

	//--------------------------------------------------
	// OPTIONAL AUTO SAVE
	//--------------------------------------------------
	private void OnApplicationQuit()
	{
		SavePlayer();
	}
}
