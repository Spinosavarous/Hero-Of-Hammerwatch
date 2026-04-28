using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerStats : MonoBehaviour
{
	public int maxHealth;

	public float speed;
	public float stamina;

	public float attack;

	public float defense;
	public float critChance;

	public void LoadPlayerStats(int level)
	{
		maxHealth = 100 + level * 10;
		speed = 5f + level * 0.05f;
		stamina = 100 + level * 5;
		attack = 25f + level * 2f;
		defense = 5f + level * 1f;
		critChance = 0.1f + level * 0.02f;
	}
}
