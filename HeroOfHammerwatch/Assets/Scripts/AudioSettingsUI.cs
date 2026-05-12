using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
	[Header("UI")]
	[SerializeField] private Slider musicSlider;
	[SerializeField] private Slider sfxSlider;

	private const string MusicKey = "MusicVolume";
	private const string SfxKey = "SFXVolume";

	private void Start()
	{
		LoadSettings();

		musicSlider.onValueChanged.AddListener(SetMusicVolume);
		sfxSlider.onValueChanged.AddListener(SetSFXVolume);
	}

	// -----------------------------
	// LOAD
	// -----------------------------
	private void LoadSettings()
	{
		float musicVolume = PlayerPrefs.GetFloat(MusicKey, 1f);
		float sfxVolume = PlayerPrefs.GetFloat(SfxKey, 1f);

		musicSlider.value = musicVolume;
		sfxSlider.value = sfxVolume;

		ApplyMusicVolume(musicVolume);
		ApplySFXVolume(sfxVolume);
	}

	// -----------------------------
	// MUSIC
	// -----------------------------
	private void SetMusicVolume(float value)
	{
		ApplyMusicVolume(value);

		PlayerPrefs.SetFloat(MusicKey, value);
		PlayerPrefs.Save();
	}

	private void ApplyMusicVolume(float value)
	{
		// Example:
		// AudioManager.Instance.SetMusicVolume(value);

		AudioListener.volume = value;
	}

	// -----------------------------
	// SFX
	// -----------------------------
	private void SetSFXVolume(float value)
	{
		ApplySFXVolume(value);

		PlayerPrefs.SetFloat(SfxKey, value);
		PlayerPrefs.Save();
	}

	private void ApplySFXVolume(float value)
	{
		// Example:
		// AudioManager.Instance.SetSFXVolume(value);

		Debug.Log("SFX Volume: " + value);
	}
}