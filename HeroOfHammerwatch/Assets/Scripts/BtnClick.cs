using UnityEngine;
using UnityEngine.SceneManagement;

public class BtnClick : MonoBehaviour
{
	public void GoBtnClick()
	{
		if (SceneManager.GetActiveScene().name.Equals("Village"))
		{
			SceneManager.LoadScene("GameScene");
		}
		else if (SceneManager.GetActiveScene().name.Equals("GameScene"))
		{
			SceneManager.LoadScene("Village");
		}
	}
}
