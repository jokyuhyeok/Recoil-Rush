using UnityEngine;

// 기본 적 1 (소총수)
// 이동속도, 감지범위, 공격범위, 경고 시간을 가집니다.
public class EnemyRifle : EnemyBase
{
    // 총알 발사에 관련된 변수입니다.
    [SerializeField] private EnemyBullet _bulletPrefeb;
    [SerializeField] private Transform _firePoint;

    private float _currentAlarmTime = 0f;
    private float _fireTimer = 0f;

    private RifleStatDataSO _rifleStatData => _baseStatData as RifleStatDataSO;

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
            _fireTimer = _rifleStatData.fireRate; // 빠른 접근 시, 1회 즉시 사격합니다.
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

        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, _baseStatData.speed * Time.deltaTime);
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

        _fireTimer += Time.deltaTime;

        if (_fireTimer >= _rifleStatData.fireRate)
        {
            // 쿨타임 종료 시 총알을 생성 후 플레이어 방향으로 발사합니다.
            EnemyBullet bullet = Instantiate(_bulletPrefeb, _firePoint.position, Quaternion.identity);
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

            _fireTimer = 0f;
        }
    }

    protected override void UpdateKnockback()
    {
        // 해당 적은 1대 맞으면 바로 사망 처리이므로 Knockback 구현은 없습니다.
    }

    protected override void UpdateDead()
    {
        // [구현 예정 : 소총수 사망] [가이드]
        // player 무기에 따라 다른 로직을 적용할지는 미정입니다. (샷건이랑 권총에 맞은 리액션이 같으면 좀... 그렇긴 해서...)
        // 1. 콜라이더 판정 끄기 (길막 방지)
        // 2. receivedKnockback(총알 방향) 벡터에 Y축(위쪽) 힘을 섞어 포물선 궤도 생성
        // 3. 해당 벡터로 AddForce 적용 및 AddTorque로 회전
        // 4. 바닥에 떨어지고 n초 후 파괴 (Destroy)

        Destroy(gameObject);
    }
}