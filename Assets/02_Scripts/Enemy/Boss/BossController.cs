using UnityEngine;
using System.Collections; // 코루틴을 사용하기 위해 추가
using System.Collections.Generic;
using Unity.VisualScripting; // Queue를 사용하기 위해 추가

public class BossController : MonoBehaviour
{
    [SerializeField] private BossStatDataSO _stat;
    [SerializeField] private GameObject _ghostPrefab;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Transform[] _firePoints;
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _laserFirePoint;
    [SerializeField] private GameObject _bossLaserPrefab;
    [SerializeField] private GameObject _dronePrefab;
    [SerializeField] private Transform[] _droneSpawnPoints;

    [Header("파티클 이펙트 프리팹")]
    // 여기에 파티클 오브젝트들 넣으면 됩니다. 
    private Queue<IBossCommand> _commandQueue = new Queue<IBossCommand>(); // 패턴들을 순서대로 담아놓을 대기열입니다. 

    private bool _isExecuting = false; // 현재 보스가 패턴 하나를 실행 중인지 확인용 변수입니다.
    private int _currentPhase = 1;

    private IBossPhase[] _phases;

    public Transform PlayerTransform;
    public Rigidbody2D Rigidbody;
    public Animator Anim;

    // 프로퍼티 - Command에서 프리팹에 접근할 수 있게 열어줍니다.
    public GameObject BulletPrefab => _bulletPrefab;
    public Transform[] FirePoints => _firePoints;

    public bool IsLaserFinished { get; set; } = false;
    public int LaserSequenceIndex { get; set; } = 0;
    public bool IsHittingLeftWall = false;
    public bool IsHittingRightWall = false;

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

    // 새로운 패턴을 queue에 줄 세울 때 사용하는 메서드입니다. 
    private void EnqueueCommand(IBossCommand command)
    {
        _commandQueue.Enqueue(command);
    }

    // 핵심 코루틴입니다. (계속 큐를 감시하고 실행하는 루프)
    private IEnumerator ProcessCommands()
    {
        // 보스가 살아있는동안 계속 반복합니다.
        while (true)
        {
            if (!_isExecuting)
            {
                if (_commandQueue.Count > 0)
                {
                    _isExecuting = true;

                    // queue의 맨 앞에서 패턴을 하나 꺼냅니다.
                    IBossCommand currentCommand = _commandQueue.Dequeue();

                    // 꺼낸 패턴을 실행합니다. (패턴이 끝날 때까지 대기) 
                    yield return StartCoroutine(currentCommand.Execute(this));
                    _isExecuting = false;
                }
                else
                {
                    LoadPhaseCommands();
                }
            }

            // queue가 비어있거나 패턴이 실행 중이라면 다음 프레임까지 대기합니다. 
            yield return null;
        }
    }

    public void SetPhase(int phaseIndex)
    {
        Debug.Log($"<color=red>보스: {phaseIndex}페이즈 돌입!!</color>");
        _currentPhase = phaseIndex;

        StopAllCoroutines();

        _commandQueue.Clear();
        _isExecuting = false;

        LoadPhaseCommands();
        StartCoroutine(ProcessCommands());
    }

    private void LoadPhaseCommands()
    {
        if (_currentPhase >= 1 && _currentPhase < _phases.Length)
        {
            // 현재 페이즈 객체에게 패턴들을 받아와서 전부 큐에 넣습니다.
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

    public void StopAllAttacks()
    {
        StopAllCoroutines();
        _commandQueue.Clear();
        Rigidbody.linearVelocity = Vector2.zero;
    }

    public void ShootLaserEvent()
    {
        if (_bossLaserPrefab == null || _laserFirePoint == null) return;

        float offsetY = 0f;

        if (LaserSequenceIndex == 0)
        {
            offsetY = _stat.LaserBottomOffsetY;
        }
        else if (LaserSequenceIndex == 1)
        {
            offsetY = _stat.LaserTopOffsetY;
        }
        else
        {
            offsetY = _stat.LaserMidOffsetY;
        }

        LaserSequenceIndex++;

        Vector3 spawnPosition = _laserFirePoint.position + new Vector3(0, offsetY, 0);
        GameObject laserObject = Instantiate(_bossLaserPrefab, spawnPosition, Quaternion.identity);
        BossLaser laser = laserObject.GetComponent<BossLaser>();

        if (laser != null)
        {
            laser.FireLaser(_stat.LaserWarningTime, _stat.LaserBeamTime);
        }
    }

    // 드론들의 SpawnPoint들에 드론을 소환하는 이벤트 함수입니다.
    public void SpawnDroneEvent()
    {
        if (_dronePrefab == null || _droneSpawnPoints == null || _droneSpawnPoints.Length == 0)
        {
            Debug.LogWarning("드론 프리팹 또는 스폰 포인트가 없습니다!");
            return;
        }

        Debug.Log("<color=green>보스: 발판용 시한폭탄 드론 3대 공중 소환!</color>");

        // 지정된 스폰 위치 배열을 돌면서 드론을 각각 하나씩 생성합니다.
        foreach (Transform spawnPoint in _droneSpawnPoints)
        {
            if (spawnPoint != null)
            {
                Instantiate(_dronePrefab, spawnPoint.position, Quaternion.identity);
            }
        }
    }

    public void FinishLaserEvent()
    {
        IsLaserFinished = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 부딪힌 지점들의 정보를 하나씩 꺼내봅니다.
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

    private void PlayerCaching()
    {
        // 플레이어 transform 컴포넌트를 처음에 캐싱해놓고 계속 추적하게 했습니다. 
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            PlayerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("플레이어가 씬에 없습니다!");
        }
    }

    // 잔상 생성을 전담하는 메서드입니다. 
    public void SpawnGhost()
    {
        if (_ghostPrefab == null || _spriteRenderer == null) return;

        // 프리팹 생성 및 세팅입니다.
        GameObject ghostobject = Instantiate(_ghostPrefab, transform.position, Quaternion.identity);
        BossGhostEffect ghost = ghostobject.GetComponent<BossGhostEffect>();

        if (ghost != null)
        {
            ghost.Setup(_spriteRenderer.sprite, transform);
        }
    }
}