using UnityEngine;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;

    private void OnEnable()
    {
        _bgmSlider.SetValueWithoutNotify(AudioManager.Instance.BGMVolume);
        _sfxSlider.SetValueWithoutNotify(AudioManager.Instance.SFXVolume);

        _bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        _sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    private void OnDisable()
    {
        _bgmSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
        _sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);

        PlayerPrefs.SetFloat("BGMVolume", AudioManager.Instance.BGMVolume);
        PlayerPrefs.SetFloat("SFXVolume", AudioManager.Instance.SFXVolume);
        PlayerPrefs.Save();
    }

    private void OnBGMVolumeChanged(float volume)
    {
        AudioManager.Instance.SetBGMVolume(volume);
    }

    private void OnSFXVolumeChanged(float volume)
    {
        AudioManager.Instance.SetSFXVolume(volume);
    }
}