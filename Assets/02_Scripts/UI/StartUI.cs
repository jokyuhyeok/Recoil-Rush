using UnityEngine;
using UnityEngine.UI;

public class StartUI : MonoBehaviour
{
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _optionButton;
    [SerializeField] private Button _exitButton;

    [SerializeField] private GameObject _loadingUI;

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
        // Todo: 시간 남으면 옵션 구현
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