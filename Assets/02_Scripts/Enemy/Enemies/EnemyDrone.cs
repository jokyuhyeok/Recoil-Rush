using UnityEngine;

// 공중 적 (드론)
public class EnemyDrone : EnemyBase
{
    [SerializeField] private EnemyBullet _cannonPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private GameObject _fallingSparkObject;
    [SerializeField] private GameObject _explosionPrefab;

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

    private float _deadTimer = 0f; // 3초 카운트다운 타이머
    private float _deadStartX = 0f; // 추락 시 좌우로 휘청거릴 기준 X축

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
        Hovering();

        if (_isReturning) // 플레이어가 감지범위 밖으로 벗어나면 생성위치로 복귀합니다.
        {
            float moveX = Mathf.MoveTowards(transform.position.x, _hoverStart_X, _baseStatData.Speed * Time.deltaTime);
            transform.position = new Vector2(moveX, transform.position.y);

            if (Mathf.Abs(transform.position.x - _hoverStart_X) < 0.001f)
            {
                _isReturning = false;
            }

            return;
        }

        // 플레이어가 감지범위 내로 들어오면 Alarm 상태로 전환합니다.
        if (distanceToPlayer <= _baseStatData.DetectRange)
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
        if (distanceToPlayer > _baseStatData.DetectRange)
        {
            _isReturning = true;
            ChangeState(EnemyState.Idle);
            return;
        }

        _currentAlarmTime += Time.deltaTime;
        // 경고 시간이 끝나면 Trace 상태로 전환합니다.
        if (_currentAlarmTime >= _baseStatData.AlarmTime)
        {
            ChangeState(EnemyState.Trace);
        }
    }


    protected override void UpdateTrace()
    {
        float angleBuffer = 5.0f;

        Hovering();
        if (distanceToPlayer > _baseStatData.DetectRange)
        {
            _isReturning = true;
            ChangeState(EnemyState.Idle);
            return;
        }

        if (distanceToPlayer <= _baseStatData.AttackRange && IsInShootingAngle(angleBuffer))
        {
            StartCharging();
            ChangeState(EnemyState.Attack);
            return;
        }

        float decidedPositionX = DecideXPosition();
        float newX = Mathf.MoveTowards(transform.position.x, decidedPositionX, _baseStatData.Speed * Time.deltaTime);
        transform.position = new Vector2(newX, transform.position.y);

        LookAtPlayer();
    }

    protected override void UpdateAttack()
    {
        Hovering();
        float outOfRangeBuffer = 3.0f;
        float angleBuffer = 5.0f;

        if (distanceToPlayer > _baseStatData.AttackRange + outOfRangeBuffer || !IsInShootingAngle(angleBuffer))
        {
            CancelCharge();
            ChangeState(EnemyState.Trace);
            return;
        }

        LookAtPlayer();
        _attackTimer += Time.deltaTime;

        // 차징 중일 때의 로직입니다.
        if (_isCharging)
        {
            if (_attackTimer < _droneStatDataSo.ChargeTime) return;

            if (anim == null)
            {
                Debug.LogError("Attack 애니메이션을 넣어주세요!");
                return;
            }

            anim.SetTrigger("Attack");
            _isCharging = false;
            _attackTimer = 0f;
            return;
        }

        // 사격 후 쿨타임 동안의 로직입니다.
        if (_attackTimer < _droneStatDataSo.AttackCooldown) return;
        StartCharging();
    }

    protected override void UpdateKnockback()
    {
        if (!_isKnockbackProcessed)
        {
            _isKnockbackProcessed = true;
            _knockbackTimer = 0f;

            Debug.Log(
                $"<color=yellow>[넉백 시작]</color> 받은 물리력: {receivedKnockbackForce}, 크기: {receivedKnockbackForce.magnitude}");
            if (rb == null)
            {
                Debug.LogError("<color=red>[오류]</color> 드론에 Rigidbody2D가 없습니다!");
            }

            rb.linearVelocity = Vector2.zero;
            Vector2 amplifiedForce = receivedKnockbackForce * 4f;
            rb.AddForce(amplifiedForce, ForceMode2D.Impulse);
            rb.linearDamping = 10f; // 급브레이크
            Debug.Log($"<color=orange>[물리 적용]</color> 현재 드론 속도: {rb.linearVelocity}");
            // (클래스 맨 위 변수 선언하는 곳에 피격 이펙트 변수 하나 추가해주세요)
            // [SerializeField] private GameObject _hitSparkVFX;
            // 여기에 피격 파티클을 넣습니다.
            // if (_hitSparkVFX != null) _hitSparkVFX.SetActive(true);
            CameraController.Instance.StartShake(0.3f, 0.1f);
        }

        _knockbackTimer += Time.deltaTime;
        if (_knockbackTimer < 0.3f) return;
        if (rb == null) return;

        rb.linearVelocity = Vector2.zero;
        rb.linearDamping = 0f; // 브레이크 해제
        _isKnockbackProcessed = false;
        Debug.Log("<color=green>[넉백 종료]</color> 스턴 0.3초 끝. Trace로 복귀합니다.");
        _hoverStart_X = transform.position.x;
        _hoverStart_Y = transform.position.y;
        ChangeState(EnemyState.Trace);
    }

    protected override void UpdateDead()
    {
        // 사망 진입 첫 프레임 (발판으로 변신합니다.)
        if (!_isDeadProcessed)
        {
            _isDeadProcessed = true;
            _deadTimer = 0f;

            if (anim != null) anim.SetTrigger("Die");

            if (rb == null) return;

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 2f;
            rb.linearDamping = 0.5f;
            rb.linearVelocity = Vector2.zero;
            rb.freezeRotation = false;

            Vector2 amplifiedForce = receivedKnockbackForce * 1f;
            rb.AddForce(amplifiedForce, ForceMode2D.Impulse);

            float randomTorque = Random.Range(-4f, 4f);
            rb.AddTorque(randomTorque, ForceMode2D.Impulse);
            gameObject.layer = LayerMask.NameToLayer("Default");

            Debug.Log("<color=red>[드론 사망]</color> 넉백 힘으로 날아갑니다!");

            if (_fallingSparkObject != null)
            {
                _fallingSparkObject.SetActive(true); // 드론 프리팹 안에 있는 오브젝트를 끌어다 놓으셔야 합니다.
            }
        }

        _deadTimer += Time.deltaTime;

        // 1초 ~ 1.5초 정도가 지난 후 '공중 폭발' 합니다.
        if (_deadTimer >= 0.3f)
        {
            Debug.Log("<color=red>[쾅!!]</color> 공중 대폭발!");
            // (나중에 여기에 폭발 파티클과 카메라 흔들림 함수를 넣습니다.)
            CameraController.Instance.StartShake(1.5f, 0.4f);
            if (_explosionPrefab == null)
            {
                Debug.Log("드론 폭발 프리팹을 넣어주세요!");
            }

            Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
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

            CameraController.Instance.StartPerlinShake(0.5f, 15f, 0.2f);
            //
            // 2. Muzzle Flash & Gun SFX[cite: 3, 4]
            // (추천 구현 방법: 소총보다 훨씬 큰 링 모양의 섬광과 묵직한 대포 사운드 재생)
            // -------------------------------------------------------------
        }
    }

    // Player가 드론의 사격 가능 각도 안에 있는지 확인하는 메서드입니다.
    private bool IsInShootingAngle(float angleBuffer = 0f)
    {
        if (_firePoint == null || playerTransform == null) return false;

        Vector2 direction = playerTransform.position - _firePoint.position;
        float distanceX = Mathf.Abs(direction.x);
        float distanceY = direction.y;

        float angle = Mathf.Atan2(distanceY, distanceX) * Mathf.Rad2Deg;
        return angle >= (_droneStatDataSo.MinAttackAngle - angleBuffer) &&
               angle <= (_droneStatDataSo.MaxAttackAngle + angleBuffer);
    }

    private float DecideXPosition()
    {
        float directionSign = Mathf.Sign(transform.position.x - playerTransform.position.x);
        float currentDistanceX = Mathf.Abs(transform.position.x - playerTransform.position.x);
        float distanceY = playerTransform.position.y - _firePoint.position.y;

        float requiredXMove = 0f;

        // 플레이어가 드론보다 아래에 있을 경우 (-60도 각도를 맞추기 위한 최소 X거리 tan계산)
        if (distanceY < 0)
        {
            float angleAbs = Mathf.Abs(_droneStatDataSo.MinAttackAngle);
            float angleRad = angleAbs * Mathf.Deg2Rad;
            float tangentValue = Mathf.Tan(angleRad);
            requiredXMove = Mathf.Abs(distanceY) / tangentValue;
        }

        // 현재 거리가 필요한 각도 확보 거리보다 가깝다면? (플레이어가 바로 밑으로 파고들었다면!)
        if (currentDistanceX < requiredXMove)
        {
            // 뒤로 물러나서(거리 벌리기) 최적의 사격 각도를 확보하는 목표 X좌표 반환
            return playerTransform.position.x + (requiredXMove * directionSign);
        }
        else
        {
            // 아직 각도가 넉넉하거나 멀리 있다면, 평범하게 플레이어의 X좌표를 향해 추적
            return playerTransform.position.x;
        }
    }

    private void StartCharging()
    {
        _attackTimer = 0f;
        _isCharging = true;
        // 🎯 [Game Feel: 차징 연출 삽입 위치] (추천: 빛이 모이는 파티클과 위잉~ 사운드)
        Debug.Log("<color=cyan>드론: 위이잉... (대포 차징 시작)</color>");
    }

    private void CancelCharge()
    {
        if (_isCharging)
        {
            _isCharging = false;
            _attackTimer = 0f;
            Debug.Log("<color=grey>드론: 앗 너무 멀어졌다! (차징 취소)</color>");
        }
    }

    private void Hovering()
    {
        float newY = _hoverStart_Y + Mathf.Sin(Time.time * _droneStatDataSo.HoverSpeed) * _droneStatDataSo.HoverHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}