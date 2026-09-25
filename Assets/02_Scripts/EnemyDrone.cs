using UnityEngine;

// 공중 적 (드론)
public class EnemyDrone : EnemyBase
{
    // 제자리에서 둥둥 떠다니는 Hovering 연출을 위한 변수입니다.
    [SerializeField] private float _hoverHeight = 0.5f;
    [SerializeField] private float _hoverSpeed = 2f;

    [SerializeField] private EnemyBullet _cannonPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _chargeTime = 2f;
    [SerializeField] private float _attackCooldown = 2f;
    
    private float _hoverStart_X;
    private float _hoverStart_Y;
    private float _currentAlarmTime = 0f;
    private bool _isReturning = false;
    private float _attackTimer = 0f;
    private bool _isCharging = false;
    
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
            transform.position = Vector2.MoveTowards(transform.position, backToFirstPosition, _speed * Time.deltaTime);

            if (Vector2.Distance(transform.position, backToFirstPosition) < 0.001f)
            {
                _isReturning = false;
            }

            return;
        }

        Hovering();
        
        // 플레이어가 감지범위 내로 들어오면 Alarm 상태로 전환합니다.
        if (distanceToPlayer <= _detectRange)
        {
            _currentAlarmTime = 0f;
            ChangeState(EnemyState.Alarm);
        }
    }

    protected override void UpdateAlarm()
    {
        Hovering();
        LookAtPlayer();

        // 플레이어가 감지범위 밖으로 나가면 Idle 상태로 전환합니다. (생성 위치로 복귀) 
        if (distanceToPlayer > _detectRange)
        {
            _isReturning = true;
            ChangeState(EnemyState.Idle);
            return;
        }
        // 플레이어가 공격범위 안으로 들어오면 Attack 상태로 전환합니다.
        if (distanceToPlayer <= _attackRange)
        {
            StartCharging();
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
        if (distanceToPlayer > _detectRange)
        {
            _isReturning = true;
            ChangeState(EnemyState.Idle);
            return;
        }
        
        if (distanceToPlayer <= _attackRange)
        {
            StartCharging();
            ChangeState(EnemyState.Attack);
        }
        
        Hovering();
        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, _speed * Time.deltaTime);
        LookAtPlayer();
    }

    protected override void UpdateAttack()
    {
        LookAtPlayer();

        _attackTimer += Time.deltaTime;

        if (_isCharging)
        {
            if (_attackTimer > _chargeTime)
            {
                FireCannon();
                _isCharging = false;
                _attackTimer = 0f;
            }
        }
        else
        {
            if (_attackTimer >= _attackCooldown)
            {
                if (distanceToPlayer > _attackRange)
                {
                    ChangeState(EnemyState.Trace);
                    return;
                }
                
                StartCharging();
            }
        }
    }
    
    protected override void UpdateKnockback() { }
    protected override void UpdateDead() { }
    
    private void FireCannon()
    {
        Debug.Log("<color=red>드론: 쾅! (대포 발사)</color>");
        if (_cannonPrefab != null && _firePoint != null)
        {
            EnemyBullet bullet = Instantiate(_cannonPrefab, _firePoint.position, Quaternion.identity);
            Vector2 direction = (playerTransform.position - _firePoint.position).normalized;
            bullet.FireBullet(direction);
        }
    }
    
    private void StartCharging()
    {
        _attackTimer = 0f;
        _isCharging = true;
        // 여기에 에너지를 모으는 사운드나 파티클 이펙트를 실행합니다.
        Debug.Log("<color=cyan>드론: 위이잉... (대포 차징 시작)</color>");
    }
    
    private void Hovering()
    {
        float newY = _hoverStart_Y + Mathf.Sin(Time.time * _hoverSpeed) * _hoverHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
