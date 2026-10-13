using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class BossGameFeel : MonoBehaviour
{
    [Header("연출 대상")]
    [SerializeField] private Transform _bossVisual;

    [Header("타격감 텐션 세팅")]
    [SerializeField] private AnimationCurve _squashCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("잔상 릴레이 세팅")]
    [SerializeField] private float _ghostRelayInterval = 0.05f;

    [Header("포스트 프로세싱")]
    [SerializeField] private GameObject _glitchVolumeObject;

    private Volume _glitchVolume;
    private Coroutine _glitchCoroutine;
    private Coroutine _hitStopCoroutine;
    private WaitForSecondsRealtime _glitchHoldWait = new WaitForSecondsRealtime(0.05f);

    // 현재 진행 중인 스케일 코루틴을 기억해서 팀킬(충돌)을 막습니다.
    private Coroutine _scaleCoroutine;

    // 보스의 원래 크기(원본)를 기억해둡니다.
    private Vector3 _baseScale;

    private void Awake()
    {
        if (_glitchVolumeObject == null) _glitchVolumeObject = GameObject.Find("HitGlitchVolume");
        if (_glitchVolumeObject != null) _glitchVolume = _glitchVolumeObject.GetComponent<Volume>();
    }

    private void Start()
    {
        if (_bossVisual != null) _baseScale = _bossVisual.localScale;
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        if (_bossVisual != null) _bossVisual.localScale = _baseScale; // 꺼질 때 무조건 원래 크기로 복구
    }

    // ==========================================
    // 스케일 교통정리 (팀킬 방지) 헬퍼 메서드
    // ==========================================
    private void PlayScaleRoutine(IEnumerator routine)
    {
        if (_bossVisual == null) return;

        // 기존에 찌그러지거나 커지고 있는 코루틴이 있다면 즉시 멱살을 잡고 멈춥니다!
        if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);

        _scaleCoroutine = StartCoroutine(routine);
    }

    // ==========================================
    // 외부 호출 함수들
    // ==========================================
    public void PlayChargeSquash(float duration)
    {
        if (_bossVisual == null) return;

        Vector3 targetScale = new Vector3(_baseScale.x * 1.2f, _baseScale.y * 0.8f, 1f);
        PlayScaleRoutine(CurveSquashRoutine(targetScale, duration));
    }

    public void PlayDashStartPop()
    {
        PlayScaleRoutine(DashStartPopRoutine());
    }

    public void PlayJumpLaunch()
    {
        PlayScaleRoutine(JumpLaunchRoutine());
    }

    public void PlayLandImpact()
    {
        PlayHitStop(0.06f);
        PlayScaleRoutine(LandImpactRoutine());
    }

    public void PlayHitStop(float duration = 0.05f)
    {
        if (_hitStopCoroutine != null) StopCoroutine(_hitStopCoroutine);
        _hitStopCoroutine = StartCoroutine(HitStopRoutine(duration));
    }

    public void PlayGhostRelayExplosion(List<BossGhostEffect> ghosts)
    {
        StartCoroutine(GhostRelayRoutine(ghosts));
    }

    public void PlayGlitchEffect(float duration = 0.2f)
    {
        if (_glitchVolume == null) return;
        if (_glitchCoroutine != null) StopCoroutine(_glitchCoroutine);
        _glitchCoroutine = StartCoroutine(GlitchRoutine(duration));
    }

    // ==========================================
    // 코루틴 내부
    // ==========================================
    private IEnumerator CurveSquashRoutine(Vector3 targetScale, float duration)
    {
        Vector3 startScale = _bossVisual.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = _squashCurve.Evaluate(elapsed / duration);
            _bossVisual.localScale = Vector3.LerpUnclamped(startScale, targetScale, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration * 0.3f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = _squashCurve.Evaluate(elapsed / (duration * 0.3f));
            _bossVisual.localScale = Vector3.LerpUnclamped(targetScale, _baseScale, t);
            yield return null;
        }

        _bossVisual.localScale = _baseScale;
    }

    private IEnumerator DashStartPopRoutine()
    {
        Vector3 popScale = _baseScale * 1.3f;
        float elapsed = 0f;

        while (elapsed < 0.1f)
        {
            elapsed += Time.unscaledDeltaTime;
            _bossVisual.localScale = Vector3.Lerp(popScale, _baseScale, elapsed / 0.1f);
            yield return null;
        }

        _bossVisual.localScale = _baseScale;
    }

    private IEnumerator JumpLaunchRoutine()
    {
        Vector3 stretchScale = new Vector3(_baseScale.x * 0.85f, _baseScale.y * 1.25f, 1f);
        float elapsed = 0f;

        while (elapsed < 0.15f)
        {
            elapsed += Time.unscaledDeltaTime;
            _bossVisual.localScale = Vector3.Lerp(stretchScale, _baseScale, elapsed / 0.15f);
            yield return null;
        }

        _bossVisual.localScale = _baseScale;
    }

    private IEnumerator LandImpactRoutine()
    {
        Vector3 squashedScale = new Vector3(_baseScale.x * 1.3f, _baseScale.y * 0.7f, 1f);
        float elapsed = 0f;

        while (elapsed < 0.12f)
        {
            elapsed += Time.unscaledDeltaTime;
            _bossVisual.localScale = Vector3.Lerp(squashedScale, _baseScale, elapsed / 0.12f);
            yield return null;
        }

        _bossVisual.localScale = _baseScale;
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0.05f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        _hitStopCoroutine = null;
    }

    private IEnumerator GhostRelayRoutine(List<BossGhostEffect> ghosts)
    {
        int total = ghosts.Count;
        if (total == 0) yield break;

        float part = 1f / (float)total;
        WaitForSecondsRealtime relayWait = new WaitForSecondsRealtime(_ghostRelayInterval);

        for (int i = total - 1; i >= 0; i--)
        {
            if (ghosts[i] != null)
            {
                float intensity = (i + 1) * part;
            }

            yield return relayWait;
        }
    }

    private IEnumerator GlitchRoutine(float duration)
    {
        if (_glitchVolume == null) yield break;

        _glitchVolume.weight = 1f;
        yield return _glitchHoldWait;

        float elapsed = 0f;
        float fadeTime = duration - 0.05f;
        if (fadeTime <= 0f) fadeTime = 0.1f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.unscaledDeltaTime;
            _glitchVolume.weight = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            yield return null;
        }

        _glitchVolume.weight = 0.001f;
    }
}