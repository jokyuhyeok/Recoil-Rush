using UnityEngine;

// 기본 적 2 (기관총 사수)
// 이동속도, 감지범위, 공격범위, 경고 시간을 가집니다. (소총수와 동일)
public class EnemyMachineGun : EnemyBase
{
    [SerializeField] private EnemyBullet _bulletPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private GameObject _deathParticle;
    [SerializeField] private GameObject _muzzleFlashParticle;

    private float _currentAlarmTime = 0f;
    private float _fireTimer = 0f;
    private float _reloadTimer = 0f;
    private int _currentBulletRemain;
    private bool _isReloading = false;
    private bool _isDead = false;

    private bool _isAimLocked = false;
    private Vector2 _lockedAimedDirection;

    private MachineGunStatDataSO _machineGunStatDataSo => _baseStatData as MachineGunStatDataSO;

    private bool _isPlayerTooHigh => Mathf.Abs(playerTransform.position.y - transform.position.y) >
                                     _machineGunStatDataSo.ShootHeightDifference;

    protected override void Start()
    {
        base.Start();
        _currentBulletRemain = _machineGunStatDataSo.MagazineSize; // 초기에 바로 공격할 수 있게 장전을 해놓습니다.
    }

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
        if (distanceToPlayer <= _baseStatData.AttackRange && !_isPlayerTooHigh)
        {
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
        _isAimLocked = false;

        // 플레이어가 감지범위를 벗어나면 Idle 상태로 돌아갑니다.
        if (distanceToPlayer > _baseStatData.DetectRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        // 공격 범위 안으로 들어오면 Attack으로 전환합니다.
        if (distanceToPlayer <= _baseStatData.AttackRange && !_isPlayerTooHigh)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        if (_isPlayerTooHigh)
        {
            LookAtPlayer();
            return;
        }

        Vector2 targetPosition = new Vector2(playerTransform.position.x, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, targetPosition,
            _baseStatData.Speed * Time.deltaTime);
        LookAtPlayer();
    }

    protected override void UpdateAttack()
    {
        float outOfRangeBuffer = 2.0f;
        // 플레이어가 공격 범위를 벗어나면 Trace 상태로 전환합니다.
        if (distanceToPlayer > _baseStatData.AttackRange + outOfRangeBuffer || _isPlayerTooHigh)
        {
            _isAimLocked = false;
            ChangeState(EnemyState.Trace);
            return;
        }

        // 쏘기 전 한번만 Player에게 조준 고정합니다. 
        if (!_isAimLocked)
        {
            LookAtPlayer();
            _lockedAimedDirection = (playerTransform.position - _firePoint.position).normalized;
            _isAimLocked = true;
        }

        // 연사 후 재장전 로직입니다.
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
            Debug.Log("<color=yellow>기관총 장전 완료! 다시 쏜다!</color>");
            return;
        }

        _fireTimer += Time.deltaTime;

        // 총알 연사 로직입니다.
        // 쏠 시간이 안 됐으면 리턴합니다.
        if (_fireTimer < _machineGunStatDataSo.FireRate) return;

        // 실제 사격 실행
        _fireTimer = 0f;
        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        ShootBulletEvent();
    }

    private void ShootBulletEvent()
    {
        // 총알 즉시 생성 및 발사
        if (_bulletPrefab == null || _firePoint == null || playerTransform == null) return;

        EnemyBullet bullet = Instantiate(_bulletPrefab, _firePoint.position, Quaternion.identity);
        Vector2 direction = _lockedAimedDirection;

        // 2. 탄 퍼짐(Spread) 수학 로직 적용
        float halfSpread = _machineGunStatDataSo.SpreadAngle / 2f;
        float randomAngle = UnityEngine.Random.Range(-halfSpread, halfSpread);
        Vector2 spreadDirection = Quaternion.Euler(0, 0, randomAngle) * direction;

        bullet.FireBullet(spreadDirection);

        // 총구 반동입니다.
        rb.AddForce(-direction * _machineGunStatDataSo.ShootRecoilPower, ForceMode2D.Impulse);

        // 총구 화염 파티클 넣는 로직입니다. 
        if (_muzzleFlashParticle == null)
        {
            Debug.LogError("총구 화염 파티클 넣어주세요! 안 그러면 총알을 무한대로 쏘는 오류가 납니다.");
        }

        Instantiate(_muzzleFlashParticle, _firePoint.position, _firePoint.rotation);

        // -------------------------------------------------------------
        // 🎯 [Game Feel: 총을 발사할 때의 연출입니다.]
        // 1. Muzzle Flash (총구 섬광)
        // (추천 구현 방법: _firePoint.position에 총구 화염 이펙트를 띄우고 0.1초 뒤 삭제)
        //
        // 2. Gun SFX (총기 사운드)
        // (추천 구현 방법: 권총/소총 특유의 날카롭고 가벼운 발사음 재생)
        //

        _currentBulletRemain--;

        // 남은 총알이 0발일 때, 재장전에 들어갑니다.
        if (_currentBulletRemain <= 0)
        {
            _isReloading = true;
            _reloadTimer = 0f;
            Debug.Log("<color=orange>기관총 탄창 방전! 3초간 재장전 시작!</color>");
        }
    }

    protected override void UpdateKnockback()
    {
        // // 해당 적은 1대 맞으면 바로 사망 처리이므로 Knockback 구현은 없습니다.
    }

    protected override void UpdateDead()
    {
        if (_isDead) return;
        _isDead = true;

        if (_deathParticle == null)
        {
            Debug.LogError("기관총 사망 파티클 넣어주세요!");
        }

        Instantiate(_deathParticle, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        Destroy(gameObject);

        Debug.Log("<color=red>기관총 사수: 으아악! (사망)</color>");
    }
}