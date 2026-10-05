using UnityEngine;

// 기본 적 2 (기관총 사수)
public class EnemyMachineGun : EnemyBase
{
    [SerializeField] private Transform _firePoint;

    private float _currentAlarmTime = 0f;
    private float _fireTimer = 0f;
    private float _reloadTimer = 0f;
    private int _currentBulletRemain;
    private bool _isReloading = false;
    private bool _isDead = false;

    private bool _isAimLocked = false;
    private Vector2 _lockedAimedDirection;

    private MachineGunStatDataSO _machineGunStatDataSo => _baseStatData as MachineGunStatDataSO;

    protected override void Start()
    {
        base.Start();
        _currentBulletRemain = _machineGunStatDataSo.MagazineSize; // 초기에 바로 공격할 수 있게 장전
    }

    protected override void UpdateIdle()
    {
        if (distanceToPlayer <= _baseStatData.DetectRange)
        {
            _currentAlarmTime = 0f;
            ChangeState(EnemyState.Alarm);
        }
    }

    // 경고 단계: "!", "플레이어 바라보기" 등으로 플레이어를 감지함을 알려주는 상태
    protected override void UpdateAlarm()
    {
        LookAtPlayer();

        // 플레이어가 감지범위를 벗어나면 Idle로 전환 
        if (distanceToPlayer > _baseStatData.DetectRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        // 플레이어가 경고 기간 내에 공격 범위로 들어오면 즉시 Attack으로 진입
        if (distanceToPlayer <= _baseStatData.AttackRange && CanShootTarget())
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        _currentAlarmTime += Time.deltaTime;

        // 경고 시간이 끝나면 Trace로 진입
        if (_currentAlarmTime >= _baseStatData.AlarmTime)
        {
            ChangeState(EnemyState.Trace);
        }
    }

    protected override void UpdateTrace()
    {
        _isAimLocked = false;

        if (distanceToPlayer > _baseStatData.DetectRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        if (distanceToPlayer <= _baseStatData.AttackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        Vector2 targetPosition = new Vector2(playerTransform.position.x, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, targetPosition,
            _baseStatData.Speed * Time.deltaTime);
        LookAtPlayer();
    }

    protected override void UpdateAttack()
    {
        // 플레이어가 공격 범위를 벗어나면 Trace 상태로 전환
        float outOfRangeBuffer = 2.0f;
        if (distanceToPlayer > _baseStatData.AttackRange + outOfRangeBuffer)
        {
            _isAimLocked = false;
            ChangeState(EnemyState.Trace);
            return;
        }

        // 벽에 막혔을 땐 조준만 풀고 대기
        if (!CanShootTarget())
        {
            _isAimLocked = false;
            return;
        }

        // 쏘기 전 한 번만 Player에게 조준 고정
        if (!_isAimLocked)
        {
            LookAtPlayer();
            Vector2 targetPosition = GetTargetPosition();
            _lockedAimedDirection = (targetPosition - (Vector2)_firePoint.position).normalized;
            _isAimLocked = true;
        }

        // 재장전 로직
        if (_isReloading)
        {
            LookAtPlayer();
            _reloadTimer += Time.deltaTime;

            if (_reloadTimer < _machineGunStatDataSo.ReloadTime) return;

            // 장전 완료
            _isReloading = false;
            _currentBulletRemain = _machineGunStatDataSo.MagazineSize;
            _fireTimer = _machineGunStatDataSo.FireRate;
            _isAimLocked = false;
            return;
        }

        // 연사 로직
        _fireTimer += Time.deltaTime;

        if (_fireTimer < _machineGunStatDataSo.FireRate) return;

        _fireTimer = 0f;
        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        ShootBulletEvent();
    }

    protected override void UpdateKnockback()
    {
        // // 해당 적은 1대 맞으면 바로 사망 처리이므로 Knockback 구현은 없습니다.
    }

    protected override void UpdateDead()
    {
        if (_isDead) return;
        _isDead = true;

        if (_machineGunStatDataSo.DeathParticle == null)
        {
            Debug.LogError("기관총 사망 파티클 넣어주세요!");
        }

        Instantiate(_machineGunStatDataSo.DeathParticle, transform.position + Vector3.up * 0.5f, Quaternion.identity);

        CameraController.Instance.StartShake(0.5f, 0.2f);
        Destroy(gameObject);
    }

    // 애니메이션 이벤트 호출 메서드
    private void ShootBulletEvent()
    {
        if (_machineGunStatDataSo.BulletPrefab == null || _firePoint == null || playerTransform == null) return;

        EnemyBullet bullet = Instantiate(_machineGunStatDataSo.BulletPrefab, _firePoint.position, Quaternion.identity);
        Vector2 direction = _lockedAimedDirection;

        // 탄 퍼짐 계산
        float halfSpread = _machineGunStatDataSo.SpreadAngle / 2f;
        float randomAngle = UnityEngine.Random.Range(-halfSpread, halfSpread);
        Vector2 spreadDirection = Quaternion.Euler(0, 0, randomAngle) * direction;

        bullet.FireBullet(spreadDirection);

        // 총구 반동
        rb.AddForce(-direction * _machineGunStatDataSo.ShootRecoilPower, ForceMode2D.Impulse);

        // 총구 화염
        if (_machineGunStatDataSo.MuzzleFlashParticle == null)
        {
            Debug.LogError("총구 화염 파티클 넣어주세요! 안 그러면 총알을 무한대로 쏘는 오류가 납니다.");
        }

        Instantiate(_machineGunStatDataSo.MuzzleFlashParticle, _firePoint.position, _firePoint.rotation);

        CameraController.Instance.StartPerlinShake(0.30f, 18f, 0.05f);

        _currentBulletRemain--;

        // 재장전
        if (_currentBulletRemain <= 0)
        {
            _isReloading = true;
            _reloadTimer = 0f;
            Debug.Log("<color=orange>기관총 탄창 방전.. 재장전 시작!</color>");
        }
    }

    private Vector2 GetTargetPosition()
    {
        if (playerTransform == null) return Vector2.zero;
        return playerTransform.position + Vector3.up * 0.8f;
    }

    private bool CanShootTarget()
    {
        Vector2 targetPos = GetTargetPosition();
        Vector2 direction = targetPos - (Vector2)_firePoint.position;
        float distance = Vector2.Distance(_firePoint.position, targetPos);

        // 각도 제한 체크
        if (distance < 1.5f) return true;

        float angle = Mathf.Atan2(direction.y, Mathf.Abs(direction.x)) * Mathf.Rad2Deg;
        if (angle < -_machineGunStatDataSo.MaxShootAngle || angle > _machineGunStatDataSo.MaxShootAngle)
        {
            return false;
        }

        // 장애물 체크
        RaycastHit2D hit = Physics2D.Raycast(_firePoint.position, direction.normalized, distance,
            _machineGunStatDataSo.ObstacleMask);
        return hit.collider == null;
    }
}