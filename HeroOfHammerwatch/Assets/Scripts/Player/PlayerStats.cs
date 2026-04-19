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

	public PlayerStats(int n)
	{
		maxHealth = 100;
		speed = 5f;
		stamina = 100;
		attack = 25f;
		defense = 5f;
		critChance = 0.1f;
	}
}
