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
	[SerializeField] private Button _toregisterBtn;

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

	void Start()
    {
        ButtonFunctionsSetUp();
    }

    void Update()
    {
        
    }

    void ButtonFunctionsSetUp()
    {
		_newGameBtn.onClick.AddListener(() =>
		{
			Debug.Log("New Game clicked");

			APIManager.Instance.Logout();

			_homePage.SetActive(false);
			_registerPage.SetActive(true);
			_loginPage.SetActive(false);
		});


		_contineBtn.onClick.AddListener(() =>
		{
			Debug.Log("Continue clicked");

			StartCoroutine(APIManager.Instance.GetProfile((success, profile) =>
			{
				if (success)
				{
					Debug.Log("Token valid → Continue game");

					SceneManager.LoadScene("GameScene");
				}
				else
				{
					Debug.Log("Invalid token → go to login");

					_homePage.SetActive(false);
					_loginPage.SetActive(true);
				}
			}));
		});

		_exitBtn.onClick.AddListener(() =>
		{
			Application.Quit();
		});

		_registerBtn.onClick.AddListener(() =>
		{
			string username = _registerUsername.text;
			string email = _registerEmail.text;
			string password = _registerPassword.text;

			StartCoroutine(APIManager.Instance.Register(username, email, password, (success, result) =>
			{
				if (success)
				{
					Debug.Log("Registered successfully");

					_registerPage.SetActive(false);
					_loginPage.SetActive(true);
				}
				else
				{
					Debug.LogError("Register failed: " + result);
				}
			}));
		});

		_loginBtn.onClick.AddListener(() =>
		{
			string username = _loginUsername.text;
			string password = _loginPassword.text;

			StartCoroutine(APIManager.Instance.Login(username, password, (success, result) =>
			{
				if (success)
				{
					Debug.Log("Login success");

					StartCoroutine(APIManager.Instance.GetProfile((profileSuccess, profile) =>
					{
						if (profileSuccess)
						{
							Debug.Log("Profile loaded");

							if (profile.currency == null)
							{
								StartCoroutine(APIManager.Instance.InitPlayer(initSuccess =>
								{
									Debug.Log("Player initialized");

									SceneManager.LoadScene("GameScene");
								}));
							}
							else
							{
								SceneManager.LoadScene("GameScene");
							}
						}
					}));
				}
				else
				{
					Debug.LogError("Login failed: " + result);
				}
			}));
		});

		_backBtn.onClick.AddListener(() =>
        {
            _homePage.SetActive(true);
            _registerPage.SetActive(false);
            _loginPage.SetActive(false);
        });

        _back1Btn.onClick.AddListener(() =>
        {
            _homePage.SetActive(true);
            _registerPage.SetActive(false);
            _loginPage.SetActive(false);
        });

        _toregisterBtn.onClick.AddListener(() =>
        {
            _homePage.SetActive(false);
            _registerPage.SetActive(true);
            _loginPage.SetActive(false);
        });
    }
}
