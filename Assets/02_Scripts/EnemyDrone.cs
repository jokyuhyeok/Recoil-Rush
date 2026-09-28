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

    protected override void UpdateKnockback()
    {
        // [구현 예정: 드론 넉백] [가이드]
        // 총의 종류에 따라서 넉백 정도를 다르게 줄 예정입니다. (총 damage가 확정되면 조정하면 될 것 같습니다. 일단 가이드...)
        // 1. receivedDamage 확인
        // 2-case.A: (권총, damage == n) 살짝 뒤로 밀렸다가 Trace 복귀
        // 2-case.B: (샷건, damage >= m) _attackTimer 초기화 및 _isCharging = false (차징 취소)
        // 2-case.B: 위로 튕기듯 크게 밀려났다가 Trace 복귀
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
    }
    
    private void FireCannon()
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
        float newY = _hoverStart_Y + Mathf.Sin(Time.time * _hoverSpeed) * _hoverHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
