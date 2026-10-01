using UnityEngine;

// 기본 적 2 (기관총 사수)
// 이동속도, 감지범위, 공격범위, 경고 시간을 가집니다. (소총수와 동일)
public class EnemyMachineGun : EnemyBase
{
    [SerializeField] private EnemyBullet _bulletPrefab;
    [SerializeField] private Transform _firePoint;

    private float _currentAlarmTime = 0f;
    private float _fireTimer = 0f;
    private float _reloadTimer = 0f;
    private int _currentBulletRemain;
    private bool _isReloading = false;

    private MachineGunStatDataSO _machineGunStatDataSo => _baseStatData as MachineGunStatDataSO;

    protected override void Start()
    {
        base.Start();
        _currentBulletRemain = _machineGunStatDataSo.magazineSize; // 초기에 바로 공격할 수 있게 장전을 해놓습니다.
    }

    protected override void UpdateIdle()
    {
        // 플레이어가 감지범위 안으로 들어왔을 시, 경고 단계로 넘어갑니다.
        if (distanceToPlayer <= _baseStatData.detectRange)
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
        if (distanceToPlayer > _baseStatData.detectRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        // 플레이어가 경고 기간 내에 공격 범위로 들어오면 즉시 Attack으로 진입합니다.
        if (distanceToPlayer <= _baseStatData.attackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        _currentAlarmTime += Time.deltaTime;

        // 경고 시간이 끝나면 Trace로 진입합니다.
        if (_currentAlarmTime >= _baseStatData.alarmTime)
        {
            ChangeState(EnemyState.Trace);
        }
    }

    protected override void UpdateTrace()
    {
        // 플레이어가 감지범위를 벗어나면 Idle 상태로 돌아갑니다.
        if (distanceToPlayer > _baseStatData.detectRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        // 공격 범위 안으로 들어오면 Attack으로 전환합니다.
        if (distanceToPlayer <= _baseStatData.attackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position,
            _baseStatData.speed * Time.deltaTime);
        LookAtPlayer();
    }

    protected override void UpdateAttack()
    {
        // 플레이어가 공격 범위를 벗어나면 Trace 상태로 전환합니다.
        if (distanceToPlayer > _baseStatData.attackRange)
        {
            ChangeState(EnemyState.Trace);
            return;
        }

        LookAtPlayer();

        // 연사 후 재장전 로직입니다.
        if (_isReloading)
        {
            _reloadTimer += Time.deltaTime;

            if (_reloadTimer >= _machineGunStatDataSo.reloadTime)
            {
                _isReloading = false;
                _currentBulletRemain = _machineGunStatDataSo.magazineSize;
                _fireTimer = _machineGunStatDataSo.fireRate;
                Debug.Log("<color=yellow>기관총 장전 완료! 다시 쏜다!</color>");
            }

            return;
        }

        _fireTimer += Time.deltaTime;

        // 총알 연사 로직입니다.
        if (_fireTimer >= _machineGunStatDataSo.fireRate)
        {
            // 사격 애니메이션 실행
            if (anim != null)
            {
                anim.SetTrigger("Attack");
                ShootBulletEvent();
                _fireTimer = 0f;
            }
        }
    }

    private void ShootBulletEvent()
    {
        // 총알 즉시 생성 및 발사
        if (_bulletPrefab == null || _firePoint == null || playerTransform == null) return;

        EnemyBullet bullet = Instantiate(_bulletPrefab, _firePoint.position, Quaternion.identity);
        Vector2 direction = (playerTransform.position - _firePoint.position).normalized;
        bullet.FireBullet(direction);

        // -------------------------------------------------------------
        // 🎯 [Game Feel: 총을 발사할 때의 연출입니다.]
        // 1. Muzzle Flash (총구 섬광)
        // (추천 구현 방법: _firePoint.position에 총구 화염 이펙트를 띄우고 0.1초 뒤 삭제)
        //
        // 2. Gun SFX (총기 사운드)
        // (추천 구현 방법: 권총/소총 특유의 날카롭고 가벼운 발사음 재생)
        //
        // 3. Gun Kick Animation (총구 반동 애니메이션)
        // (추천 구현 방법: 적 스프라이트 전체를 발사 반대 방향으로 아주 짧게 DOShakePosition 하거나 탄피 파티클 배출)
        // -------------------------------------------------------------

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
        // [구현 예정 : 기관총 사수 사망] [가이드]
        // player 무기에 따라 다른 로직을 적용할지는 미정입니다. (샷건이랑 권총에 맞은 리액션이 같으면 좀... 그렇긴 해서...)
        // 1. 콜라이더 판정 끄기 (길막 방지)
        // 2. receivedKnockback(총알 방향) 벡터에 Y축(위쪽) 힘을 섞어 포물선 궤도 생성
        // 3. 해당 벡터로 AddForce 적용 및 AddTorque로 회전
        // 4. 바닥에 떨어지고 n초 후 파괴 (Destroy)

        Destroy(gameObject);
        Debug.Log("<color=red>기관총 사수: 으아악! (사망)</color>");
    }
}