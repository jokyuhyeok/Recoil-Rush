using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 보스의 전체적인 상태, 물리, 패턴(명령) 큐를 관리하는 메인 컨트롤러입니다.
/// 시각 효과(VFX)는 BossVFXController가 담당합니다.
/// </summary>
public class BossController : MonoBehaviour
{
    public BossVFX VFX { get; private set; } // 분리된 VFX 컨트롤러 참조
    public BossGameFeel GameFeel { get; private set; }
    public BossStatDataSO Stat => _stat;
    public Transform PlayerTransform { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }
    public Animator Anim { get; private set; }

    // 외부 커맨드 스크립트들이 에러 없이 동작하도록 프로퍼티 연결(Facade 패턴)
    public SpriteRenderer BossSpriteRenderer => GetComponent<SpriteRenderer>();
    public Transform[] FirePoints => _firePoints;
    public BossLaserTest RotatingLaserSkill => _rotatingLaserSkill;
    public Transform CenterAirPosition => _centerAirPosition;

    // 시각 효과 객체들은 VFX 컨트롤러를 거쳐 반환합니다.
    public GameObject RedSphereEffect => VFX.RedSphereEffect;
    public GameObject IdleSmokeEffect => VFX.IdleSmokeEffect;
    public GameObject DashChargeEffect => VFX.DashChargeEffect;
    public GameObject DashFireEffect => VFX.DashFireEffect;
    public GameObject HitWallEffectPrefab => VFX.HitWallEffectPrefab;
    public GameObject HandSwirlEffect => VFX.HandSwirlEffect;

    public int LaserSequenceIndex { get; set; } = 0;
    public bool IsHittingLeftWall { get; private set; } = false;
    public bool IsHittingRightWall { get; private set; } = false;

    [Header("기본 세팅")]
    [SerializeField] private BossStatDataSO _stat;

    [Header("레이저 및 특수 공격")]
    [SerializeField] private Transform[] _firePoints;
    [SerializeField] private Transform _laserFirePoint;
    [SerializeField] private BossLaserTest _rotatingLaserSkill;
    [SerializeField] private Transform _centerAirPosition;

    private AudioSource _audioSource;
    private Queue<IBossCommand> _commandQueue = new Queue<IBossCommand>();
    private IBossPhase[] _phases;

    private int _currentPhase = 1;
    private bool _isExecuting = false;
    private bool _hasLandedOnSpawn = false;
    private Vector3 _initialPosition;

    /// <summary>
    /// 컴포넌트들을 캐싱하고 물리력, 상태 머신 인스턴스를 초기화합니다.
    /// </summary>
    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        Anim = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        GameFeel = GetComponent<BossGameFeel>();
        VFX = GetComponent<BossVFX>();

        Rigidbody.gravityScale = _stat.HeavyGravity;
        PlayerCaching();

        _phases = new IBossPhase[]
        {
            null,
            new BossPhase1(),
            new BossPhase2(),
            new BossPhase3()
        };

        _initialPosition = transform.position;
    }

    /// <summary>
    /// 보스 활성화 시 컷씬이 아니라면 1페이즈 전투를 바로 시작합니다.
    /// </summary>
    private void OnEnable()
    {
        if (GameManager.Instance.IsCutscene) return;
        SetPhase(1);
    }

    /// <summary>
    /// 벽이나 바닥과 처음 충돌했을 때를 감지하여 플래그를 변경합니다.
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.x > 0.7f) IsHittingLeftWall = true;
            else if (contact.normal.x < -0.7f) IsHittingRightWall = true;

            // 공중에서 바닥으로 처음 떨어졌을 때 착지 이펙트
            if (!_hasLandedOnSpawn && contact.normal.y > 0.7f)
            {
                _hasLandedOnSpawn = true;
                if (GameFeel != null) GameFeel.PlayLandImpact();
                PlayShakeHeavy(); // 착지 시 쾅!
            }
        }
    }

    /// <summary>
    /// 벽에 비비고 있는 동안 지속적으로 충돌 상태를 유지합니다.
    /// </summary>
    private void OnCollisionStay2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.x > 0.7f) IsHittingLeftWall = true;
            else if (contact.normal.x < -0.7f) IsHittingRightWall = true;
        }
    }

    /// <summary>
    /// 벽에서 떨어졌을 때 충돌 플래그를 해제합니다.
    /// </summary>
    private void OnCollisionExit2D(Collision2D collision)
    {
        IsHittingLeftWall = false;
        IsHittingRightWall = false;
    }

    // =========================================================
    // 외부 커맨드 호출용 헬퍼 (VFX 컨트롤러로 전달)
    // =========================================================

    /// <summary> VFX 컨트롤러를 통해 가벼운 카메라 진동을 호출합니다. </summary>
    public void PlayShakeLight(float forceOverride = -1f) => VFX.PlayShakeLight(forceOverride);

    /// <summary> VFX 컨트롤러를 통해 무거운 카메라 진동을 호출합니다. </summary>
    public void PlayShakeHeavy(float forceOverride = -1f) => VFX.PlayShakeHeavy(forceOverride);

    /// <summary> VFX 컨트롤러를 통해 지진 효과를 호출합니다. </summary>
    public void PlayShakeEarthquake(float forceOverride = -1f) => VFX.PlayShakeEarthquake(forceOverride);

    /// <summary>
    /// 지정된 효과음을 보스의 오디오 소스를 통해 재생합니다.
    /// </summary>
    public void PlayBossSound(SoundEffectType type, bool isLoop = false)
    {
        if (_audioSource == null) return;
        AudioClip clip = AudioManager.Instance.GetSfxClip(type);
        if (clip != null)
        {
            _audioSource.clip = clip;
            _audioSource.loop = isLoop;
            _audioSource.Play();
        }
    }

    /// <summary>
    /// 지정된 효과음을 한 번만 재생하며 중첩을 허용합니다 (PlayOneShot).
    /// </summary>
    public void PlayBossSoundOneShot(SoundEffectType type)
    {
        if (_audioSource == null) return;
        AudioClip clip = AudioManager.Instance.GetSfxClip(type);
        if (clip != null) _audioSource.PlayOneShot(clip);
    }

    /// <summary>
    /// 현재 재생 중인 보스의 효과음을 즉시 중단합니다.
    /// </summary>
    public void StopBossSound()
    {
        if (_audioSource != null && _audioSource.isPlaying) _audioSource.Stop();
    }

    /// <summary>
    /// 보스의 페이즈를 전환하고 상태와 큐를 리셋한 뒤, 전환 연출을 시작합니다.
    /// </summary>
    public void SetPhase(int phaseIndex)
    {
        Debug.Log($"<color=red>보스: {phaseIndex}페이즈 돌입</color>");
        _currentPhase = phaseIndex;

        StopAllAttacks();
        VFX.DisableAllVFX(); // 켜져있던 모든 시각 효과 초기화

        _isExecuting = false;
        StartCoroutine(TransitionAndStart(phaseIndex));
    }

    /// <summary>
    /// 실행 중인 패턴, 코루틴, 움직임을 강제로 멈추고 큐를 비웁니다.
    /// </summary>
    public void StopAllAttacks()
    {
        StopAllCoroutines();
        VFX.ResetFlashMaterial();

        if (_commandQueue != null) _commandQueue.Clear();
        if (Rigidbody != null) Rigidbody.linearVelocity = Vector2.zero;

        StopBossSound();
    }

    /// <summary>
    /// VFX 컨트롤러를 호출하여 피격 번쩍임 효과를 재생합니다.
    /// </summary>
    public void PlayHitFlash()
    {
        VFX.PlayHitFlash(); // 시각 연출 전담 스크립트 호출
    }

    /// <summary>
    /// 돌진 등 패턴 종료 후 충돌 플래그를 수동으로 강제 초기화합니다.
    /// </summary>
    public void ResetWallCollision()
    {
        IsHittingLeftWall = false;
        IsHittingRightWall = false;
    }

    /// <summary>
    /// 다중 레이저 패턴 중 애니메이션 이벤트에서 호출되어 개별 레이저를 스폰합니다.
    /// </summary>
    public void ShootLaserEvent()
    {
        if (_stat.BossLaserPrefab == null || _laserFirePoint == null) return;
        if (_stat.MultiLaserOffsets == null || _stat.MultiLaserOffsets.Length == 0) return;

        int index = Mathf.Clamp(LaserSequenceIndex, 0, _stat.MultiLaserOffsets.Length - 1);
        float offsetY = _stat.MultiLaserOffsets[index];
        LaserSequenceIndex++;

        Vector3 spawnPosition = _laserFirePoint.position + new Vector3(0, offsetY, 0);
        GameObject laserObject = Instantiate(_stat.BossLaserPrefab, spawnPosition, Quaternion.identity);
        BossLaser laser = laserObject.GetComponent<BossLaser>();

        if (laser != null) laser.FireLaser(_stat.LaserWarningTime, _stat.LaserBeamTime);

        PlayShakeHeavy(); // 레이저 발사 시 반동
    }

    /// <summary>
    /// 돌진 패턴 시 보스 위치에 옅은 잔상(애프터이미지)을 생성합니다.
    /// </summary>
    public void SpawnGhost()
    {
        if (_stat.GhostPrefab == null) return;

        GameObject ghostobject = Instantiate(_stat.GhostPrefab, transform.position, Quaternion.identity);
        BossGhostEffect ghost = ghostobject.GetComponent<BossGhostEffect>();

        if (ghost != null)
        {
            ghost.Setup(BossSpriteRenderer.sprite, transform);
            ghost.ExplodeWithDelay(0.15f, 0.4f);
        }
    }

    /// <summary>
    /// 씬 내의 플레이어를 찾아 참조값에 저장합니다.
    /// </summary>
    private void PlayerCaching()
    {
        // 플레이어 transform 컴포넌트를 처음에 캐싱해놓고 계속 추적하게 합니다.
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) PlayerTransform = player.transform;
        else Debug.LogError("플레이어가 씬에 없습니다!");
    }

    /// <summary>
    /// 현재 페이즈 객체에서 패턴 리스트를 불러와 실행 큐(Queue)에 적재합니다.
    /// </summary>
    private void LoadPhaseCommands()
    {
        if (_currentPhase >= 1 && _currentPhase < _phases.Length)
        {
            IEnumerable<IBossCommand> commands = _phases[_currentPhase].PhaseCommands(_stat);
            foreach (IBossCommand cmd in commands) _commandQueue.Enqueue(cmd);
        }
        else
        {
            Debug.LogWarning("잘못된 페이즈 인덱스입니다!");
        }
    }

    /// <summary>
    /// 페이즈 전환 연출이 끝날 때까지 기다렸다가 패턴 큐 처리를 시작하는 코루틴입니다.
    /// </summary>
    private IEnumerator TransitionAndStart(int phaseIndex)
    {
        if (phaseIndex > 1)
        {
            // 움직임 제어 (돌아다니지 못하게 정지)
            Rigidbody.linearVelocity = Vector2.zero;
            PlayBossSoundOneShot(SoundEffectType.DASH_PREPARATION);

            // 시각 연출은 VFX 컨트롤러에 코루틴 실행을 위임하고 끝날 때까지 대기합니다.
            yield return StartCoroutine(VFX.PlayPhaseTransitionVFX(2.0f));

            AudioManager.Instance.PlaySFX(SoundEffectType.BOSS_HIT);
        }

        LoadPhaseCommands();
        StartCoroutine(ProcessCommands());
    }

    /// <summary>
    /// 큐에 쌓인 보스의 패턴(명령)을 하나씩 꺼내 순차적으로 비동기 실행하는 메인 루프입니다.
    /// </summary>
    private IEnumerator ProcessCommands()
    {
        while (true)
        {
            // 1. 이미 다른 패턴을 실행 중이라면, 다음 프레임까지 대기
            if (_isExecuting)
            {
                yield return null;
                continue;
            }

            // 2. 큐가 비어있다면 패턴을 새로 장전하고, 다음 프레임까지 대기
            if (_commandQueue.Count == 0)
            {
                LoadPhaseCommands();
                yield return null;
                continue;
            }

            // 3. 실행 중도 아니고 큐도 비어있지 않다면, 명령을 꺼내서 실행
            _isExecuting = true;
            IBossCommand currentCommand = _commandQueue.Dequeue();

            yield return StartCoroutine(currentCommand.Execute(this));

            _isExecuting = false;

            yield return null;
        }
    }

    /// <summary>
    /// 플레이어 사망 시 보스의 상태, 체력, 위치, 물리력을 초기화합니다.
    /// </summary>
    public void ResetBoss()
    {
        // 1. 모든 공격, 사운드 및 이펙트 정지
        StopAllAttacks();
        VFX.DisableAllVFX();
        if (_rotatingLaserSkill != null) _rotatingLaserSkill.gameObject.SetActive(false);

        // 2. 물리력 초기화 및 초기 위치로 복귀
        if (Rigidbody != null) Rigidbody.linearVelocity = Vector2.zero;
        if (_stat != null) Rigidbody.gravityScale = _stat.HeavyGravity;

        LaserSequenceIndex = 0;
        transform.position = _initialPosition;
        _hasLandedOnSpawn = false;

        // 3. 애니메이션 초기화 (아직 애니메이터가 켜진 적이 없다면 Play를 무시하도록 처리)
        if (Anim != null && Anim.isActiveAndEnabled) Anim.Play("BossIdle");

        // 4. 체력 원상복구
        BossHealth health = GetComponent<BossHealth>();
        if (health != null) health.ResetHealth();

        // 5. 보스를 꺼서 숨김 (플레이어가 다시 방에 들어오면 트리거가 켜줌 -> OnEnable 실행)
        gameObject.SetActive(false);
    }
}