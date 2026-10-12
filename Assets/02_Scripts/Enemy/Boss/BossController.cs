using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
        _commandQueue.Clear();
        Rigidbody.linearVelocity = Vector2.zero;
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
            Rigidbody.linearVelocity = Vector2.zero;
            Rigidbody.gravityScale = 0f;

            PlayBossSoundOneShot(SoundEffectType.DASH_PREPARATION);

            float elapsed = 0f;
            while (elapsed < 2.0f)
            {
                PlayHitFlash();
                yield return _phaseBlinkDelay;
                elapsed += 0.3f;
            }

            Rigidbody.gravityScale = _stat.HeavyGravity;
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
        if (_rotatingLaserSkill != null) _rotatingLaserSkill.gameObject.SetActive(false);

        // 3. 물리력 초기화 및 초기 위치로 복귀
        Rigidbody.linearVelocity = Vector2.zero;
        Rigidbody.gravityScale = _stat.HeavyGravity;
        LaserSequenceIndex = 0;
        transform.position = _initialPosition;
        _hasLandedOnSpawn = false;

        // 4. 애니메이션 초기화
        Anim.Play("Idle");

        // 5. 체력 원상복구
        BossHealth health = GetComponent<BossHealth>();
        if (health != null) health.ResetHealth();

        // 6. 보스를 꺼서 숨김 (플레이어가 다시 방에 들어오면 트리거가 켜줌 -> OnEnable 실행)
        gameObject.SetActive(false);
    }
}