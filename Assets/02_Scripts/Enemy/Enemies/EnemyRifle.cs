using UnityEngine;

// 기본 적 1 (소총수)
public class EnemyRifle : EnemyBase
{
    [SerializeField] private EnemyBullet _bulletPrefeb;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private GameObject _deathParticle;
    [SerializeField] private GameObject _muzzleFlashParticle;

    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private float _maxShootAngle = 45f;

    private bool _isDead = false;
    private float _currentAlarmTime = 0f;
    private float _fireTimer = 0f;

    private RifleStatDataSO _rifleStatData => _baseStatData as RifleStatDataSO;

    private bool _isPlayerTooHigh => Mathf.Abs(playerTransform.position.y - transform.position.y) >
                                     _rifleStatData.ShootHeightDifference;

    protected override void UpdateIdle()
    {
        // 플레이어가 감지범위 안으로 들어왔을 시, 경고 단계로 넘어갑니다.
        if (distanceToPlayer <= _baseStatData.DetectRange)
        {
            _currentAlarmTime = 0f; // 경고 시간 초기화
            ChangeState(EnemyState.Alarm);
        }
    }

    // 경고 단계: "!", "플레이어 바라보기" 등으로 플레이어를 감지함을 알려주는 상태입니다.
    protected override void UpdateAlarm()
    {
        LookAtPlayer();

        // 플레이어가 감지범위를 벗어나면 Idle로 돌아갑니다. 
        if (distanceToPlayer > _baseStatData.DetectRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        // 플레이어가 경고 기간 내에 공격 범위로 들어오면 즉시 Attack으로 진입합니다.
        if (distanceToPlayer <= _baseStatData.AttackRange && CanShootTarget())
        {
            _fireTimer = _rifleStatData.FireRate; // 빠른 접근 시, 1회 즉시 사격합니다.
            ChangeState(EnemyState.Attack);
            return;
        }

        _currentAlarmTime += Time.deltaTime;

        // 경고 시간이 끝나면 Trace로 진입합니다.
        if (_currentAlarmTime >= _baseStatData.AlarmTime)
        {
            ChangeState(EnemyState.Trace);
        }
    }

    protected override void UpdateTrace()
    {
        // 플레이어가 감지범위를 벗어나면 Idle 상태로 돌아갑니다.
        if (distanceToPlayer > _baseStatData.DetectRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        // 공격 범위 안으로 들어오면 Attack으로 전환합니다.
        if (distanceToPlayer <= _baseStatData.AttackRange && CanShootTarget())
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        Vector2 targetPosition = new Vector2(playerTransform.position.x, transform.position.y);
        transform.position =
            Vector2.MoveTowards(transform.position, targetPosition, _baseStatData.Speed * Time.deltaTime);
        LookAtPlayer();
    }

    protected override void UpdateAttack()
    {
        // 플레이어가 공격 범위를 벗어나면 Trace 상태로 전환합니다.
        float outOfRangeBuffer = 2.0f;
        if (distanceToPlayer > _baseStatData.AttackRange + outOfRangeBuffer || !CanShootTarget())
        {
            ChangeState(EnemyState.Trace);
            return;
        }

        LookAtPlayer();

        _fireTimer += Time.deltaTime;

        if (_fireTimer >= _rifleStatData.FireRate)
        {
            anim.SetTrigger("Attack");
            _fireTimer = 0f;
        }
    }

    protected override void UpdateKnockback()
    {
        // 해당 적은 1대 맞으면 바로 사망 처리이므로 Knockback 구현은 없습니다.
    }

    protected override void UpdateDead()
    {
        if (_isDead) return;
        _isDead = true;

        if (_deathParticle == null) return;

        Instantiate(_deathParticle, transform.position + Vector3.up * 0.5f, Quaternion.identity);

        CameraController.Instance.StartShake(0.5f, 0.2f);

        Destroy(gameObject);
        Debug.Log("<color=red>소총수: 으아악! (사망)</color>");
    }

    // 애니메이션에서 호출될 메서드입니다. 
    public void ExecuteFire()
    {
        Vector2 targetPosition = GetTargetPosition();
        Vector2 direction = (targetPosition - (Vector2)_firePoint.position).normalized;

        // 총알 생성 및 발사
        EnemyBullet bullet = Instantiate(_bulletPrefeb, _firePoint.position, Quaternion.identity);
        bullet.FireBullet(direction);

        // 2. 반동 적용
        rb.AddForce(-direction * _rifleStatData.ShootRecoilPower, ForceMode2D.Impulse);

        // 3. 이펙트 생성
        if (_muzzleFlashParticle != null)
        {
            Instantiate(_muzzleFlashParticle, _firePoint.position, _firePoint.rotation);
        }

        CameraController.Instance.StartPerlinShake(0.2f, 10f, 0.1f);
    }

    // 플레이어 피벗이 발 밑에 있어서 명치를 겨냥하도록 위치 보정.
    private Vector2 GetTargetPosition()
    {
        if (playerTransform == null) return Vector2.zero;

        return playerTransform.position + Vector3.up * 0.5f;
    }

    // 사격 가능 각도인지 + 중간에 벽이 없는지 동시에 체크합니다.
    private bool CanShootTarget()
    {
        Vector2 targetPosition = GetTargetPosition();
        Vector2 direction = targetPosition - (Vector2)_firePoint.position;

        // 각도 체크
        float angle = Mathf.Atan2(direction.y, Mathf.Abs(direction.x)) * Mathf.Rad2Deg;
        if (angle < -_maxShootAngle || angle > _maxShootAngle)
        {
            return false; // 각도가 너무 가파르면 사격 불가
        }

        // 벽(장애물) 관통 체크
        float distance = Vector2.Distance(_firePoint.position, targetPosition);
        RaycastHit2D hit = Physics2D.Raycast(_firePoint.position, direction.normalized, distance, _obstacleMask);

        // hit.collider가 null이면 중간에 가로막는 벽이 없다는 뜻입니다.
        return hit.collider == null;
    }
}