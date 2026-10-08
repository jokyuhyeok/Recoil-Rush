using UnityEngine;
using UnityEngine.UI;

public class StartUI : MonoBehaviour
{
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _optionButton;
    [SerializeField] private Button _exitButton;

    [SerializeField] private GameObject _loadingUI;
    [SerializeField] private GameObject _optionUI;


    private void Awake()
    {
        _startButton?.onClick.AddListener(OnStartButtonClicked);
        _optionButton?.onClick.AddListener(OnOptionButtonClicked);
        _exitButton?.onClick.AddListener(OnExitButtonClicked);
    }

    private void OnStartButtonClicked()
    {
        _loadingUI.SetActive(true);
        gameObject.SetActive(false);
    }

    private void OnOptionButtonClicked()
    {
        _optionUI.SetActive(true);
    }

    private void OnExitButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}