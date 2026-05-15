using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class APIManager : MonoBehaviour
{
	public static APIManager Instance;

	[Header("API")]
	[SerializeField] private string baseUrl = "http://localhost:5067";

	private string token;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);

			token = PlayerPrefs.GetString("token", "");
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public void SetToken(string jwt)
	{
		token = jwt;
		PlayerPrefs.SetString("token", token);
		PlayerPrefs.Save();
	}

	public void Logout()
	{
		token = null;
		PlayerPrefs.DeleteKey("token");
	}

	private UnityWebRequest CreateRequest(string endpoint, string method, string jsonBody = null)
	{
		UnityWebRequest request = new UnityWebRequest(baseUrl + endpoint, method);

		if (!string.IsNullOrEmpty(jsonBody))
		{
			byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody); 
			request.uploadHandler = new UploadHandlerRaw(bodyRaw);
		}

		request.downloadHandler = new DownloadHandlerBuffer();
		request.SetRequestHeader("Content-Type", "application/json");

		if (!string.IsNullOrEmpty(token))
		{
			request.SetRequestHeader("Authorization", "Bearer " + token);
		}

		return request;
	}


	public IEnumerator Register(string username, string email, string password, Action<bool, string> callback)
	{
		string json = JsonConvert.SerializeObject(new RegisterModel
		{
			Username = username,
			Email = email,
			Password = password
		});

		Debug.Log(json);

		var request = CreateRequest("/register", "POST", json);

		yield return request.SendWebRequest();

		if (request.result == UnityWebRequest.Result.Success)
		{
			callback(true, request.downloadHandler.text);
		}
		else
		{
			callback(false, request.downloadHandler.text);
		}
	}

	public IEnumerator Login(string username, string password, Action<bool, string> callback)
	{
		string json = JsonConvert.SerializeObject(new LoginModel
		{
			Username = username,
			Password = password
		});

		var request = CreateRequest("/login", "POST", json);

		yield return request.SendWebRequest();

		if (request.result == UnityWebRequest.Result.Success)
		{
			var response = JsonUtility.FromJson<TokenResponse>(request.downloadHandler.text);

			SetToken(response.token);

			callback(true, response.token);
		}
		else
		{
			callback(false, request.error);
		}
	}

	public IEnumerator GetProfile(Action<bool, PlayerProfile> callback)
	{
		var request = CreateRequest("/profile", "GET");

		yield return request.SendWebRequest();

		if (request.result == UnityWebRequest.Result.Success)
		{
			var profile = JsonUtility.FromJson<PlayerProfile>(request.downloadHandler.text);
			callback(true, profile);
		}
		else
		{
			Debug.LogError("Profile error: " + request.error);
			callback(false, null);
		}
	}

	public IEnumerator InitPlayer(Action<bool> callback)
	{
		var request = CreateRequest("/initplayer", "POST");

		yield return request.SendWebRequest();

		if (request.result == UnityWebRequest.Result.Success)
		{
			callback(true);
		}
		else
		{
			Debug.LogError("Init error: " + request.error);
			callback(false);
		}
	}

	#region Upgrades
	public IEnumerator GetUpgrades(Action<bool, Upgrades> callback)
	{
		var request = CreateRequest("/save/upgrades", "GET");

		yield return request.SendWebRequest();

		if (request.result == UnityWebRequest.Result.Success)
		{
			var data = JsonConvert.DeserializeObject<Upgrades>(request.downloadHandler.text);

			callback(true, data);
		} else
		{
			print(request.error);
			callback(false, null);
		}
	}

	public IEnumerator Upgrade(int attackLevel, int armorLevel, int staminaLevel, int healthLevel, Action<bool, Upgrades> callback)
	{
		string json = JsonConvert.SerializeObject(new Upgrades
		{
			attackLevel = attackLevel,
			armorLevel = armorLevel,
			staminaLevel = staminaLevel,
			healthLevel = healthLevel,
		});

		print(json);

		var req = CreateRequest("/save/upgrades", "POST", json);

		yield return req.SendWebRequest();

		if (req.result == UnityWebRequest.Result.Success)
		{
			callback(true, JsonUtility.FromJson<Upgrades>(req.downloadHandler.text));
		} else
		{
			print(req.error);
			callback(false, null);
		}
	}
	#endregion

	public IEnumerator GetCurrrecnies(Action<bool, Currency> callback)	
	{
		var req = CreateRequest("/save/currencies", "GET");

		yield return req.SendWebRequest();

		if (req.result == UnityWebRequest.Result.Success)
		{
			var data = JsonConvert.DeserializeObject<Currency>(req.downloadHandler.text);
			callback(true, data);
		} else
		{
			print(req.error);
			callback(false, null);
		}
	}

	public IEnumerator SaveWorld(WorldSaveData worldData, Action<bool> callback)
	{
		string json = JsonConvert.SerializeObject(worldData);

		Debug.Log("=== SAVE WORLD JSON ===");
		Debug.Log(json);

		var request = CreateRequest("/save/world", "POST", json);

		yield return request.SendWebRequest();

		Debug.Log("=== SAVE RESPONSE ===");
		Debug.Log("Code: " + request.responseCode);
		Debug.Log("Result: " + request.result);
		Debug.Log("Error: " + request.error);
		Debug.Log("Body: " + request.downloadHandler.text);

		if (request.responseCode == 401)
		{
			Debug.LogError("❌ Unauthorized - TOKEN PROBLEM");
			callback(false);
			yield break;
		}

		if (request.result == UnityWebRequest.Result.Success)
		{
			callback(true);
		}
		else
		{
			callback(false);
		}
	}

	public IEnumerator LoadWorld(Action<bool, WorldSaveData> callback)
	{
		var request = CreateRequest("/save/load", "GET");

		yield return request.SendWebRequest();

		if (request.result == UnityWebRequest.Result.Success)
		{
			Debug.Log("Loaded World: " + request.downloadHandler.text);

			var world = JsonConvert.DeserializeObject<WorldSaveData>(request.downloadHandler.text);

			callback(true, world);
		}
		else
		{
			Debug.LogError("Load World Error: " + request.error);
			callback(false, null);
		}
	}

	public IEnumerator LoadAllData(Action<bool> callback)
	{
		yield return APIManager.Instance.LoadWorld((success, world) =>
		{
			if (!success)
			{
				callback(false);
				return;
			}

			PlayerDataManager.Instance.worldData = world;
		});

		yield return APIManager.Instance.GetUpgrades((success, upgrades) =>
		{
			if (!success)
			{
				callback(false);
				return;
			}

			PlayerDataManager.Instance.upgrades = upgrades;
		});

		yield return APIManager.Instance.GetCurrrecnies((success, currency) =>
		{
			if (!success)
			{
				callback(false);
				return;
			}

			PlayerDataManager.Instance.currency = currency;
		});

		PlayerDataManager.Instance.isLoaded = true;

		callback(true);
	}

	public IEnumerator SaveWorldCoroutine(WorldSaveData worldData)
	{
		string json = JsonConvert.SerializeObject(worldData);

		var request = CreateRequest("/save/world", "POST", json);

		yield return request.SendWebRequest();

		Debug.Log("SAVE RESPONSE: " + request.downloadHandler.text);

		if (request.result != UnityWebRequest.Result.Success)
		{
			Debug.LogError("Save failed: " + request.error);
		}
	}

	// -----------------------------------
	// FULL RESET SAVE DATA
	// -----------------------------------
	public IEnumerator FullReset(Action<bool> callback)
	{
		var request = CreateRequest("/save/full-reset", "DELETE");

		yield return request.SendWebRequest();

		Debug.Log("=== FULL RESET RESPONSE ===");
		Debug.Log("Code: " + request.responseCode);
		Debug.Log("Result: " + request.result);
		Debug.Log("Error: " + request.error);
		Debug.Log("Body: " + request.downloadHandler.text);

		if (request.result == UnityWebRequest.Result.Success)
		{
			callback(true);
		}
		else
		{
			callback(false);
		}
	}
}

#region DTOs
[Serializable]
public class RegisterModel
{
	public string Username;
	public string Email;
	public string Password;
}

[Serializable]
public class LoginModel
{
	public string Username;
	public string Password;
}

[Serializable]
public class TokenResponse
{
	public string token;
}

[Serializable]
public class PlayerProfile
{
	public string username;
	public int health;
	public int bandages;
	public Currency currency;
	public Upgrades upgrades;
}

[Serializable]
public class Currency
{
	public int coins;
}

[Serializable]
public class Upgrades
{
	public int attackLevel;
	public int armorLevel;
	public int staminaLevel;
	public int healthLevel;
}

[Serializable]
public class WorldSaveData
{
	public int level;
	public int currentXP;
	public int gold;

	public float hp;
	public float maxHp;

	public float posX;
	public float posY;

	public string currentRegion;

	public List<string> openedChests;
	public List<string> clearedRegions;
	public List<string> destroyedSpawners;

	public List<EnemyData> enemies;
	public List<SpawnerData> spawners;
}

[Serializable]
public class EnemyData
{
	public string id;
	public string type;
	public float hp;
	public float posX;
	public float posY;
	public bool dead;
}

[Serializable]
public class SpawnerData
{
	public string id;
	public bool destroyed;
	public float respawnTimer;
	public int aliveCount;
	public int currentHP;
}
#endregion