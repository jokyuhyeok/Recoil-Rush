
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingCutscene : MonoBehaviour
{
    [Header("Cutscene Images")]
    [SerializeField] private Image[] _panels;

    [Header("Animation")]
    [SerializeField] private float _appearTime = 0.4f;
    [SerializeField] private float _interval = 1.0f;
    [SerializeField] private float _lastDelay = 1.5f;
    [SerializeField] private float _startScale = 0.85f;

    [Header("Next Scene")]
    [SerializeField] private string _gameSceneName = "GameScene";

    private AsyncOperation _loading;

    private IEnumerator Start()
    {
        Time.timeScale = 1f;

        if (_panels == null || _panels.Length == 0)
        {
            Debug.LogError("Cutscene panels are missing.");
            yield break;
        }

        // Hide all panels first.
        foreach (Image panel in _panels)
        {
            if (panel == null)
            {
                Debug.LogError("A cutscene panel is null.");
                yield break;
            }

            panel.gameObject.SetActive(true);
            panel.color = new Color(1f, 1f, 1f, 0f);
            panel.rectTransform.localScale =
                Vector3.one * _startScale;

            panel.raycastTarget = false;
        }

        // Begin loading the actual game in the background.
        _loading = SceneManager.LoadSceneAsync(_gameSceneName);

        if (_loading == null)
        {
            Debug.LogError("Game scene could not be loaded.");
            yield break;
        }

        _loading.allowSceneActivation = false;

        // Show each panel in order.
        for (int i = 0; i < _panels.Length; i++)
        {
            yield return StartCoroutine(
                ShowPanel(_panels[i])
            );

            if (i < _panels.Length - 1)
                yield return new WaitForSecondsRealtime(_interval);
        }

        // Hold the completed comic.
        yield return new WaitForSecondsRealtime(_lastDelay);

        // Wait until loading is ready.
        while (_loading.progress < 0.9f)
        {
            yield return null;
        }

        // Start the actual game.
        _loading.allowSceneActivation = true;
    }

    private IEnumerator ShowPanel(Image panel)
    {
        float elapsed = 0f;

        while (elapsed < _appearTime)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(_appearTime, 0.01f)
            );

            // Ease-out animation.
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);

            // Fade in.
            panel.color = new Color(
                1f, 1f, 1f, smoothT
            );

            // Scale up.
            float scale = Mathf.Lerp(
                _startScale,
                1f,
                smoothT
            );

            panel.rectTransform.localScale =
                Vector3.one * scale;

            yield return null;
        }

        panel.color = Color.white;
        panel.rectTransform.localScale = Vector3.one;
    }
}