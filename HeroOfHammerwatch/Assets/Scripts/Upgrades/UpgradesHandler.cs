using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradesHandler : MonoBehaviour
{
    [Serializable]
    public class UpgradeEntity
    {
        public string upgradeName;
        
        //texts
        public TextMeshProUGUI current_data;
        public TextMeshProUGUI cost;

        //btns
        public Button upgrade_btn;

        public int upgrade_count;
    }

    [Header("Upgrade Options")]
    [SerializeField] private List<UpgradeEntity> upgrades = new();

    [Header("Player")]
    [SerializeField] private PlayerMovement player;
	[SerializeField] private PlayerSavings playerSavings;

    void Start()
    {
		upgrades.ForEach(u => u.upgrade_btn.onClick.RemoveAllListeners());
	}

    void Update()
    {
        
    }

	public void ClickBtn(int i)
	{
		if (player.gold >= Convert.ToInt16(upgrades[i].cost.text))
		{
			player.gold -= Convert.ToInt16(upgrades[i].cost.text);

			PlayerDataManager.Instance.currency.coins = player.gold;

			upgrades[i].upgrade_count++;
			StartCoroutine(APIManager.Instance.Upgrade(upgrades[0].upgrade_count, upgrades[1].upgrade_count, upgrades[2].upgrade_count, upgrades[3].upgrade_count, (success, result) =>
			{
				if (success)
				{
					float stat = 0;
					switch (i)
					{
						case 0:
							player.playerStats.attack += 5;

							stat = player.playerStats.attack;
							break;
						case 1:
							player.playerStats.defense += 3;

							stat = player.playerStats.defense;
							break;
						case 2:
							player.playerStats.maxStamina += 10;

							stat = player.playerStats.stamina;
							break;
						case 3:
							player.playerStats.maxHealth += 10;

							stat = player.playerStats.maxHealth;
							break;
					}

					upgrades[i].cost.text = (10 + upgrades[i].upgrade_count * upgrades[i].upgrade_count * 10).ToString();
					upgrades[i].current_data.text = "Current " + upgrades[i].upgradeName + ": " + stat.ToString();
				}
			}));
		}
		else
		{
			StartCoroutine(DisableBtn(upgrades[i].upgrade_btn));
		}
	}

	public void LoadPage()
	{
		StartCoroutine(APIManager.Instance.GetUpgrades((success, upgrades_data) =>
		{
			if (success)
			{
				upgrades[0].upgrade_count = upgrades_data.attackLevel;
				upgrades[1].upgrade_count = upgrades_data.armorLevel;
				upgrades[2].upgrade_count = upgrades_data.staminaLevel;
				upgrades[3].upgrade_count = upgrades_data.healthLevel;

				for (int i = 0; i < upgrades.Count; i ++)
				{
					upgrades[i].cost.text = (10 + upgrades[i].upgrade_count * upgrades[i].upgrade_count * 10).ToString();
				}

				upgrades[0].current_data.text = "Current " + upgrades[0].upgradeName + ": " + player.playerStats.attack.ToString();
				upgrades[1].current_data.text = "Current " + upgrades[1].upgradeName + ": " + player.playerStats.defense.ToString();
				upgrades[2].current_data.text = "Current " + upgrades[2].upgradeName + ": " + player.playerStats.maxStamina.ToString();
				upgrades[3].current_data.text = "Current " + upgrades[3].upgradeName + ": " + player.playerStats.maxHealth.ToString();

			}
			else
			{

			}
		}));
	}

	IEnumerator DisableBtn(Button btn)
	{
		btn.enabled = false;
		yield return new WaitForSeconds(2);

		btn.enabled = true;
	}
}
