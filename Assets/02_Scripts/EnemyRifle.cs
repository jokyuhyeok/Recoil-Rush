using UnityEngine;

// 기본 적 1 (소총수)
// 이동속도, 감지범위, 공격범위, 경고 시간을 가집니다.
public class EnemyRifle : EnemyBase
{
    // 총알 발사에 관련된 변수입니다.
    [SerializeField] private EnemyBullet _bulletPrefeb;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _fireRate = 2f;
    
    private float _currentAlarmTime = 0f;
    private float _fireTimer = 0f;
    
    protected override void UpdateIdle()
    {
        // 플레이어가 감지범위 안으로 들어왔을 시, 경고 단계로 넘어갑니다.
        if (distanceToPlayer <= _detectRange)
        {
            _currentAlarmTime = 0f;         // 경고 시간 초기화
            ChangeState(EnemyState.Alarm);
        }
    }

    // 경고 단계: "!", "플레이어 바라보기" 등으로 플레이어를 감지함을 알려주는 상태입니다.
    protected override void UpdateAlarm()
    {
        LookAtPlayer();
        
        // 플레이어가 감지범위를 벗어나면 Idle로 돌아갑니다. 
        if (distanceToPlayer > _detectRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        // 플레이어가 경고 기간 내에 공격 범위로 들어오면 즉시 Attack으로 진입합니다.
        if (distanceToPlayer <= _attackRange)
        {
            _fireTimer = _fireRate;     // 빠른 접근 시, 1회 즉시 사격합니다.
            ChangeState(EnemyState.Attack);
            return;
        }
        
        _currentAlarmTime += Time.deltaTime;
        
        // 경고 시간이 끝나면 Trace로 진입합니다.
        if (_currentAlarmTime >= _alarmTime)
        {
            ChangeState(EnemyState.Trace);
        }
    }
    
    protected override void UpdateTrace()
    {
        // 플레이어가 감지범위를 벗어나면 Idle 상태로 돌아갑니다.
        if (distanceToPlayer > _detectRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }
        
        // 공격 범위 안으로 들어오면 Attack으로 전환합니다.
        if (distanceToPlayer <= _attackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, _speed * Time.deltaTime);
        LookAtPlayer();
    }
    
    protected override void UpdateAttack()
    {
        // 플레이어가 공격 범위를 벗어나면 Trace 상태로 전환합니다.
        if (distanceToPlayer > _attackRange)
        {
            ChangeState(EnemyState.Trace);
            return;
        }
        
        LookAtPlayer();

        _fireTimer += Time.deltaTime;
        
        if (_fireTimer >= _fireRate)
        {
            // 쿨타임 종료 시 총알을 생성 후 플레이어 방향으로 발사합니다.
            EnemyBullet bullet = Instantiate(_bulletPrefeb, _firePoint.position, Quaternion.identity);
            Vector2 direction = (playerTransform.position - _firePoint.position).normalized;
            bullet.FireBullet(direction);

            _fireTimer = 0f;
        }
    }
}
