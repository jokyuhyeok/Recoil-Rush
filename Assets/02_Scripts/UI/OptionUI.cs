using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private Slider _mouseSensitivitySlider;

    [SerializeField] private TextMeshProUGUI _bgmText;
    [SerializeField] private TextMeshProUGUI _sfxText;
    [SerializeField] private TextMeshProUGUI _mouseSensitivityText;


    private void OnEnable()
    {
        float bgmVolume = AudioManager.Instance.BGMVolume * 100f;
        float sfxVolume = AudioManager.Instance.SFXVolume * 100f;
        float mouseSensitivity = VirtualMouse.Instance.Sensitivity;

        _bgmSlider.SetValueWithoutNotify(bgmVolume);
        _sfxSlider.SetValueWithoutNotify(sfxVolume);
        _mouseSensitivitySlider.SetValueWithoutNotify(mouseSensitivity);

        _bgmText.text = bgmVolume.ToString();
        _sfxText.text = sfxVolume.ToString();
        _mouseSensitivityText.text = mouseSensitivity.ToString("F2");

        _bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        _sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        _mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
    }

    private void OnDisable()
    {
        _bgmSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
        _sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        _mouseSensitivitySlider.onValueChanged.RemoveListener(OnMouseSensitivityChanged);

        PlayerPrefs.SetFloat("BGMVolume", AudioManager.Instance.BGMVolume * 100f);
        PlayerPrefs.SetFloat("SFXVolume", AudioManager.Instance.SFXVolume * 100f);
        PlayerPrefs.SetFloat("MouseSensitivity", _mouseSensitivitySlider.value);
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

    private void OnMouseSensitivityChanged(float value)
    {
        VirtualMouse.Instance.SetSensitivity(value);
        _mouseSensitivityText.text = value.ToString("F2");
    }
}