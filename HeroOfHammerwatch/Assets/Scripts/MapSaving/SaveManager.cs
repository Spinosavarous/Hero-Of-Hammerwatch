// ======================================
// UNITY LOCAL SAVE MANAGER
// SaveManager.cs
// ======================================

using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
	public static SaveManager Instance;

	private string path;

	void Awake()
	{
		Instance = this;
		path = Application.dataPath + "/save.json"; //Application.persistentDataPath

		print("Save Path: " + path);
	}

	public void SaveLocal(SaveData data)
	{
		string json = JsonUtility.ToJson(data, true);
		File.WriteAllText(path, json);
	}

	public SaveData LoadLocal()
	{
		if (!File.Exists(path))
			return null;

		string json = File.ReadAllText(path);
		return JsonUtility.FromJson<SaveData>(json);
	}

	public void DeleteLocal()
	{
		if (File.Exists(path))
			File.Delete(path);
	}
}