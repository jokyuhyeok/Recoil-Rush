using UnityEngine;
using System.Collections; // 코루틴을 사용하기 위해 추가
using System.Collections.Generic; // Queue를 사용하기 위해 추가

public class BossController : MonoBehaviour
{
    [SerializeField] private float _waitTime = 1.0f;
    [SerializeField] private float _dashSpeed = 1.0f;
    [SerializeField] private float _dashDuration = 1.0f;
    
    private Queue<IBossCommand> _commandQueue = new Queue<IBossCommand>();  // 패턴들을 순서대로 담아놓을 대기열입니다. 
    private bool _isExecuting = false;  // 현재 보스가 패턴 하나를 실행 중인지 확인용 변수입니다.
    
    public Rigidbody2D rb;
    public Transform playerTransform;
    public bool isHittingLeftWall = false;
    public bool isHittingRightWall = false;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        PlayerCaching();
    }
    
    private void Start()
    {
        StartCoroutine(ProcessCommands());
        
        EnqueueCommand(new Cmd_Wait(_waitTime));
        EnqueueCommand(new Cmd_ForwardDash(_dashSpeed, _dashDuration));
        EnqueueCommand(new Cmd_Wait(_waitTime));
        EnqueueCommand(new Cmd_BackwardDash(_dashSpeed, _dashDuration));
        EnqueueCommand(new Cmd_Wait(_waitTime));
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
            if ((!_isExecuting) && (_commandQueue.Count > 0))
            {
                _isExecuting = true;
                // queue의 맨 앞에서 패턴을 하나 꺼냅니다.
                IBossCommand currentCommand = _commandQueue.Dequeue();
                
                // 꺼낸 패턴을 실행합니다. (패턴이 끝날 때까지 대기) 
                yield return StartCoroutine(currentCommand.Execute(this));
                _isExecuting = false;
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
