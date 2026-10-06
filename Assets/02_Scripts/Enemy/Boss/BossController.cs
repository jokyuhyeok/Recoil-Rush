using UnityEngine;
using System.Collections; // 코루틴을 사용하기 위해 추가
using System.Collections.Generic;

public class BossController : MonoBehaviour
{
    [SerializeField] private BossStatDataSO _stat;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Transform[] _firePoints;
    [SerializeField] private Transform _laserFirePoint;
    [SerializeField] private Transform[] _droneSpawnPoints;

    [Header("레이저 회전 공격 전용 변수")]
    [SerializeField] private BossLaserTest _rotatingLaserSkill; // 레이저 회전 Script
    [SerializeField] private GameObject _redSphereEffect;
    [SerializeField] private Transform _centerAirPosition;

    // 내부 상태 변수
    private Queue<IBossCommand> _commandQueue = new Queue<IBossCommand>();
    private bool _isExecuting = false;
    private int _currentPhase = 1;
    private IBossPhase[] _phases;

    // 외부 공개형 프로퍼티 (읽기 전용)
    public BossStatDataSO Stat => _stat;
    public Transform PlayerTransform { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }
    public Animator Anim { get; private set; }

    public Transform[] FirePoints => _firePoints;
    public BossLaserTest RotatingLaserSkill => _rotatingLaserSkill;
    public GameObject RedSphereEffect => _redSphereEffect;
    public Transform CenterAirPosition => _centerAirPosition;

    public bool IsLaserFinished { get; set; } = false;
    public int LaserSequenceIndex { get; set; } = 0;
    public bool IsHittingLeftWall { get; private set; } = false;
    public bool IsHittingRightWall { get; private set; } = false;

    // 유니티 생명주기
    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        Anim = GetComponent<Animator>();
        Rigidbody.gravityScale = _stat.HeavyGravity;
        PlayerCaching();

        _phases = new IBossPhase[]
        {
            null,
            new BossPhase1(),
            new BossPhase2(),
            new BossPhase3()
        };
    }

    private void Start()
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
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        IsHittingLeftWall = false;
        IsHittingRightWall = false;
    }

    // Public 기능 메서드
    public void SetPhase(int phaseIndex)
    {
        Debug.Log($"<color=red>보스: {phaseIndex}페이즈 돌입</color>");
        _currentPhase = phaseIndex;

        StopAllCoroutines();
        _commandQueue.Clear();
        _isExecuting = false;

        LoadPhaseCommands();
        StartCoroutine(ProcessCommands());
    }

    public void StopAllAttacks()
    {
        StopAllCoroutines();
        _commandQueue.Clear();
        Rigidbody.linearVelocity = Vector2.zero;
    }

    public void ShootLaserEvent()
    {
        if (_stat.BossLaserPrefab == null || _laserFirePoint == null) return;

        float offsetY = LaserSequenceIndex == 0 ? _stat.LaserBottomOffsetY
            : LaserSequenceIndex == 1 ? _stat.LaserTopOffsetY
            : _stat.LaserMidOffsetY;

        LaserSequenceIndex++;

        Vector3 spawnPosition = _laserFirePoint.position + new Vector3(0, offsetY, 0);
        GameObject laserObject = Instantiate(_stat.BossLaserPrefab, spawnPosition, Quaternion.identity);
        BossLaser laser = laserObject.GetComponent<BossLaser>();

        if (laser != null)
        {
            laser.FireLaser(_stat.LaserWarningTime, _stat.LaserBeamTime);
        }
    }

    public void SpawnDroneEvent()
    {
        if (_stat.DronePrefab == null || _droneSpawnPoints == null)
        {
            Debug.LogError("드론 프리팹 또는 스폰 포인트가 없습니다!");
            return;
        }
        Debug.Log("<color=green>보스: 발판용 시한폭탄 드론 공중 소환!</color>");

        foreach (Transform spawnPoint in _droneSpawnPoints)
        {
            if (spawnPoint == null) return;

            Instantiate(_stat.DronePrefab, spawnPoint.position, Quaternion.identity);
        }
    }

    // 잔상 생성
    public void SpawnGhost()
    {
        if (_stat.GhostPrefab == null || _spriteRenderer == null) return;

        GameObject ghostobject = Instantiate(_stat.GhostPrefab, transform.position, Quaternion.identity);
        BossGhostEffect ghost = ghostobject.GetComponent<BossGhostEffect>();

        if (ghost == null) return;
        ghost.Setup(_spriteRenderer.sprite, transform);
    }

    public void FinishLaserEvent()
    {
        IsLaserFinished = true;
    }

    // private 제어 메서드
    private IEnumerator ProcessCommands()   // 핵심 코루틴
    {
        // 보스가 살아있는동안 반복
        while (true)
        {
            if (!_isExecuting)
            {
                if (_commandQueue.Count > 0)
                {
                    _isExecuting = true;
                    IBossCommand currentCommand = _commandQueue.Dequeue();
                    yield return StartCoroutine(currentCommand.Execute(this));  // 꺼낸 패턴 실행 + 패턴이 끝날 때까지 대기
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

    private void LoadPhaseCommands()
    {
        if (_currentPhase >= 1 && _currentPhase < _phases.Length)
        {
            // 현재 페이즈 객체에게 패턴들을 받아와 큐에 넣음.
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

    void PlayerCaching()
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
}