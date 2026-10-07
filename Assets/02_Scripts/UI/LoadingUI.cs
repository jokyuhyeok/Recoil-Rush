using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    [SerializeField] private Image _loadingFillImage;
    [SerializeField] private TMP_Text _loadingText;

    [SerializeField] private float _loadingBarSpeed = 1f;

    private Coroutine _loadCoroutine;

    private void OnEnable()
    {
        _loadCoroutine = StartCoroutine(LoadScene());
    }

    private void OnDisable()
    {
        if (_loadCoroutine != null)
        {
            StopCoroutine(_loadCoroutine);
            _loadCoroutine = null;
        }
    }

    private IEnumerator LoadScene()
    {
        _loadingFillImage.fillAmount = 0f;
        _loadingText.text = "Loading.. 0%";

        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        operation.allowSceneActivation = false;

        float loadingProgress = 0f;

        while (operation.progress < 0.9f || loadingProgress < 0.95f)
        {
            float operationProgress = Mathf.Clamp01(operation.progress / 0.9f);

            loadingProgress = Mathf.Lerp(loadingProgress, operationProgress, _loadingBarSpeed * Time.deltaTime);

            if (Mathf.Abs(loadingProgress - operationProgress) < 0.001f)
            {
                loadingProgress = operationProgress;
            }

            _loadingFillImage.fillAmount = loadingProgress;
            _loadingText.text = $"Loading.. {loadingProgress * 100f:0}%";

            yield return null;
        }

        _loadingFillImage.fillAmount = 1f;
        _loadingText.text = "Loading.. 100%";

        yield return null;

        operation.allowSceneActivation = true;
    }
}