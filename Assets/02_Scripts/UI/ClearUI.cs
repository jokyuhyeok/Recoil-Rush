using System.Collections;
using UnityEngine;

public class ClearUI : MonoBehaviour
{
    [SerializeField] private float _startY = 768f;
    [SerializeField] private float _endY = 0f;
    [SerializeField] private float _moveTime = 1.2f;
    [SerializeField] private GameObject _nextClearUI;

    private void Start()
    {
        StartCoroutine(FadeinAnimation());
    }

    private IEnumerator FadeinAnimation()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();

        Vector2 startPosition = new Vector2(rectTransform.anchoredPosition.x, _startY);
        Vector2 endPosition = new Vector2(rectTransform.anchoredPosition.x, _endY);

        rectTransform.anchoredPosition = startPosition;

        float timer = 0f;

        while (timer < _moveTime)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / _moveTime);
            float easedT = Mathf.Sin(t * Mathf.PI * 0.5f);

            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, easedT);

            yield return null;
        }

        rectTransform.anchoredPosition = endPosition;

        _nextClearUI?.SetActive(true);
    }
}