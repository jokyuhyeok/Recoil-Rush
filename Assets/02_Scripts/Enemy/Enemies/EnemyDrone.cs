using UnityEngine;

// 공중 적 (드론)
public class EnemyDrone : EnemyBase
{
    [SerializeField] private EnemyBullet _cannonPrefab;
    [SerializeField] private Transform _firePoint;

    private float _hoverStart_X;
    private float _hoverStart_Y;
    private float _currentAlarmTime = 0f;
    private bool _isReturning = false;
    private float _attackTimer = 0f;
    private bool _isCharging = false;

    // 매 프레임 실행되는 Update 안에서 물리력을 1번만 주도록 제어하는 자물쇠 변수들입니다.
    private bool _isKnockbackProcessed = false;
    private float _knockbackTimer = 0f;
    private bool _isDeadProcessed = false;

    private DroneStatDataSO _droneStatDataSo => _baseStatData as DroneStatDataSO;

    protected override void Start()
    {
        base.Start();
        if (rb != null)
        {
            rb.gravityScale = 0f; // 공중 적이므로 중력을 0으로 만듭니다.
        }

        _hoverStart_X = transform.position.x;
        _hoverStart_Y = transform.position.y;
    }

    protected override void UpdateIdle()
    {
        if (_isReturning) // 플레이어가 감지범위 밖으로 벗어나면 생성위치로 복귀합니다.
        {
            Hovering();
            Vector2 backToFirstPosition = new Vector2(_hoverStart_X, _hoverStart_Y);
            transform.position = Vector2.MoveTowards(transform.position, backToFirstPosition,
                _baseStatData.speed * Time.deltaTime);

            if (Vector2.Distance(transform.position, backToFirstPosition) < 0.001f)
            {
                _isReturning = false;
            }

            return;
        }

        Hovering();

        // 플레이어가 감지범위 내로 들어오면 Alarm 상태로 전환합니다.
        if (distanceToPlayer <= _baseStatData.detectRange)
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
        if (distanceToPlayer > _baseStatData.detectRange)
        {
            _isReturning = true;
            ChangeState(EnemyState.Idle);
            return;
        }

        // 플레이어가 공격범위 안으로 들어오면 Attack 상태로 전환합니다.
        if (distanceToPlayer <= _baseStatData.attackRange)
        {
            StartCharging();
            ChangeState(EnemyState.Attack);
            return;
        }

        _currentAlarmTime += Time.deltaTime;
        // 경고 시간이 끝나면 Trace 상태로 전환합니다.
        if (_currentAlarmTime >= _baseStatData.alarmTime)
        {
            ChangeState(EnemyState.Trace);
        }
    }


    protected override void UpdateTrace()
    {
        if (distanceToPlayer > _baseStatData.detectRange)
        {
            _isReturning = true;
            ChangeState(EnemyState.Idle);
            return;
        }

        if (distanceToPlayer <= _baseStatData.attackRange)
        {
            StartCharging();
            ChangeState(EnemyState.Attack);
        }

        Hovering();
        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position,
            _baseStatData.speed * Time.deltaTime);
        LookAtPlayer();
    }

    protected override void UpdateAttack()
    {
        LookAtPlayer();

        _attackTimer += Time.deltaTime;

        if (_isCharging)
        {
            if (_attackTimer > _droneStatDataSo.chargeTime)
            {
                if (anim != null)
                {
                    anim.SetTrigger("Attack");
                }

                _isCharging = false;
                _attackTimer = 0f;
            }
        }
        else
        {
            if (_attackTimer >= _droneStatDataSo.attackCooldown)
            {
                if (distanceToPlayer > _baseStatData.attackRange)
                {
                    ChangeState(EnemyState.Trace);
                    return;
                }

                StartCharging();
            }
        }
    }

    protected override void UpdateKnockback()
    {
        // [구현 예정: 드론 넉백] [가이드]
        // 총의 종류에 따라서 넉백 정도를 다르게 줄 예정입니다. (총 damage가 확정되면 조정하면 될 것 같습니다. 일단 가이드...)
        // 1. receivedDamage 확인
        // 2-case.A: (권총, damage == n) 살짝 뒤로 밀렸다가 Trace 복귀
        // 2-case.B: (샷건, damage >= m) _attackTimer 초기화 및 _isCharging = false (차징 취소)
        // 2-case.B: 위로 튕기듯 크게 밀려났다가 Trace 복귀

        // 1. 상태 진입 첫 프레임에 딱 한 번만 뒤로 밀어냅니다.
        if (!_isKnockbackProcessed)
        {
            // 플레이어 반대 방향(뒤) 계산
            Vector2 pushDir = (transform.position - playerTransform.position).normalized;

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero; // 기존 속도 초기화
                rb.AddForce(pushDir * 5f, ForceMode2D.Impulse); // 뒤로 휙! 밀려남
            }

            _isKnockbackProcessed = true;
            _knockbackTimer = 0f;
        }

        _knockbackTimer += Time.deltaTime;

        // 2. 0.2초(짧은 시간) 뒤에 넉백을 끝내고 다시 추적 상태로 돌아갑니다.
        if (_knockbackTimer >= 0.2f)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero; // 밀리던 것 멈춤
            _isKnockbackProcessed = false; // 다음 피격을 위해 자물쇠 초기화
            ChangeState(EnemyState.Trace);
        }
    }

    protected override void UpdateDead()
    {
        // [구현 예정: 드론 추락 사망] [가이드]
        // 사망할 때, player가 밟고 이동할 수 있게 사망시 지형으로 바꾸겠습니다.
        // 1. gameObject.layer = LayerMask.NameToLayer("Platform"); (지형으로 변경)
        // 2. 피격 판정 콜라이더 비활성화
        // 3. rb.gravityScale = 0.5f; (추락 시작)
        // 4. transform.Rotate 로 빙글빙글 회전 연출
        // 5. 폭발 파티클 재생 후 3초 뒤 Destroy

        // -------------------------------------------------------------
        // 🎯 [Game Feel: 드론의 폭발 및 추락 연출입니다.]
        // 1. Impact Flash (인상 섬광)
        // (추천 구현 방법: 기계가 파괴되는 순간 화면 전체가 짧고 굵게 하얗게 번쩍이게 처리)
        //
        // 2. 추락 파티클 및 사운드
        // (추천 구현 방법: 빙글빙글 추락하는 동안 검은 연기와 스파크 파티클 지속 재생, 고장난 기계음 루프)
        // -------------------------------------------------------------

        // 상태 진입 첫 프레임에 딱 한 번만 애니메이션과 추락 물리력을 줍니다.
        if (!_isDeadProcessed)
        {
            // 1. 사망 애니메이션 트리거
            if (anim != null) anim.SetTrigger("Die");

            // 2. 콜라이더 끄기 (시체 길막 방지)
            Collider2D coll = GetComponent<Collider2D>();
            if (coll != null) coll.enabled = false;

            // 3. 중력 켜기 및 추락 연출
            if (rb != null)
            {
                rb.gravityScale = 2f;

                // 맞은 방향(receivedKnockbackForce)으로 살짝 튕기며 떨어지게
                Vector2 pushForce = new Vector2(receivedKnockbackForce.x, 0.5f).normalized * 5f;
                rb.AddForce(pushForce, ForceMode2D.Impulse);

                // 고장나서 빙글빙글 돌기
                rb.AddTorque(720f);
            }

            // 4. 2초 뒤 삭제
            Destroy(gameObject, 2f);
            Debug.Log("<color=red>드론: 끼기긱... (빙글빙글 추락)</color>");

            _isDeadProcessed = true; // 이후 이 블록은 다시 실행되지 않음
        }
    }

    private void ShootBulletEvent()
    {
        Debug.Log("<color=red>드론: 쾅! (대포 발사)</color>");
        if (_cannonPrefab != null && _firePoint != null)
        {
            EnemyBullet bullet = Instantiate(_cannonPrefab, _firePoint.position, Quaternion.identity);
            Vector2 direction = (playerTransform.position - _firePoint.position).normalized;
            bullet.FireBullet(direction);

            // -------------------------------------------------------------
            // 🎯 [Game Feel: 거대 대포 발사 연출 삽입 위치]
            // 1. Camera Shake (카메라 진동)[cite: 3]
            // (추천 구현 방법: 대포의 폭발감을 플레이어가 직접 느끼도록 화면 전체에 강한 진동 부여)
            //
            // 2. Muzzle Flash & Gun SFX[cite: 3, 4]
            // (추천 구현 방법: 소총보다 훨씬 큰 링 모양의 섬광과 묵직한 대포 사운드 재생)
            // -------------------------------------------------------------
        }
    }

    private void StartCharging()
    {
        _attackTimer = 0f;
        _isCharging = true;
        // 🎯 [Game Feel: 차징 연출 삽입 위치] (추천: 빛이 모이는 파티클과 위잉~ 사운드)
        Debug.Log("<color=cyan>드론: 위이잉... (대포 차징 시작)</color>");
    }

    private void Hovering()
    {
        float newY = _hoverStart_Y + Mathf.Sin(Time.time * _droneStatDataSo.hoverSpeed) * _droneStatDataSo.hoverHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}