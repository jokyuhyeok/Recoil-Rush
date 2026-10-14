using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

/// <summary>
/// 보스의 모든 시각적 연출(VFX, 머티리얼 깜빡임, 카메라 흔들림)을 전담하는 컨트롤러입니다.
/// 단일 책임 원칙(SRP)에 따라 로직과 연출을 분리하기 위해 생성되었습니다.
/// </summary>
public class BossVFX : MonoBehaviour
{
    [Header("시각 효과 (VFX)")]
    [SerializeField] private Transform _vfxGroup;
    public GameObject IdleSmokeEffect;
    public GameObject DashChargeEffect;
    public GameObject DashFireEffect;
    public GameObject HitWallEffectPrefab;
    public GameObject HandSwirlEffect;
    public GameObject RedSphereEffect;

    [Header("스프라이트 및 머티리얼")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Material _flashMaterial;
    private Material _originalMaterial;

    [Header("카메라 흔들림 발생기")]
    [SerializeField] private CinemachineImpulseSource _lightShakeSource; // 피격, 약한 타격 (Dissipating 적용)
    [SerializeField] private CinemachineImpulseSource _heavyShakeSource; // 쿵! 찍기, 충돌 (강하고 짧음)
    [SerializeField] private CinemachineImpulseSource _earthquakeShakeSource; // 페이즈 전환, 사망 (Rumble, 길고 무거움)

    [Header("카메라 흔들림 강도")]
    [SerializeField] private float _hitShakeForce = 0.5f; // 피격 시
    [SerializeField] private float _wallSlamForce = 2.0f; // 벽 충돌, 땅 찍기 시
    [SerializeField] private float _phaseChangeForce = 1.5f; // 페이즈 전환 굉음
    [SerializeField] private float _deathEarthquakeForce = 3.5f; // 사망 연출 시

    private Coroutine _flashCoroutine;
    private WaitForSeconds _hitFlashDelay = new WaitForSeconds(0.05f);
    private BossGameFeel _gameFeel;

    /// <summary>
    /// 초기화 단계. 필요한 컴포넌트와 원래 머티리얼을 캐싱합니다.
    /// </summary>
    private void Awake()
    {
        _gameFeel = GetComponent<BossGameFeel>();
        if (_spriteRenderer != null)
        {
            _originalMaterial = _spriteRenderer.material;
        }
    }

    // =========================================================
    // 시네머신 흔들림 헬퍼 함수들
    // =========================================================

    /// <summary>
    /// 가벼운 카메라 진동(피격, 기관총 연사 등)을 발생시킵니다.
    /// </summary>
    public void PlayShakeLight(float forceOverride = -1f)
    {
        if (_lightShakeSource != null)
        {
            float force = forceOverride < 0 ? _hitShakeForce : forceOverride;
            Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;
            _lightShakeSource.GenerateImpulse(randomDir * force);
        }
    }

    /// <summary>
    /// 강하고 묵직한 카메라 진동(착지, 돌진 충돌 등)을 발생시킵니다.
    /// </summary>
    public void PlayShakeHeavy(float forceOverride = -1f)
    {
        if (_heavyShakeSource != null)
        {
            float force = forceOverride < 0 ? _wallSlamForce : forceOverride;
            Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;
            _heavyShakeSource.GenerateImpulse(randomDir * force);
        }
    }

    /// <summary>
    /// 길고 지속적인 화면 지진(페이즈 전환, 사망 폭주 등)을 발생시킵니다.
    /// </summary>
    public void PlayShakeEarthquake(float forceOverride = -1f)
    {
        if (_earthquakeShakeSource != null)
        {
            float force = forceOverride < 0 ? _phaseChangeForce : forceOverride;
            Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;
            _earthquakeShakeSource.GenerateImpulse(randomDir * force);
        }
    }

    // =========================================================
    // 시각 연출 (깜빡임, 페이즈 전환, 사망)
    // =========================================================

    /// <summary>
    /// 피격 시 스프라이트가 하얗게 번쩍이는 효과와 가벼운 진동을 실행합니다.
    /// </summary>
    public void PlayHitFlash()
    {
        if (_spriteRenderer == null || _flashMaterial == null) return;

        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(HitFlashRoutine());
        PlayShakeLight(); // 피격 시 가벼운 진동
    }

    /// <summary>
    /// 짧은 시간 동안 하얀색 머티리얼로 변경했다가 원래대로 복구하는 내부 코루틴입니다.
    /// </summary>
    private IEnumerator HitFlashRoutine()
    {
        _spriteRenderer.material = _flashMaterial;
        yield return _hitFlashDelay;
        _spriteRenderer.material = _originalMaterial;
    }

    /// <summary>
    /// 상태 강제 초기화 시 스프라이트 머티리얼을 강제로 원상복구 합니다.
    /// </summary>
    public void ResetFlashMaterial()
    {
        if (_spriteRenderer != null && _originalMaterial != null)
        {
            _spriteRenderer.material = _originalMaterial;
        }
    }

    /// <summary>
    /// 현재 활성화되어 있는 모든 파티클 및 시각 효과(VFX)를 끕니다.
    /// </summary>
    public void DisableAllVFX()
    {
        if (_vfxGroup != null)
        {
            foreach (Transform child in _vfxGroup)
            {
                child.gameObject.SetActive(false);
            }
        }
        if (RedSphereEffect != null) RedSphereEffect.SetActive(false);
        if (HandSwirlEffect != null) HandSwirlEffect.SetActive(false);
    }

    /// <summary>
    /// 페이즈 전환 시 발생하는 붉은색 점멸과 글리치, 지진 연출을 제어하는 코루틴입니다.
    /// </summary>
    public IEnumerator PlayPhaseTransitionVFX(float duration)
    {
        // 1. 글리치 효과 및 화면 둥둥 흔들림 시작
        if (_gameFeel != null) _gameFeel.PlayGlitchEffect(duration);
        PlayShakeEarthquake(_phaseChangeForce * 0.5f);

        Vector3 originalScale = transform.localScale;
        float elapsed = 0f;
        float blinkInterval = 0.3f;
        bool isRed = false;

        while (elapsed < duration)
        {
            // 페이즈 전환 중 빨간색 점멸
            _spriteRenderer.color = isRed ? Color.red : Color.white;
            isRed = !isRed;

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
            blinkInterval = Mathf.Max(0.05f, blinkInterval - 0.04f);
        }

        // 연출 종료 후 원상복구
        _spriteRenderer.color = Color.white;
        transform.localScale = originalScale;
        PlayShakeHeavy(_wallSlamForce * 1.5f); // 페이즈 전환 직후 쾅!
    }

    /// <summary>
    /// 보스 체력이 0이 되어 사망할 때 발생하는 보라색 폭주 및 찌그러짐 연출 코루틴입니다.
    /// </summary>
    public IEnumerator PlayDeathVFX(float duration)
    {
        if (_gameFeel != null) _gameFeel.PlayGlitchEffect(duration);
        PlayShakeEarthquake(_deathEarthquakeForce); // 보스 사망 시 거대한 연쇄 지진 발생

        Color purpleColor = new Color(0.65f, 0f, 1f);
        Vector3 originalScale = transform.localScale;
        float elapsed = 0f;
        float blinkTimer = 0f;
        bool isColored = false;

        // 프레임 단위(yield return null)로 쪼개서 늘줄늘줄은 60프레임으로 부드럽게, 깜빡임은 타이머로 제어
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            blinkTimer += Time.deltaTime;

            // 1. 색상 점멸: 시간이 지날수록 점멸 간격이 '서서히' 짧아짐
            float currentBlinkInterval = Mathf.Lerp(0.5f, 0.03f, elapsed / duration);
            if (blinkTimer >= currentBlinkInterval)
            {
                blinkTimer = 0f;
                _spriteRenderer.color = isColored ? purpleColor : Color.white;
                isColored = !isColored;
            }

            // 2. 늘줄늘줄 요동: 시간이 지날수록 진동폭과 진동 속도가 미친듯이 증가
            float currentStretchMax = Mathf.Lerp(0.05f, 0.15f, elapsed / duration);
            float speedMultiplier = Mathf.Lerp(15.0f, 45.0f, elapsed / duration);
            float stretchAmount = Mathf.Sin(elapsed * speedMultiplier) * currentStretchMax;

            transform.localScale = new Vector3(
                originalScale.x * (1f + stretchAmount),
                originalScale.y * (1f - stretchAmount),
                originalScale.z
            );

            yield return null; // 매 프레임마다 크기를 갱신하여 찰지고 부드러운 고무공 진동 완성
        }

        // 연출 종료 시 초기화
        _spriteRenderer.color = Color.white;
        transform.localScale = originalScale;
    }
}