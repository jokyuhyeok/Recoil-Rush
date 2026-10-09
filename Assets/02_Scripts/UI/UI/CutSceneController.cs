using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CutsceneController : MonoBehaviour
{
    [Header("Cutscene Images")]
    [SerializeField] private Image[] _panels;

    [Header("Animation Settings")]
    [SerializeField] private float _appearTime = 0.5f;
    [SerializeField] private float _interval = 1f;
    [SerializeField] private float _lastDelay = 2f;
    [SerializeField] private float _startScale = 0.85f;

    [Header("Continue UI")]
    [SerializeField] private TMP_Text _continueText;

    private Vector3[] _originalScales;
    private Color[] _originalColors;

    private Coroutine _cutsceneCoroutine;

    private void Awake()
    {
        Time.timeScale = 1f;

        if (_continueText != null)
            _continueText.gameObject.SetActive(false);

        if (_panels == null || _panels.Length != 6)
        {
            Debug.LogError("컷씬 Image 6개를 연결해주세요.");
            return;
        }

        _originalScales = new Vector3[6];
        _originalColors = new Color[6];

        // 처음에는 모든 이미지 투명하게 설정
        for (int i = 0; i < 6; i++)
        {
            Image panel = _panels[i];

            if (panel == null)
                continue;

            panel.gameObject.SetActive(true);
            panel.raycastTarget = false;

            _originalScales[i] = panel.rectTransform.localScale;
            _originalColors[i] = panel.color;

            Color color = panel.color;
            color.a = 0f;
            panel.color = color;

            panel.rectTransform.localScale = _originalScales[i] * _startScale;
        }
    }

    private void OnEnable()
    {
        _cutsceneCoroutine = StartCoroutine(PlayCutscene());
    }

    private void OnDisable()
    {
        if (_cutsceneCoroutine != null)
        {
            StopCoroutine(_cutsceneCoroutine);
            _cutsceneCoroutine = null;
        }
    }

    private IEnumerator PlayCutscene()
    {
        if (_panels == null || _panels.Length != 6)
            yield break;

        for (int i = 0; i < 6; i++)
        {
            if (_panels[i] == null)
            {
                Debug.LogError($"Panel0{i + 1} 연결 안 됨!");
                yield break;
            }
        }

        // 실제 게임 씬을 백그라운드에서 로딩
        AsyncOperation operation = SceneManager.LoadSceneAsync(
            SceneManager.GetActiveScene().buildIndex + 1);

        if (operation == null)
        {
            Debug.LogError("게임 씬 로딩 실패");
            yield break;
        }

        operation.allowSceneActivation = false;

        // 6컷 순서대로 등장
        for (int i = 0; i < 6; i++)
        {
            yield return StartCoroutine(ShowPanel(i));

            if (i < 5)
            {
                yield return new WaitForSecondsRealtime(_interval);
            }
        }

        // 마지막 컷이 등장한 후 잠시 유지
        yield return new WaitForSecondsRealtime(_lastDelay);

        // 게임 씬 로딩 완료 대기
        while (operation.progress < 0.9f)
        {
            yield return null;
        }

        // 클릭 안내 TMP 표시
        _continueText?.gameObject.SetActive(true);

        // TMP가 표시된 프레임의 클릭 입력은 무시
        yield return null;

        // 마우스 왼쪽 버튼 클릭 대기
        while (!Input.GetMouseButtonDown(0))
        {
            yield return null;
        }

        // 게임 시작
        operation.allowSceneActivation = true;
        _cutsceneCoroutine = null;
    }

    private IEnumerator ShowPanel(int index)
    {
        Image panel = _panels[index];

        Vector3 originalScale = _originalScales[index];
        Color originalColor = _originalColors[index];

        float elapsed = 0f;

        while (elapsed < _appearTime)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / Mathf.Max(_appearTime, 0.01f));

            // 부드럽게 확대
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);

            // 페이드인
            Color color = originalColor;
            color.a = originalColor.a * smoothT;
            panel.color = color;

            // 이미지 크기 애니메이션
            float scale = Mathf.Lerp(_startScale, 1f, smoothT);
            panel.rectTransform.localScale = originalScale * scale;

            yield return null;
        }

        panel.color = originalColor;
        panel.rectTransform.localScale = originalScale;
    }
}