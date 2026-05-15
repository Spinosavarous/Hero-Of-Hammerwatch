using UnityEngine;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance;

	[Header("Audio Sources")]
	[SerializeField] private AudioSource musicSource;
	[SerializeField] private AudioSource sfxSource;

	[Header("Music")]
	[SerializeField] private AudioClip backgroundMusic;

	[Header("SFX")]
	[SerializeField] private AudioClip walkClip;
	[SerializeField] private AudioClip attackClip;
	[SerializeField] private AudioClip bloodClip;

	private const string MusicKey = "MusicVolume";
	private const string SFXKey = "SFXVolume";

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void Start()
	{
		LoadVolumes();
		PlayMusic();
	}

	// -----------------------------
	// MUSIC
	// -----------------------------
	public void PlayMusic()
	{
		musicSource.clip = backgroundMusic;
		musicSource.loop = true;
		musicSource.Play();
	}

	public void SetMusicVolume(float volume)
	{
		musicSource.volume = volume;

		PlayerPrefs.SetFloat(MusicKey, volume);
		PlayerPrefs.Save();
	}

	// -----------------------------
	// SFX
	// -----------------------------
	public void SetSFXVolume(float volume)
	{
		sfxSource.volume = volume;

		PlayerPrefs.SetFloat(SFXKey, volume);
		PlayerPrefs.Save();
	}

	public void StartWalking()
	{
		if (sfxSource.clip == walkClip && sfxSource.isPlaying)
			return;

		sfxSource.clip = walkClip;
		sfxSource.loop = true;
		sfxSource.Play();
	}

	public void StopWalking()
	{
		if (sfxSource.clip == walkClip)
		{
			sfxSource.Stop();
			sfxSource.loop = false;
			sfxSource.clip = null;
		}
	}

	public void PlayAttack()
	{
		sfxSource.PlayOneShot(attackClip);
	}

	public void PlayBlood()
	{
		sfxSource.PlayOneShot(bloodClip);
	}

	// -----------------------------
	// LOAD
	// -----------------------------
	private void LoadVolumes()
	{
		float musicVolume = PlayerPrefs.GetFloat(MusicKey, 1f);
		float sfxVolume = PlayerPrefs.GetFloat(SFXKey, 1f);

		musicSource.volume = musicVolume;
		sfxSource.volume = sfxVolume;
	}
}