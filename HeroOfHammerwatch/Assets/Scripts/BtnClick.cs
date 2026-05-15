using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BtnClick : MonoBehaviour
{
	[SerializeField] private GameObject loading_parent;
	[SerializeField] private TextMeshProUGUI loading_text;

	public void GoBtnClick()
	{
		string nextScene = SceneManager.GetActiveScene().name == "Village"
			? "GameScene"
			: "Village";

		StartCoroutine(LoadSceneWithUI(nextScene));
	}

	IEnumerator LoadSceneWithUI(string sceneName)
	{
		loading_parent.SetActive(true);

		string baseText = "Loading";
		float timer = 0f;
		int dots = 0;

		if (SceneManager.GetActiveScene().name == "GameScene")
		{
			MapSaving.Instance.SaveMap();
		}

		AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
		op.allowSceneActivation = false;

		while (op.progress < 0.9f)
		{
			timer += Time.deltaTime;

			if (timer >= 0.5f)
			{
				timer = 0f;
				dots = (dots + 1) % 4;
				loading_text.text = baseText + new string('.', dots);
			}

			yield return null;
		}

		yield return new WaitForSeconds(0.5f);

		op.allowSceneActivation = true;
	}

	public void RestartGame()
	{
		StartCoroutine(APIManager.Instance.FullReset(success =>
		{
			if (success)
			{
				Debug.Log("Player data fully reset!");

				PlayerDataManager.Instance.worldData = null;

				// Reload scene or return to menu
				SceneManager.LoadScene(0);
			}
			else
			{
				Debug.LogError("Failed to reset save");
			}
		}));
	}
}