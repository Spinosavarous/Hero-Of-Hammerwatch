using System;
using UnityEngine;

public class Shops : MonoBehaviour
{
    [Serializable]
    public class ShopPlace
    {
        public GameObject place;
        public GameObject ui;
	}

	[Header("Collision Objects")]
    [SerializeField] private ShopPlace[] objects;

    private UpgradesHandler upgradesHandler;
    void Start()
    {
        upgradesHandler = FindAnyObjectByType<UpgradesHandler>();
    }

    void Update()
    {
        
    }

	private void OnCollisionEnter2D(Collision2D collision)
	{
		foreach (var shop in objects)
        {
            if (collision.gameObject == shop.place)
            {
                shop.ui.SetActive(true);

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                upgradesHandler.LoadPage();
			}
		}
	}

    private void OnCollisionExit2D(Collision2D collision)
    {
        foreach (var shop in objects)
        {
            if (collision.gameObject == shop.place)
            {
                shop.ui.SetActive(false);

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
			}
        }
	}

	public void SaveWorldFromGlobal()
	{
		var world = PlayerDataManager.Instance.worldData;
		var player = FindAnyObjectByType<PlayerMovement>();

		if (world == null || player == null)
		{
			Debug.LogWarning("Missing world or player");
			return;
		}

		// 🔥 ONLY update player-related fields
		world.level = player.playerLevel;
		world.currentXP = (int)player.currentXP;
		world.gold = (int)player.gold;

		world.hp = player.currentHp;
		world.maxHp = player.playerStats.maxHealth;

		// DO NOT TOUCH:
		// enemies
		// spawners
		// regions
		// openedChests

		StartCoroutine(APIManager.Instance.SaveWorld(world, success =>
		{
			if (success)
				Debug.Log("Saved from non-map scene");
			else
				Debug.LogError("Save failed");
		}));
	}

	private void OnApplicationQuit()
	{
		SaveWorldFromGlobal();
	}
}
