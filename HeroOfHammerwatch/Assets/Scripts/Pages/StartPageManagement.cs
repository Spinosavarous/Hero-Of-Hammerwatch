using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartPageManagement : MonoBehaviour
{
    [Header("Buttons")]
    //start page
    [SerializeField] private Button _newGameBtn;
	[SerializeField] private Button _contineBtn;
	[SerializeField] private Button _exitBtn;
    //register page
	[SerializeField] private Button _registerBtn;
	[SerializeField] private Button _backBtn;
	//login
	[SerializeField] private Button _loginBtn;
	[SerializeField] private Button _back1Btn;

    [Header("Pages")]
    [SerializeField] private GameObject _homePage;
	[SerializeField] private GameObject _registerPage;
	[SerializeField] private GameObject _loginPage;

	[Header("Input Fields")]
	[SerializeField] private TMP_InputField _loginUsername;
	[SerializeField] private TMP_InputField _loginPassword;

	[SerializeField] private TMP_InputField _registerUsername;
	[SerializeField] private TMP_InputField _registerEmail;
	[SerializeField] private TMP_InputField _registerPassword;

	[SerializeField] private GameObject loading_parent;
	[SerializeField] private Animator animator;
	private TextMeshProUGUI loading_text;

	void Start()
    {
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		loading_text = loading_parent.GetComponentInChildren<TextMeshProUGUI>();

		animator.gameObject.SetActive(false);

		ButtonFunctionsSetUp();
    }

    void Update()
    {
        
    }

    void ButtonFunctionsSetUp()
    {
		_newGameBtn.onClick.AddListener(() =>
		{
			RunWithClick(() =>
			{
				APIManager.Instance.Logout();

				_homePage.SetActive(false);
				_registerPage.SetActive(true);
				_loginPage.SetActive(false);
			});
		});


		_contineBtn.onClick.AddListener(() =>
		{
			RunWithClick(() =>
			{
				StartCoroutine(ContinueFlow());
			});
		});

		_exitBtn.onClick.AddListener(() =>
		{
			RunWithClick(() =>
			{
				Application.Quit();
			});
		});

		_registerBtn.onClick.AddListener(() =>
		{
			RunWithClick(() =>
			{
				StartCoroutine(RegisterFlow());
			});
		});

		_loginBtn.onClick.AddListener(() =>
		{
			RunWithClick(() =>
			{
				StartCoroutine(LoginFlow());
			});
		});

		_backBtn.onClick.AddListener(() =>
		{
			RunWithClick(() =>
			{
				ShowHome();
			});
		});

		_back1Btn.onClick.AddListener(() =>
        {
			RunWithClick(() =>
			{
				ShowHome();
			});
		});
    }

	void ShowHome()
	{
		_homePage.SetActive(true);
		_registerPage.SetActive(false);
		_loginPage.SetActive(false);
	}

	void ShowLogin()
	{
		_homePage.SetActive(false);
		_registerPage.SetActive(false);
		_loginPage.SetActive(true);
	}

	IEnumerator StartWait(string sceneName)
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

	IEnumerator ContinueFlow()
	{
		yield return APIManager.Instance.GetProfile((success, profile) =>
		{
			if (!success)
			{
				ShowLogin();
			}
		});

		yield return APIManager.Instance.LoadAllData(success =>
		{
			if (!success)
				Debug.LogError("Failed loading data");
		});

		yield return StartWait("Village");
	}
	IEnumerator LoginFlow()
	{
		yield return APIManager.Instance.Login(
			_loginUsername.text,
			_loginPassword.text,
			(success, result) =>
			{
				if (!success)
					Debug.LogError(result);
			});

		yield return APIManager.Instance.LoadAllData(success =>
		{
			if (!success)
				Debug.LogError("Failed loading data");
		});

		yield return StartWait("Village");
	}

	IEnumerator RegisterFlow()
	{
		yield return APIManager.Instance.Register(
			_registerUsername.text,
			_registerEmail.text,
			_registerPassword.text,
			(success, result) =>
			{
				if (!success)
					Debug.LogError(result);
			});

		ShowLogin();
	}


	void RunWithClick(Action action)
	{
		StartCoroutine(ClickProccess(action));
	}

	IEnumerator ClickProccess(Action action)
	{
		animator.gameObject.SetActive(true);
		animator.SetBool("isClicked", true);

		yield return new WaitForSeconds(1); // animation duration

		animator.SetBool("isClicked", false);

		action?.Invoke(); // run AFTER animation
		animator.gameObject.SetActive(false);
	}
}
