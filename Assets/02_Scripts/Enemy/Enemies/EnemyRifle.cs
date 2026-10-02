using UnityEngine;

// 기본 적 1 (소총수)
// 이동속도, 감지범위, 공격범위, 경고 시간을 가집니다.
public class EnemyRifle : EnemyBase
{
    // 총알 발사에 관련된 변수입니다.
    [SerializeField] private EnemyBullet _bulletPrefeb;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private GameObject _deathParticle;
    [SerializeField] private GameObject _muzzleFlashParticle;
    [SerializeField] private float _shootRecoilPower = 3f;

    private bool _isDead = false;
    private float _currentAlarmTime = 0f;
    private float _fireTimer = 0f;

    private RifleStatDataSO _rifleStatData => _baseStatData as RifleStatDataSO;

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
        if (distanceToPlayer <= _baseStatData.AttackRange)
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
        if (distanceToPlayer <= _baseStatData.AttackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position,
            _baseStatData.Speed * Time.deltaTime);
        LookAtPlayer();
    }

    protected override void UpdateAttack()
    {
        // 플레이어가 공격 범위를 벗어나면 Trace 상태로 전환합니다.
        if (distanceToPlayer > _baseStatData.AttackRange)
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

        GameObject effect = Instantiate(_deathParticle, transform.position, Quaternion.identity);

        Rigidbody2D effectRb = effect.GetComponent<Rigidbody2D>();

        if (effectRb == null) return;

        effectRb.AddForce(receivedKnockbackForce, ForceMode2D.Impulse);

        Destroy(gameObject);
        Debug.Log("<color=red>소총수: 으아악! (사망)</color>");
    }

    // 애니메이션에서 호출될 메서드입니다. 
    public void ExecuteFire()
    {
        // 1. 총알 생성 및 발사
        EnemyBullet bullet = Instantiate(_bulletPrefeb, _firePoint.position, Quaternion.identity);
        Vector2 direction = (playerTransform.position - _firePoint.position).normalized;
        bullet.FireBullet(direction);

        // 2. 반동 적용
        rb.AddForce(-direction * _shootRecoilPower, ForceMode2D.Impulse);

        // 3. 이펙트 생성 (위치 수정 완료)
        if (_muzzleFlashParticle != null)
        {
            // 🚨 하체에서 나오던 연기/화염을 반드시 _firePoint.position에서 나오도록 고정!
            Instantiate(_muzzleFlashParticle, _firePoint.position, _firePoint.rotation);
        }
    }
}