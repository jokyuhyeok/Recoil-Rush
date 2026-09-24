using UnityEngine;

// 공중 적 (드론)
public class EnemyDrone : EnemyBase
{
    // 행동 패턴에 관련된 변수입니다.
    [SerializeField] private float _flySpeed = 2f;
    [SerializeField] private float _detectRange = 10f;
    [SerializeField] private float _attackRange = 8f;
    [SerializeField] private float _alarmTime = 1f;

    // 제자리에서 둥둥 떠다니는 Hovering 연출을 위한 변수입니다.
    [SerializeField] private float _hoverHeight = 0.5f;
    [SerializeField] private float _hoverSpeed = 2f;

    private float _hoverStart_X;
    private float _hoverStart_Y;
    private float _currentAlarmTime = 0f;
    private bool _isReturning = false;

    protected override void Start()
    {
        base.Start();
        if (rb != null)
        {
            rb.gravityScale = 0f;   // 공중 적이므로 중력을 0으로 만듭니다.
        }
        
        _hoverStart_X = transform.position.x;
        _hoverStart_Y = transform.position.y;
    }

    protected override void UpdateIdle()
    {
        if (_isReturning)   // 플레이어가 감지범위 밖으로 벗어나면 생성위치로 복귀합니다.
        {
            Hovering();
            Vector2 backToFirstPosition = new Vector2(_hoverStart_X, _hoverStart_Y);
            transform.position = Vector2.MoveTowards(transform.position, backToFirstPosition, _flySpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, backToFirstPosition) < 0.001f)
            {
                _isReturning = false;
            }

            return;
        }

        Hovering();

        if (playerTransform == null) return;
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        // 플레이어가 감지범위 내로 들어오면 Alarm 상태로 전환합니다.
        if (distance <= _detectRange)
        {
            _currentAlarmTime = 0f;
            ChangeState(EnemyState.Alarm);
        }
    }

    protected override void UpdateAlarm()
    {
        Hovering();
        if (playerTransform == null) return;
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        LookAtPlayer();

        // 플레이어가 감지범위 밖으로 나가면 Idle 상태로 전환합니다. (생성 위치로 복귀) 
        if (distance > _detectRange)
        {
            _isReturning = true;
            ChangeState(EnemyState.Idle);
            return;
        }
        // 플레이어가 공격범위 안으로 들어오면 Attack 상태로 전환합니다.
        if (distance <= _attackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }
        
        _currentAlarmTime += Time.deltaTime;
        // 경고 시간이 끝나면 Trace 상태로 전환합니다.
        if (_currentAlarmTime >= _alarmTime)
        {
            ChangeState(EnemyState.Trace);
        }
    }

    protected override void UpdateTrace()
    {
        if (playerTransform == null) return;
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        
        if (distance > _detectRange)
        {
            _isReturning = true;
            ChangeState(EnemyState.Idle);
            return;
        }
        
        if (distance <= _attackRange)
        {
            ChangeState(EnemyState.Attack);
        }
        
        Hovering();
        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, _flySpeed * Time.deltaTime);
        LookAtPlayer();
    }
    
    protected override void UpdateAttack() { }
    protected override void UpdateKnockback() { }
    protected override void UpdateDead() { }

    private void Hovering()
    {
        float newY = _hoverStart_Y + Mathf.Sin(Time.time * _hoverSpeed) * _hoverHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    
    private void LookAtPlayer()
    {
        Vector3 scale = transform.localScale;
        if (playerTransform.position.x > transform.position.x)
        {
            scale.x = Mathf.Abs(scale.x);
        }
        else
        {
            scale.x = -Mathf.Abs(scale.x);
        }

        transform.localScale = scale;
    }
}
