using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

/// <summary>
/// 보스의 시각적, 시간적 타격감(Game Feel)을 전담하는 컴포넌트
/// (역경직, 찌그러짐, 잔상 폭발, 글리치 등 처리)
/// </summary>
public class BossGameFeel : MonoBehaviour
{
    [Header("연출 대상")]
    [Tooltip("보스의 형태를 담당하는 SpriteRenderer의 Transform")]
    [SerializeField] private Transform _bossVisual;

    [Header("타격감 텐션 세팅")]
    [Tooltip("찌그러지고 늘어나는 탄성을 조절하는 커브 (위로 볼록한 포물선 추천)")]
    [SerializeField] private AnimationCurve _squashCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("잔상 릴레이 세팅")]
    [Tooltip("잔상들이 연쇄 폭발할 때의 시간 간격 (초)")]
    [SerializeField] private float _ghostRelayInterval = 0.05f;

    [Header("포스트 프로세싱 (색수차 글리치)")]
    [Tooltip("시작 시 코드가 하이라키에서 자동으로 찾음. 비워놔도 됨.")]
    [SerializeField] private GameObject _glitchVolumeObject;

    private Volume _glitchVolume;
    private Coroutine _glitchCoroutine;

    private Coroutine _hitStopCoroutine;
    private WaitForSecondsRealtime _glitchHoldWait = new WaitForSecondsRealtime(0.05f);

    private void Awake()
    {
        if (_glitchVolumeObject == null)
        {
            _glitchVolumeObject = GameObject.Find("HitGlitchVolume");
        }

        if (_glitchVolumeObject != null)
        {
            _glitchVolume = _glitchVolumeObject.GetComponent<Volume>();
        }
        else
        {
            Debug.LogError("[BossGameFeel] 씬에 'HitGlitchVolume' 오브젝트가 없습니다! 프리팹을 씬에 배치해주세요.");
        }
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }

    public void PlayChargeSquash(float duration)
    {
        if (_bossVisual == null) return;

        Vector3 targetScale = new Vector3(_bossVisual.localScale.x, _bossVisual.localScale.y * 0.8f, 1f);
        StartCoroutine(CurveSquashRoutine(targetScale, duration));
    }

    public void PlayDashStartPop()
    {
        if (_bossVisual == null) return;
        StartCoroutine(DashStartPopRoutine());
    }

    public void PlayHitStop(float duration = 0.05f)
    {
        if (_hitStopCoroutine != null) StopCoroutine(_hitStopCoroutine);
        _hitStopCoroutine = StartCoroutine(HitStopRoutine(duration));
    }

    public void PlayGhostRelayExplosion(System.Collections.Generic.List<BossGhostEffect> ghosts)
    {
        StartCoroutine(GhostRelayRoutine(ghosts));
    }

    public void PlayGlitchEffect(float duration = 0.2f)
    {
        if (_glitchVolume == null) return;

        if (_glitchCoroutine != null) StopCoroutine(_glitchCoroutine);
        _glitchCoroutine = StartCoroutine(GlitchRoutine(duration));
    }

    public void PlayJumpLaunch()
    {
        if (_bossVisual == null) return;
        StartCoroutine(JumpLaunchRoutine());
    }

    public void PlayLandImpact()
    {
        if (_bossVisual == null) return;

        PlayHitStop(0.06f);

        StartCoroutine(LandImpactRoutine());
    }

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
            _bossVisual.localScale = Vector3.LerpUnclamped(targetScale, startScale, t);
            yield return null;
        }

        _bossVisual.localScale = startScale;
    }

    private IEnumerator DashStartPopRoutine()
    {
        Vector3 originalScale = _bossVisual.localScale;
        _bossVisual.localScale = originalScale * 1.3f;

        float elapsed = 0f;
        while (elapsed < 0.1f)
        {
            elapsed += Time.unscaledDeltaTime;
            _bossVisual.localScale = Vector3.Lerp(originalScale * 1.3f, originalScale, elapsed / 0.1f);
            yield return null;
        }

        _bossVisual.localScale = originalScale;
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0.05f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        _hitStopCoroutine = null;
    }

    private IEnumerator GhostRelayRoutine(System.Collections.Generic.List<BossGhostEffect> ghosts)
    {
        int total = ghosts.Count;
        if (total == 0) yield break;

        float part = 1f / (float)total;

        WaitForSeconds relayWait = new WaitForSeconds(_ghostRelayInterval);

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

    private IEnumerator JumpLaunchRoutine()
    {
        Vector3 originalScale = _bossVisual.localScale;
        Vector3 stretchScale = new Vector3(originalScale.x * 0.85f, originalScale.y * 1.25f, 1f);

        _bossVisual.localScale = stretchScale;
        float elapsed = 0f;
        while (elapsed < 0.15f)
        {
            elapsed += Time.deltaTime;
            _bossVisual.localScale = Vector3.Lerp(stretchScale, originalScale, elapsed / 0.15f);
            yield return null;
        }

        _bossVisual.localScale = originalScale;
    }

    private IEnumerator LandImpactRoutine()
    {
        Vector3 originalScale = _bossVisual.localScale;
        Vector3 squashedScale = new Vector3(originalScale.x * 1.3f, originalScale.y * 0.7f, 1f);

        _bossVisual.localScale = squashedScale;
        float elapsed = 0f;
        while (elapsed < 0.12f)
        {
            elapsed += Time.unscaledDeltaTime;
            _bossVisual.localScale = Vector3.Lerp(squashedScale, originalScale, elapsed / 0.12f);
            yield return null;
        }

        _bossVisual.localScale = originalScale;
    }
}