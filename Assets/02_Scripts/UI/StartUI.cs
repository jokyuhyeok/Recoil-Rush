using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartUI : MonoBehaviour
{
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _optionButton;
    [SerializeField] private Button _exitButton;


    private void Awake()
    {
        _startButton?.onClick.AddListener(OnStartButtonClicked);
        _optionButton?.onClick.AddListener(OnOptionButtonClicked);
        _exitButton?.onClick.AddListener(OnExitButtonClicked);
    }

    private void OnStartButtonClicked()
    {
        gameObject.SetActive(false);
    }

    private void OnOptionButtonClicked()
    {
        // Todo: 시간 남으면 옵션 구현
    }

    private void OnExitButtonClicked()
    {
        // 저장할 게 없어서 바로 끝내면 됨 (옵션 생기면 저장해야 함..)
        Application.Quit();
    }
}