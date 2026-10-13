using UnityEngine;
using System.Collections;

/// <summary>
/// 보스의 돌진 시, 잔상 효과
/// </summary>
public class BossGhostEffect : MonoBehaviour
{
    [SerializeField] private float _fadeSpeed = 3f;
    private SpriteRenderer _sr;
    private bool _isExploding = false;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 잔상 소환 시 초기 상태를 세팅
    /// </summary>
    public void Setup(Sprite currentSprite, Transform bossTransform)
    {
        _sr.sprite = currentSprite;
        transform.position = bossTransform.position;
        transform.localScale = bossTransform.localScale;
        _sr.color = new Color(0.5f, 0.7f, 0.9f, 0.8f);
    }

    private void Update()
    {
        if (_isExploding) return;

        // 매 프레임마다 투명도를 깎아냅니다.
        Color color = _sr.color;
        color.a -= Time.deltaTime * _fadeSpeed;
        _sr.color = color;

        // 완전히 투명해지면 파괴
        if (color.a <= 0)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 일정 시간 대기 후 잔상이 폭발하는 효과를 실행
    /// </summary>
    public void ExplodeWithDelay(float delay, float intensity)
    {
        if (_isExploding) return;
        _isExploding = true;
        StartCoroutine(ExplodeDelayRoutine(delay, intensity));
    }

    private IEnumerator ExplodeDelayRoutine(float delay, float intensity)
    {
        // [메모리 최적화] new WaitForSeconds(delay) 대신 타이머를 사용하여 GC 쓰레기를 0으로 만듬 (Zero-Allocation)
        float delayTimer = 0f;
        while (delayTimer < delay)
        {
            delayTimer += Time.deltaTime;
            yield return null;
        }

        // 시간이 되면 번쩍! 팽창하며 폭발!
        _sr.color = new Color(1f, 1f, 1f, intensity);
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * (1f + (0.5f * intensity));

        float duration = 0.15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);

            Color c = _sr.color;
            c.a = Mathf.Lerp(intensity, 0f, elapsed / duration);
            _sr.color = c;

            yield return null;
        }

        Destroy(gameObject);
    }
}