using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    private static FadeManager _instance;
    public static FadeManager Instance => _instance;

    private Image _image;
    private Coroutine _fadeCoroutine;


    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        _image = GetComponent<Image>();
    }

    public void FadeIn(float fadeTime)
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        _fadeCoroutine = StartCoroutine(FadeInAnimation(fadeTime));
    }

    public void FadeOut(float fadeTime)
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        _fadeCoroutine = StartCoroutine(FadeOutAnimation(fadeTime));
    }

    private IEnumerator FadeInAnimation(float fadeTime)
    {
        float timer = 0f;

        Color color = _image.color;
        float startAlpha = color.a;

        while (timer < fadeTime)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / fadeTime);

            color.a = Mathf.Lerp(startAlpha, 0f, t);
            _image.color = color;

            yield return null;
        }

        color.a = 0f;
        _image.color = color;

        _fadeCoroutine = null;
    }

    private IEnumerator FadeOutAnimation(float fadeTime)
    {
        float timer = 0f;

        Color color = _image.color;
        float startAlpha = color.a;

        while (timer < fadeTime)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / fadeTime);

            color.a = Mathf.Lerp(startAlpha, 1f, t);
            _image.color = color;

            yield return null;
        }

        color.a = 1f;
        _image.color = color;

        _fadeCoroutine = null;
    }
}