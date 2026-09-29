using UnityEngine;
using System.Collections; // 코루틴을 사용하기 위해 추가
using System.Collections.Generic; // Queue를 사용하기 위해 추가

public class BossController : MonoBehaviour
{
    [SerializeField] private BossStatDataSO _stat;
    
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private GameObject _laserBeamObject;
    [SerializeField] private GameObject _dronePrefab;
    [SerializeField] private GameObject _BombPrefab;
    [SerializeField] private Transform[] _droneSpawnPoints;
    [SerializeField] private Transform _firePoint;
    
    private Queue<IBossCommand> _commandQueue = new Queue<IBossCommand>();  // 패턴들을 순서대로 담아놓을 대기열입니다. 
    private bool _isExecuting = false;  // 현재 보스가 패턴 하나를 실행 중인지 확인용 변수입니다.
    private int _currentPhase = 1;
    
    public Rigidbody2D rb;
    public Transform playerTransform;
    
    // 프로퍼티 - Command에서 프리팹에 접근할 수 있게 열어줍니다.
    public GameObject BulletPrefab => _bulletPrefab;
    public GameObject LaserBeamObject => _laserBeamObject;
    public GameObject DronePrefab => _dronePrefab;
    public GameObject BombPrefab => _BombPrefab;
    public Transform[] DroneSpawnPoints => _droneSpawnPoints;
    public Transform FirePoint => _firePoint;
    
    public bool isHittingLeftWall = false;
    public bool isHittingRightWall = false;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (collision.transform.position.x < transform.position.x)
            {
                isHittingLeftWall = true;
            }
            else
            {
                isHittingRightWall = true;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (collision.transform.position.x < transform.position.x)
            {
                isHittingLeftWall = false;
            }
            else
            {
                isHittingRightWall = false;
            }
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
        
        EnqueueCommand(new Cmd_SpreadShoot(
            _stat.spreadBulletCount, _stat.spreadStartAngle, _stat.spreadEndAngle, 
            _stat.spreadFireDelay, _stat.spreadBulletSpeed));
        EnqueueCommand(new Cmd_SpreadShoot(
            _stat.spreadBulletCount, _stat.spreadStartAngle, _stat.spreadEndAngle, 
            _stat.spreadFireDelay, _stat.spreadBulletSpeed));
    }

    private void EnqueuePhase2()
    {
        EnqueueCommand(new Cmd_Wait(_stat.waitTime));
        
        EnqueueCommand(new Cmd_ThrowBomb(
            _stat.centerMoveSpeed, _stat.numOfCountBomb, _stat.bouncePower, 
            _stat.bombTargetX, _stat.bombStopThreshold, _stat.bombSpreadAngle, 
            _stat.bombFireDelay, _stat.bombPostDelay));
            
        EnqueueCommand(new Cmd_BackwardDash(_stat.dashSpeed, _stat.dashDuration));
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
}
