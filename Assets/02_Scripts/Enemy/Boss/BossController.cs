using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;

/// <summary>
/// 보스의 전체적인 상태, 물리, 이펙트, 패턴(명령) 큐를 관리하는 메인 컨트롤러입니다.
/// </summary>
public class BossController : MonoBehaviour
{
    public BossGameFeel GameFeel { get; private set; }
    public BossStatDataSO Stat => _stat;
    public Transform PlayerTransform { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }
    public Animator Anim { get; private set; }
    public SpriteRenderer BossSpriteRenderer => _spriteRenderer;
    public Transform[] FirePoints => _firePoints;
    public BossLaserTest RotatingLaserSkill => _rotatingLaserSkill;
    public GameObject RedSphereEffect => _redSphereEffect;
    public Transform CenterAirPosition => _centerAirPosition;
    public GameObject IdleSmokeEffect => _idleSmokeEffect;
    public GameObject DashChargeEffect => _dashChargeEffect;
    public GameObject DashFireEffect => _dashFireEffect;
    public GameObject HitWallEffectPrefab => _hitWallEffectPrefab;
    public GameObject HandSwirlEffect => _handSwirlEffect;
    public int LaserSequenceIndex { get; set; } = 0;
    public bool IsHittingLeftWall { get; private set; } = false;
    public bool IsHittingRightWall { get; private set; } = false;

    [Header("기본 세팅")]
    [SerializeField] private BossStatDataSO _stat;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Material _flashMaterial;

    [Header("시각 효과 (VFX)")]
    [SerializeField] private Transform _vfxGroup;
    [SerializeField] private GameObject _idleSmokeEffect;
    [SerializeField] private GameObject _dashChargeEffect;
    [SerializeField] private GameObject _dashFireEffect;
    [SerializeField] private GameObject _hitWallEffectPrefab;
    [SerializeField] private GameObject _handSwirlEffect;

    [Header("레이저 및 특수 공격")]
    [SerializeField] private Transform[] _firePoints;
    [SerializeField] private Transform _laserFirePoint;
    [SerializeField] private BossLaserTest _rotatingLaserSkill;
    [SerializeField] private GameObject _redSphereEffect;
    [SerializeField] private Transform _centerAirPosition;

    [Header("카메라 흔들림 발생기 (인스펙터 할당 필수)")]
    [SerializeField] private CinemachineImpulseSource _lightShakeSource; // 피격, 약한 타격 (Dissipating 적용)
    [SerializeField] private CinemachineImpulseSource _heavyShakeSource; // 쿵! 찍기, 충돌 (강하고 짧음)
    [SerializeField] private CinemachineImpulseSource _earthquakeShakeSource; // 페이즈 전환, 사망 (Rumble, 길고 무거움)

    [Header("카메라 흔들림 강도")]
    [SerializeField] private float _hitShakeForce = 0.5f; // 피격 시
    [SerializeField] private float _wallSlamForce = 2.0f; // 벽 충돌, 땅 찍기 시
    [SerializeField] private float _phaseChangeForce = 1.5f; // 페이즈 전환 굉음
    [SerializeField] private float _deathEarthquakeForce = 3.5f; // 사망 연출 시

    private Material _originalMaterial;
    private Coroutine _flashCoroutine;
    private AudioSource _audioSource;

    private Queue<IBossCommand> _commandQueue = new Queue<IBossCommand>();
    private IBossPhase[] _phases;

    private int _currentPhase = 1;
    private bool _isExecuting = false;
    private bool _hasLandedOnSpawn = false;
    private Vector3 _initialPosition;

    private WaitForSeconds _hitFlashDelay = new WaitForSeconds(0.05f);
    private WaitForSeconds _phaseBlinkDelay = new WaitForSeconds(0.3f);

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        Anim = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        GameFeel = GetComponent<BossGameFeel>();

        Rigidbody.gravityScale = _stat.HeavyGravity;
        PlayerCaching();

        if (_spriteRenderer != null)
        {
            _originalMaterial = _spriteRenderer.material;
        }

        _phases = new IBossPhase[]
        {
            null,
            new BossPhase1(),
            new BossPhase2(),
            new BossPhase3()
        };

        _initialPosition = transform.position;
    }

    private void OnEnable()
    {
        if (GameManager.Instance.IsCutscene)
            return;

        SetPhase(1);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.x > 0.7f)
            {
                IsHittingLeftWall = true;
            }
            else if (contact.normal.x < -0.7f)
            {
                IsHittingRightWall = true;
            }

            if (!_hasLandedOnSpawn && contact.normal.y > 0.7f)
            {
                _hasLandedOnSpawn = true;
                if (GameFeel != null)
                {
                    GameFeel.PlayLandImpact();
                }
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.x > 0.7f)
            {
                IsHittingLeftWall = true;
            }
            else if (contact.normal.x < -0.7f)
            {
                IsHittingRightWall = true;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        IsHittingLeftWall = false;
        IsHittingRightWall = false;
    }

    // =========================================================
    // 🌟 [추가] 외부 호출용 시네머신 흔들림 헬퍼 함수들
    // =========================================================

    public void PlayShakeLight(float forceOverride = -1f)
    {
        if (_lightShakeSource != null)
        {
            float force = forceOverride < 0 ? _hitShakeForce : forceOverride;
            Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;
            _lightShakeSource.GenerateImpulse(randomDir * force);
        }
    }

    public void PlayShakeHeavy(float forceOverride = -1f)
    {
        if (_heavyShakeSource != null)
        {
            float force = forceOverride < 0 ? _wallSlamForce : forceOverride;
            Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;
            _heavyShakeSource.GenerateImpulse(randomDir * force);
        }
    }

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

    public void PlayBossSound(SoundEffectType type, bool isLoop = false)
    {
        if (_audioSource == null)
        {
            return;
        }

        AudioClip clip = AudioManager.Instance.GetSfxClip(type);
        if (clip != null)
        {
            _audioSource.clip = clip;
            _audioSource.loop = isLoop;
            _audioSource.Play();
        }
    }

    public void PlayBossSoundOneShot(SoundEffectType type)
    {
        if (_audioSource == null)
        {
            return;
        }

        AudioClip clip = AudioManager.Instance.GetSfxClip(type);
        if (clip != null)
        {
            _audioSource.PlayOneShot(clip);
        }
    }

    public void StopBossSound()
    {
        if (_audioSource != null && _audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
    }

    public void SetPhase(int phaseIndex)
    {
        Debug.Log($"<color=red>보스: {phaseIndex}페이즈 돌입</color>");
        _currentPhase = phaseIndex;

        StopAllAttacks();

        if (_vfxGroup == null)
        {
            Debug.LogError("보스 프리팹 안의 VFX_Group을 연결해주세요.");
        }
        else
        {
            foreach (Transform child in _vfxGroup)
            {
                child.gameObject.SetActive(false);
            }
        }

        _isExecuting = false;
        StartCoroutine(TransitionAndStart(phaseIndex));
    }

    public void StopAllAttacks()
    {
        StopAllCoroutines();
        ResetFlashMaterial();

        if (_commandQueue != null)
        {
            _commandQueue.Clear();
        }

        if (Rigidbody != null)
        {
            Rigidbody.linearVelocity = Vector2.zero;
        }

        StopBossSound();
    }

    public void PlayHitFlash()
    {
        if (_spriteRenderer == null || _flashMaterial == null)
        {
            return;
        }

        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
        }

        _flashCoroutine = StartCoroutine(HitFlashRoutine());
        PlayShakeLight();
    }

    public void ResetWallCollision()
    {
        IsHittingLeftWall = false;
        IsHittingRightWall = false;
    }

    public void ShootLaserEvent()
    {
        if (_stat.BossLaserPrefab == null || _laserFirePoint == null)
        {
            return;
        }

        if (_stat.MultiLaserOffsets == null || _stat.MultiLaserOffsets.Length == 0)
        {
            return;
        }

        int index = Mathf.Clamp(LaserSequenceIndex, 0, _stat.MultiLaserOffsets.Length - 1);
        float offsetY = _stat.MultiLaserOffsets[index];

        LaserSequenceIndex++;

        Vector3 spawnPosition = _laserFirePoint.position + new Vector3(0, offsetY, 0);
        GameObject laserObject = Instantiate(_stat.BossLaserPrefab, spawnPosition, Quaternion.identity);
        BossLaser laser = laserObject.GetComponent<BossLaser>();

        if (laser != null)
        {
            laser.FireLaser(_stat.LaserWarningTime, _stat.LaserBeamTime);
        }

        PlayShakeHeavy(_wallSlamForce * 0.8f);
    }

    public void SpawnGhost()
    {
        if (_stat.GhostPrefab == null || _spriteRenderer == null)
        {
            return;
        }

        GameObject ghostobject = Instantiate(_stat.GhostPrefab, transform.position, Quaternion.identity);
        BossGhostEffect ghost = ghostobject.GetComponent<BossGhostEffect>();

        if (ghost == null)
        {
            return;
        }

        ghost.Setup(_spriteRenderer.sprite, transform);
        ghost.ExplodeWithDelay(0.15f, 0.4f);
    }

    private void PlayerCaching()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            PlayerTransform = player.transform;
        }
        else
        {
            Debug.LogError("플레이어가 씬에 없습니다!");
        }
    }

    private void LoadPhaseCommands()
    {
        if (_currentPhase >= 1 && _currentPhase < _phases.Length)
        {
            IEnumerable<IBossCommand> commands = _phases[_currentPhase].PhaseCommands(_stat);
            foreach (IBossCommand cmd in commands)
            {
                _commandQueue.Enqueue(cmd);
            }
        }
        else
        {
            Debug.LogWarning("잘못된 페이즈 인덱스입니다!");
        }
    }

    private void ResetFlashMaterial()
    {
        if (_spriteRenderer != null && _originalMaterial != null)
        {
            _spriteRenderer.material = _originalMaterial;
        }
    }

    private IEnumerator TransitionAndStart(int phaseIndex)
    {
        if (phaseIndex > 1)
        {
            // 움직임 제어 (돌아다니지 못하게 정지)
            Rigidbody.linearVelocity = Vector2.zero;

            // 1. 글리치 효과 및 화면 둥둥 흔들림 시작 (2초간)
            if (GameFeel != null)
            {
                GameFeel.PlayGlitchEffect(2.0f);
            }

            PlayShakeEarthquake(_phaseChangeForce * 0.5f);
            PlayBossSoundOneShot(SoundEffectType.DASH_PREPARATION);

            Vector3 originalScale = transform.localScale;

            float elapsed = 0f;
            float blinkInterval = 0.3f;
            bool isRed = false;

            while (elapsed < 2.0f)
            {
                // 페이즈 상관없이 빨간색 점멸
                _spriteRenderer.color = isRed ? Color.red : Color.white;
                isRed = !isRed;

                yield return new WaitForSeconds(blinkInterval);
                elapsed += blinkInterval;
                blinkInterval = Mathf.Max(0.05f, blinkInterval - 0.04f);
            }

            _spriteRenderer.color = Color.white;
            transform.localScale = originalScale;

            PlayShakeHeavy(_wallSlamForce * 1.5f);
            AudioManager.Instance.PlaySFX(SoundEffectType.BOSS_HIT);
        }

        LoadPhaseCommands();
        StartCoroutine(ProcessCommands());
    }

    private IEnumerator ProcessCommands()
    {
        while (true)
        {
            if (!_isExecuting)
            {
                if (_commandQueue.Count > 0)
                {
                    _isExecuting = true;
                    IBossCommand currentCommand = _commandQueue.Dequeue();
                    yield return StartCoroutine(currentCommand.Execute(this));
                    _isExecuting = false;
                }
                else
                {
                    LoadPhaseCommands();
                }
            }

            yield return null;
        }
    }

    private IEnumerator HitFlashRoutine()
    {
        _spriteRenderer.material = _flashMaterial;
        yield return _hitFlashDelay;
        _spriteRenderer.material = _originalMaterial;
    }

    public void ResetBoss()
    {
        // 1. 모든 공격 및 사운드 정지
        StopAllAttacks();

        // 2. 켜져있던 이펙트 끄기
        if (_redSphereEffect != null) _redSphereEffect.SetActive(false);
        if (_handSwirlEffect != null) _handSwirlEffect.SetActive(false);
        if (_rotatingLaserSkill != null) _rotatingLaserSkill.enabled = false;

        // 3. 물리력 초기화 및 초기 위치로 복귀
        if (Rigidbody == null) return;
        Rigidbody.linearVelocity = Vector2.zero;

        if (_stat == null) return;
        Rigidbody.gravityScale = _stat.HeavyGravity;

        LaserSequenceIndex = 0;
        transform.position = _initialPosition;
        _hasLandedOnSpawn = false;

        // 4. 애니메이션 초기화
        if (Anim == null) return;

        // 아직 애니메이터가 켜진 적이 없다면 Play를 무시하도록 처리
        if (Anim.isActiveAndEnabled)
        {
            Anim.Play("Idle");
        }

        // 5. 체력 원상복구
        BossHealth health = GetComponent<BossHealth>();
        if (health != null) health.ResetHealth();

        if (_rotatingLaserSkill != null) _rotatingLaserSkill.enabled = true;

        // 6. 보스를 꺼서 숨김 (플레이어가 다시 방에 들어오면 트리거가 켜줌 -> OnEnable 실행)
        gameObject.SetActive(false);
    }

    public IEnumerator PlayDeathVFX(float duration)
    {
        // 1. 모든 공격 및 사운드 정지
        StopAllAttacks();

        // 2. 켜져있던 이펙트 끄기
        if (_redSphereEffect != null) _redSphereEffect.SetActive(false);
        if (_handSwirlEffect != null) _handSwirlEffect.SetActive(false);
        if (_rotatingLaserSkill != null) _rotatingLaserSkill.enabled = false;

        // 3. 물리력 초기화
        if (Rigidbody != null)
        {
            Rigidbody.linearVelocity = Vector2.zero;
        }

        if (_stat != null)
        {
            Rigidbody.gravityScale = _stat.HeavyGravity;
        }


        if (GameFeel != null) GameFeel.PlayGlitchEffect(duration);
        PlayShakeEarthquake(_deathEarthquakeForce);
        PlayBossSoundOneShot(SoundEffectType.DASH_PREPARATION);

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

            // 1. 색상 점멸: 시간이 지날수록 점멸 간격이 0.5초에서 0.03초까지 '서서히' 짧아짐
            float currentBlinkInterval = Mathf.Lerp(0.5f, 0.03f, elapsed / duration);

            if (blinkTimer >= currentBlinkInterval)
            {
                blinkTimer = 0f;
                _spriteRenderer.color = isColored ? purpleColor : Color.white;
                isColored = !isColored;
            }

            // 2. 늘줄늘줄 요동: 시간이 지날수록 진동폭(0.05 -> 0.15)과 진동 속도(15 -> 45)가 미친듯이 증가
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