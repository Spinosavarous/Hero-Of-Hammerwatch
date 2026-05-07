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
}