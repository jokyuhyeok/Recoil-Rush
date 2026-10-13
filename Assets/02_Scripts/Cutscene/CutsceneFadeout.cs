using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CutsceneFadeout : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private float _fadeTime = 2f;

    [SerializeField] private GameObject _clearUI;
    [SerializeField] private float _uiInterval = 0.5f;


    private void Start()
    {
        _image = GetComponent<Image>();

        StartCoroutine(FadeoutAnimation());
    }

    private IEnumerator FadeoutAnimation()
    {
        float timer = 0f;

        Color color = _image.color;
        color.a = 0f;
        _image.color = color;

        while (timer < _fadeTime)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / _fadeTime);

            color.a = t;
            _image.color = color;

            yield return null;
        }

        color.a = 1f;
        _image.color = color;

        yield return new WaitForSeconds(_uiInterval);
        _clearUI.SetActive(true);
    }
}