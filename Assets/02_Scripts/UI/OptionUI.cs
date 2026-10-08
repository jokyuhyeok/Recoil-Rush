using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;

    [SerializeField] private TextMeshProUGUI _bgmText;
    [SerializeField] private TextMeshProUGUI _sfxText;


    private void OnEnable()
    {
        float bgmVolume = AudioManager.Instance.BGMVolume * 100f;
        float sfxVolume = AudioManager.Instance.SFXVolume * 100f;

        _bgmSlider.SetValueWithoutNotify(bgmVolume);
        _sfxSlider.SetValueWithoutNotify(sfxVolume);

        _bgmText.text = bgmVolume.ToString();
        _sfxText.text = sfxVolume.ToString();

        _bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        _sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    private void OnDisable()
    {
        _bgmSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
        _sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);

        PlayerPrefs.SetFloat("BGMVolume", AudioManager.Instance.BGMVolume * 100f);
        PlayerPrefs.SetFloat("SFXVolume", AudioManager.Instance.SFXVolume * 100f);
        PlayerPrefs.Save();
    }

    private void OnBGMVolumeChanged(float volume)
    {
        AudioManager.Instance.SetBGMVolume(volume / 100f);
        _bgmText.text = volume.ToString();
    }

    private void OnSFXVolumeChanged(float volume)
    {
        AudioManager.Instance.SetSFXVolume(volume / 100f);
        _sfxText.text = volume.ToString();
    }
}