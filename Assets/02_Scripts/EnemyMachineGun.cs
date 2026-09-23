using UnityEngine;

// 기본 적 2 (기관총 사수)
// 이동속도, 감지범위, 공격범위, 경고 시간을 가집니다. (소총수와 동일)
public class EnemyMachineGun : EnemyBase
{
    [SerializeField] private float _speed = 1.5f;
    [SerializeField] private float _detectRange = 12f;
    [SerializeField] private float _attackRange = 10f;
    [SerializeField] private float _alarmTime = 0.3f;

    [SerializeField] private EnemyBullet _bulletPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _fireRate = 2f;

    [SerializeField] private int _magazineSize = 8;
    [SerializeField] private float _reloadTime = 2f;
    
    private float _currentAlarmTime = 0f;
    private float _fireTimer = 0f;
    private float _reloadTimer = 0f;
    private int _currentBulletRemain;
    private bool _isReloading = false;

    protected override void Start()
    {
        base.Start();
        _currentBulletRemain = _magazineSize;   // 초기에 바로 공격할 수 있게 장전을 해놓습니다.
    }
    
    protected override void UpdateIdle()
    {
        if (playerTransform == null) return;
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        
        // 플레이어가 감지범위 안으로 들어왔을 시, 경고 단계로 넘어갑니다.
        if (distance <= _detectRange)
        {
            _currentAlarmTime = 0f;         // 경고 시간 초기화
            ChangeState(EnemyState.Alarm);
        }
    }

    // 경고 단계: "!", "플레이어 바라보기" 등으로 플레이어를 감지함을 알려주는 상태입니다.
    protected override void UpdateAlarm()
    {
        if (playerTransform == null) return;
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        LookAtPlayer();
        
        // 플레이어가 감지범위를 벗어나면 Idle로 돌아갑니다. 
        if (distance > _detectRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        // 플레이어가 경고 기간 내에 공격 범위로 들어오면 즉시 Attack으로 진입합니다.
        if (distance <= _attackRange)
        {
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
        if (playerTransform == null) return;
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        // 플레이어가 감지범위를 벗어나면 Idle 상태로 돌아갑니다.
        if (distance > _detectRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }
        
        // 공격 범위 안으로 들어오면 Attack으로 전환합니다.
        if (distance <= _attackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, _speed * Time.deltaTime);
        LookAtPlayer();
    }
    
    protected override void UpdateAttack()
    {
        if (playerTransform == null) return;
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        // 플레이어가 공격 범위를 벗어나면 Trace 상태로 전환합니다.
        if (distance > _attackRange)
        {
            ChangeState(EnemyState.Trace);
            return;
        }
        
        LookAtPlayer();

        // 연사 후 재장전 로직입니다.
        if (_isReloading)
        {
            _reloadTimer +=  Time.deltaTime;

            if (_reloadTimer >= _reloadTime)
            {
                _isReloading = false;
                _currentBulletRemain = _magazineSize;
                _fireTimer = _fireRate;
                Debug.Log("<color=yellow>기관총 장전 완료! 다시 쏜다!</color>");
            }
            return;
        }
        
        _fireTimer += Time.deltaTime;
        
        // 총알 연사 로직입니다.
        if (_fireTimer >= _fireRate)
        {
            EnemyBullet bullet = Instantiate(_bulletPrefab, _firePoint.position, Quaternion.identity);
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            bullet.FireBullet(direction);

            _currentBulletRemain--;
            _fireTimer = 0f;

            // 남은 총알이 0발일 때, 재장전에 들어갑니다.
            if (_currentBulletRemain <= 0)
            {
                _isReloading = true;
                _reloadTimer = 0f;
                Debug.Log("<color=orange>기관총 탄창 방전! 3초간 재장전 시작!</color>");
            }
        }
    }
    
    // x 좌표 비교를 통해 플레이어를 바라보도록 설정합니다. 
    private void LookAtPlayer()
    {
        Vector3 scale = transform.localScale;

        if (playerTransform.position.x < transform.position.x)
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
