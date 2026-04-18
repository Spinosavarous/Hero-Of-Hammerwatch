using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;
using Newtonsoft.Json;

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
}

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
	public int gems;
}

[Serializable]
public class Upgrades
{
	public int attackLevel;
	public int armorLevel;
	public int staminaLevel;
	public int healthLevel;
}
