
using System.Collections;
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

    private Vector3[] _originalScales;
    private Color[] _originalColors;

    private void Awake()
    {
        Time.timeScale = 1f;

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

            _originalScales[i] =
                panel.rectTransform.localScale;

            _originalColors[i] = panel.color;

            Color color = panel.color;
            color.a = 0f;
            panel.color = color;

            panel.rectTransform.localScale =
                _originalScales[i] * _startScale;
        }
    }

    private IEnumerator Start()
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
        int nextScene =
            SceneManager.GetActiveScene().buildIndex + 1;

        if (nextScene >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError("다음 게임 씬이 등록되지 않았습니다.");
            yield break;
        }

        AsyncOperation operation =
            SceneManager.LoadSceneAsync(nextScene);

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
                yield return new WaitForSecondsRealtime(
                    _interval
                );
            }
        }

        // 마지막 컷이 등장한 후 잠시 유지
        yield return new WaitForSecondsRealtime(_lastDelay);

        // 게임 씬 로딩 완료 대기
        while (operation.progress < 0.9f)
        {
            yield return null;
        }

        // 게임 시작
        operation.allowSceneActivation = true;
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

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(_appearTime, 0.01f)
            );

            // 부드럽게 확대
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);

            // 페이드인
            Color color = originalColor;
            color.a = originalColor.a * smoothT;
            panel.color = color;

            // 이미지 크기 애니메이션
            float scale = Mathf.Lerp(
                _startScale,
                1f,
                smoothT
            );

            panel.rectTransform.localScale =
                originalScale * scale;

            yield return null;
        }

        panel.color = originalColor;
        panel.rectTransform.localScale = originalScale;
    }
}
