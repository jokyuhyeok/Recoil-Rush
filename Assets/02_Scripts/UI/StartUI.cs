
using UnityEngine;
using UnityEngine.UI;

public enum MenuAction
{
    Start,
    Option,
    Exit
}

public class StartUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _optionButton;
    [SerializeField] private Button _exitButton;

    [Header("UI")]
    [SerializeField] private GameObject _loadingUI;
    [SerializeField] private GameObject _optionUI;

    [Header("Shooting")]
    [SerializeField] private Collider2D[] _menuHitboxes;

    private bool _isLoading;
    private bool _isOptionOpen;

    public bool IsOptionOpen => _isOptionOpen;
    private void Awake()
    {
        // 메인 메뉴 버튼은 마우스로 클릭 불가능
        DisableMouseClick(_startButton);
        DisableMouseClick(_optionButton);
        DisableMouseClick(_exitButton);

        _startButton.onClick.AddListener(OnStartButtonClicked);
        _optionButton.onClick.AddListener(OnOptionButtonClicked);
        _exitButton.onClick.AddListener(OnExitButtonClicked);
    }

    private void DisableMouseClick(Button button)
    {
        if (button == null)
            return;

        CanvasGroup group = button.GetComponent<CanvasGroup>();

        if (group == null)
        {
            group = button.gameObject.AddComponent<CanvasGroup>();
        }

        // 마우스 Raycast 차단
        group.blocksRaycasts = false;
    }

    private void OnDestroy()
    {
        _startButton.onClick.RemoveListener(OnStartButtonClicked);
        _optionButton.onClick.RemoveListener(OnOptionButtonClicked);
        _exitButton.onClick.RemoveListener(OnExitButtonClicked);
    }

    // 총알이 메뉴에 명중했을 때 호출
    public void OnMenuHit(MenuAction action)
    {
        if (_isLoading || _isOptionOpen)
            return;

        switch (action)
        {
            case MenuAction.Start:
                _startButton.onClick.Invoke();
                break;

            case MenuAction.Option:
                _optionButton.onClick.Invoke();
                break;

            case MenuAction.Exit:
                _exitButton.onClick.Invoke();
                break;
        }
    }

    private void OnStartButtonClicked()
    {
        if (_isLoading || _isOptionOpen)
            return;

        _isLoading = true;

        _loadingUI.SetActive(true);
        gameObject.SetActive(false);
    }

    private void OnOptionButtonClicked()
    {
        if (_isLoading || _isOptionOpen)
            return;

        _isOptionOpen = true;

        // 메뉴 충돌 판정 비활성화
        SetMenuShooting(false);

        // 옵션창 활성화
        _optionUI.SetActive(true);
    }


    public void CloseOptionUI()
    {
        if (!_isOptionOpen)
            return;

        _isOptionOpen = false;

        // 메뉴 총격 판정 다시 활성화
        SetMenuShooting(true);

        // 옵션창 닫기
        _optionUI.SetActive(false);

        Debug.Log("옵션 종료 - 총 발사 가능");
    }


    private void SetMenuShooting(bool enabled)
    {
        if (_menuHitboxes == null)
            return;

        foreach (Collider2D hitbox in _menuHitboxes)
        {
            if (hitbox != null)
                hitbox.enabled = enabled;
        }
    }

    private void OnExitButtonClicked()
    {
        if (_isLoading || _isOptionOpen)
            return;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
