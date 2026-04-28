// ======================================
// UNITY LOCAL SAVE DATA
// SaveData.cs
// ======================================

using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
	public int level;
	public int currentXP;
	public int gold;

	public float hp;
	public float maxHp;

	public float posX;
	public float posY;

	public string currentRegion;

	public List<string> openedChests = new();
	public List<string> clearedRegions = new();
	public List<string> destroyedSpawners = new();
	public List<EnemySave> enemies = new();
	public List<SpawnerSave> spawners = new();
}

[Serializable]
public class EnemySave
{
	public string id;

	public string type;

	public float hp;

	public float posX;
	public float posY;

	public bool dead;
}

[Serializable]
public class SpawnerSave
{
	public string id;

	public bool destroyed;

	public float respawnTimer;

	public int aliveCount;

	public int currentHP;
}