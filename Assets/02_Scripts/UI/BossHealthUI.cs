using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 9-Slicing을 쓰지 않는 Filled 체력바에 대해 역경직(HitStop)을 극복하는
/// 위로 튕기기(Jump Hop) + 펌핑(Bump) + 딜레이 홀드(Holding) 시스템을 제공합니다.
/// </summary>
public class BossHealthUI : MonoBehaviour
{
    [Header("UI 연결")]
    [Tooltip("즉각 깎이는 메인 체력바 (빨간색)")]
    [SerializeField] private Image _mainBar;

    [Tooltip("천천히 깎이는 딜레이 체력바 (노란색)")]
    [SerializeField] private Image _delayBar;

    [Header("타격감(Feel) 설정 (범프 & 튕기기)")]
    [Tooltip("피격 시 체력바가 커지는 펌핑 배율 (1.35 = 35% 확대)")]
    [SerializeField] private float _bumpScale = 1.35f;

    [Tooltip("피격 시 위로 튕겨 올라갈 수직 높이 (Pixel)")]
    [SerializeField] private float _bumpHopHeight = 35f;

    [Tooltip("커졌다 원래대로 돌아오는 시간")]
    [SerializeField] private float _bumpDuration = 0.15f;

    [Header("노란색 딜레이 설정 (오래 유지)")]
    [Tooltip("피격 후 노란색 바가 즉시 깎이지 않고 제자리에서 버티는 대기 시간 (초)")]
    [SerializeField] private float _delayHoldDuration = 0.6f;

    [Tooltip("버티는 대기 시간이 지난 후 노란색 바가 줄어드는 속도")]
    [SerializeField] private float _delaySpeed = 0.4f;

    private float _targetFill = 1f;
    private float _delayHoldTimer = 0f;
    private Coroutine _bumpCoroutine;

    private Vector3 _originalScale;
    private Vector3 _originalPosition;

    private void Awake()
    {
        _originalScale = transform.localScale;
        _originalPosition = transform.localPosition;
    }

    public void UpdateHealth(int currentHp, int maxHp)
    {
        _targetFill = Mathf.Clamp01((float)currentHp / maxHp);

        if (_mainBar != null)
        {
            _mainBar.fillAmount = _targetFill;
        }

        _delayHoldTimer = _delayHoldDuration;

        if (_bumpCoroutine != null)
        {
            StopCoroutine(_bumpCoroutine);
        }
        _bumpCoroutine = StartCoroutine(BumpRoutine());
    }

    private void Update()
    {
        if (_delayBar == null)
        {
            return;
        }

        if (_delayHoldTimer > 0f)
        {
            _delayHoldTimer -= Time.unscaledDeltaTime;
            return;
        }

        if (_delayBar.fillAmount > _targetFill)
        {
            _delayBar.fillAmount -= _delaySpeed * Time.unscaledDeltaTime;

            if (_delayBar.fillAmount < _targetFill)
            {
                _delayBar.fillAmount = _targetFill;
            }
        }
    }

    private IEnumerator BumpRoutine()
    {
        float timer = 0f;

        while (timer < _bumpDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / _bumpDuration);

            float bounceCurve = Mathf.Sin(progress * Mathf.PI);

            transform.localPosition = _originalPosition + Vector3.up * (bounceCurve * _bumpHopHeight);
            transform.localScale = _originalScale * Mathf.Lerp(1f, _bumpScale, bounceCurve);

            yield return null;
        }

        transform.localPosition = _originalPosition;
        transform.localScale = _originalScale;
    }

    private void OnDisable()
    {
        transform.localPosition = _originalPosition;
        transform.localScale = _originalScale;
    }
}