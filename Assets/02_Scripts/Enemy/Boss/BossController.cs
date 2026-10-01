using UnityEngine;
using System.Collections; // 코루틴을 사용하기 위해 추가
using System.Collections.Generic;
using Unity.VisualScripting; // Queue를 사용하기 위해 추가

public class BossController : MonoBehaviour
{
    [SerializeField] private BossStatDataSO _stat;
    
    [SerializeField] private GameObject _laserBeamObject;
    [SerializeField] private GameObject _dronePrefab;
    [SerializeField] private GameObject _BombPrefab;
    [SerializeField] private Transform[] _droneSpawnPoints;
    [SerializeField] private GameObject _ghostPrefab;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    public Transform[] _firePoints;
    public GameObject _bulletPrefab;
    
    private Queue<IBossCommand> _commandQueue = new Queue<IBossCommand>();  // 패턴들을 순서대로 담아놓을 대기열입니다. 
    private bool _isExecuting = false;  // 현재 보스가 패턴 하나를 실행 중인지 확인용 변수입니다.
    private int _currentPhase = 1;
    
    public Transform playerTransform;
    public Rigidbody2D rb;
    public Animator anim;
    
    // 프로퍼티 - Command에서 프리팹에 접근할 수 있게 열어줍니다.
    public GameObject BulletPrefab => _bulletPrefab;
    public GameObject LaserBeamObject => _laserBeamObject;
    public GameObject DronePrefab => _dronePrefab;
    public GameObject BombPrefab => _BombPrefab;
    public Transform[] DroneSpawnPoints => _droneSpawnPoints;
    public Transform[] FirePoints => _firePoints;
    
    public bool isHittingLeftWall = false;
    public bool isHittingRightWall = false;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rb.gravityScale = _stat.heavyGravity;
        PlayerCaching();
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
        switch (_currentPhase)
        {
            case 1:
                {
                    EnqueuePhase1();
                    break;
                }
            case 2:
                {
                    EnqueuePhase2();
                    break;
                }
            case 3:
                {
                    EnqueuePhase3();
                    break;
                }
        }
    }
    
    // 각 페이즈별 패턴 조립만 전담하는 메서드들로 쪼갰습니다.
    private void EnqueuePhase1()
    {
        EnqueueCommand(new Cmd_Wait(_stat.waitTime));
        EnqueueCommand(new Cmd_ForwardDash(_stat.dashSpeed, 
            _stat.dashDuration,
            _stat.dashChargeTime, 
            _stat.dashGhostInterval));
        EnqueueCommand(new Cmd_Wait(_stat.waitTime));
        EnqueueCommand(new Cmd_BackwardDash(_stat.backDashSpeed, 
            _stat.backDashDuration,
            _stat.backDashChargeTime, 
            _stat.backDashGhostInterval));
        EnqueueCommand(new Cmd_Wait(_stat.waitTime));
        EnqueueCommand(new Cmd_SpreadShoot(
            _stat.leftBulletCount, _stat.leftStartAngle, _stat.leftEndAngle, _stat.leftFireDelay, _stat.leftBulletSpeed,
            _stat.rightBulletCount, _stat.rightStartAngle, _stat.rightEndAngle, _stat.rightFireDelay, _stat.rightBulletSpeed
        ));
        EnqueueCommand(new Cmd_Wait(0.1f));
        EnqueueCommand(new Cmd_SpreadShoot(
            _stat.leftBulletCount, _stat.leftStartAngle, _stat.leftEndAngle, _stat.leftFireDelay, _stat.leftBulletSpeed,
            _stat.rightBulletCount, _stat.rightStartAngle, _stat.rightEndAngle, _stat.rightFireDelay, _stat.rightBulletSpeed
        ));
        EnqueueCommand(new Cmd_Wait(0.1f));
        EnqueueCommand(new Cmd_SpreadShoot(
            _stat.leftBulletCount, _stat.leftStartAngle, _stat.leftEndAngle, _stat.leftFireDelay, _stat.leftBulletSpeed,
            _stat.rightBulletCount, _stat.rightStartAngle, _stat.rightEndAngle, _stat.rightFireDelay, _stat.rightBulletSpeed
        ));
    }

    private void EnqueuePhase2()
    {
        EnqueueCommand(new Cmd_Wait(_stat.waitTime));
        
        EnqueueCommand(new Cmd_ThrowBomb(
            _stat.centerMoveSpeed, _stat.numOfCountBomb, _stat.bouncePower, 
            _stat.bombTargetX, _stat.bombStopThreshold, _stat.bombSpreadAngle, 
            _stat.bombFireDelay, _stat.bombPostDelay));
            
        EnqueueCommand(new Cmd_BackwardDash(_stat.backDashSpeed, 
            _stat.backDashDuration,
            _stat.backDashChargeTime, 
            _stat.backDashGhostInterval));
    }

    private void EnqueuePhase3()
    {
        EnqueueCommand(new Cmd_Wait(_stat.waitTime));
        
        EnqueueCommand(new Cmd_ThrowBomb(
            _stat.centerMoveSpeed, _stat.numOfCountBomb, _stat.bouncePower, 
            _stat.bombTargetX, _stat.bombStopThreshold, _stat.bombSpreadAngle, 
            _stat.bombFireDelay, _stat.bombPostDelay));
            
        EnqueueCommand(new Cmd_LaserAndDrone(
            _stat.laserChargeTime, _stat.laserDuration, 
            _stat.droneSpawnDelay, _stat.droneSpawnCount));
    }
    
    public void StopAllAttacks()
    {
        StopAllCoroutines();
        _commandQueue.Clear();
        rb.linearVelocity = Vector2.zero; 
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 부딪힌 지점들의 정보를 하나씩 꺼내봅니다.
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.x > 0.7f)
            {
                isHittingLeftWall = true;
            }
            else if (contact.normal.x < -0.7f)
            {
                isHittingRightWall = true;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isHittingLeftWall = false;
        isHittingRightWall = false;
    }
    
    private void PlayerCaching()
    {
        // 플레이어 transform 컴포넌트를 처음에 캐싱해놓고 계속 추적하게 했습니다. 
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
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
