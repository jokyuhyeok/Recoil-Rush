using UnityEngine;

// 공중 적 (드론)
public class EnemyDrone : EnemyBase
{
    [SerializeField] private Transform _firePoint;
    [SerializeField] private GameObject _chargeEffectObject; // 기 모으는 이펙트 (드론의 자식 객체)
    [SerializeField] private GameObject _fallingSparkObject; // 드론 추락할 때 기체에 연기나 불꽃

    private float _hoverStart_X;
    private float _hoverStart_Y;
    private float _currentAlarmTime = 0f;
    private bool _isReturning = false;
    private float _attackTimer = 0f;
    private bool _isCharging = false;

    // 물리 제어 자물쇠 변수
    private bool _isKnockbackProcessed = false;
    private float _knockbackTimer = 0f;
    private bool _isDeadProcessed = false;
    private float _deadTimer = 0f;

    private DroneStatDataSO _droneStatDataSo => _baseStatData as DroneStatDataSO;

    protected override void Start()
    {
        base.Start();

        if (rb == null) return;
        rb.gravityScale = 0f;

        _hoverStart_X = transform.position.x;
        _hoverStart_Y = transform.position.y;
    }

    protected override void UpdateIdle()
    {
        Hovering();

        if (_isReturning) // 플레이어가 감지범위 밖으로 벗어나면 생성위치로 복귀
        {
            float moveX = Mathf.MoveTowards(transform.position.x, _hoverStart_X, _baseStatData.Speed * Time.deltaTime);
            transform.position = new Vector2(moveX, transform.position.y);

            if (Mathf.Abs(transform.position.x - _hoverStart_X) < 0.001f)
            {
                _isReturning = false;
            }

            return;
        }

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

        if (distanceToPlayer > _baseStatData.DetectRange)
        {
            _isReturning = true;
            ChangeState(EnemyState.Idle);
            return;
        }

        _currentAlarmTime += Time.deltaTime;

        if (_currentAlarmTime >= _baseStatData.AlarmTime)
        {
            ChangeState(EnemyState.Trace);
        }
    }

    protected override void UpdateTrace()
    {
        Hovering();
        if (distanceToPlayer > _baseStatData.DetectRange)
        {
            _isReturning = true;
            ChangeState(EnemyState.Idle);
            return;
        }

        if (distanceToPlayer <= _baseStatData.AttackRange)
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

        if (distanceToPlayer > _baseStatData.AttackRange + outOfRangeBuffer)
        {
            CancelCharge();
            ChangeState(EnemyState.Trace);
            return;
        }

        LookAtPlayer();
        if (!CanShootTarget())
        {
            CancelCharge();
            return;
        }

        _attackTimer += Time.deltaTime;

        // 사격 쿨타임 대기
        if (!_isCharging && (_attackTimer < _droneStatDataSo.AttackCooldown)) return;

        if (!_isCharging)
        {
            StartCharging();
            return;
        }

        // 차징 중일 때의 로직
        if (_isCharging && (_attackTimer < _droneStatDataSo.ChargeTime)) return;

        if (anim == null)
        {
            Debug.LogError("Attack 애니메이션을 넣어주세요!");
            return;
        }

        anim.SetTrigger("Attack");
        _isCharging = false;
        _attackTimer = 0f;
    }

    protected override void UpdateKnockback()
    {
        if (!_isKnockbackProcessed)
        {
            _isKnockbackProcessed = true;
            _knockbackTimer = 0f;

            rb.linearVelocity = Vector2.zero;
            Vector2 amplifiedForce = receivedKnockbackForce * 4f;
            rb.AddForce(amplifiedForce, ForceMode2D.Impulse);
            rb.linearDamping = 10f; // 급브레이크

            CameraController.Instance.StartShake(0.3f, 0.1f);
        }

        _knockbackTimer += Time.deltaTime;

        if (_knockbackTimer < 0.3f) return;
        if (rb == null) return;

        rb.linearVelocity = Vector2.zero;
        rb.linearDamping = 0f; // 브레이크 해제
        _isKnockbackProcessed = false;

        _hoverStart_X = transform.position.x;
        _hoverStart_Y = transform.position.y;
        ChangeState(EnemyState.Trace);
    }

    protected override void UpdateDead()
    {
        // 밟을 수 있게 만들기
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

            if (_fallingSparkObject != null)
            {
                _fallingSparkObject.SetActive(true); // 드론 프리팹 안에 있는 오브젝트를 끌어다 놓으셔야 합니다.
            }
        }

        _deadTimer += Time.deltaTime;

        // 공중 폭발
        if (_deadTimer >= 0.3f)
        {
            CameraController.Instance.StartShake(1.5f, 0.4f);

            if (_droneStatDataSo.ExplosionPrefab == null)
            {
                Debug.LogError("드론 폭발 프리팹을 넣어주세요!");
                return;
            }

            Instantiate(_droneStatDataSo.ExplosionPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    private void ShootBulletEvent()
    {
        if (_chargeEffectObject != null)
        {
            _chargeEffectObject.SetActive(false);
        }

        if (_firePoint != null && _droneStatDataSo.CannonPrefab != null)
        {
            EnemyBullet bullet = Instantiate(_droneStatDataSo.CannonPrefab, _firePoint.position, Quaternion.identity);

            Vector2 targetPos = GetTargetPosition();
            Vector2 direction = (targetPos - (Vector2)_firePoint.position).normalized;
            bullet.FireBullet(direction);

            CameraController.Instance.StartPerlinShake(0.8f, 15f, 0.2f);
        }
    }

    private void StartCharging()
    {
        _attackTimer = 0f;
        _isCharging = true;

        if (_chargeEffectObject == null)
        {
            Debug.LogError("대포 차징 이펙트 넣어주세요.");
            return;
        }

        _chargeEffectObject.SetActive(true);
    }

    private void CancelCharge()
    {
        if (_isCharging)
        {
            _isCharging = false;
            _attackTimer = 0f;

            if (_chargeEffectObject == null)
            {
                Debug.LogError("대포 차징 이펙트 넣어주세요.");
                return;
            }

            _chargeEffectObject.SetActive(false);
            Debug.Log("<color=orange>드론 차징 취소</color>");
        }
    }

    private void Hovering()
    {
        float newY = _hoverStart_Y + Mathf.Sin(Time.time * _droneStatDataSo.HoverSpeed) * _droneStatDataSo.HoverHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private Vector2 GetTargetPosition()
    {
        if (playerTransform == null) return Vector2.zero;
        return playerTransform.position + Vector3.up * 0.5f;
    }

    private bool CanShootTarget()
    {
        Vector2 targetPos = GetTargetPosition();
        Vector2 direction = targetPos - (Vector2)_firePoint.position;
        float distance = Vector2.Distance(_firePoint.position, targetPos);

        if (distance < 1.5f) return true; // 초근접은 사격 가능
        float angle = Mathf.Atan2(direction.y, Mathf.Abs(direction.x)) * Mathf.Rad2Deg;

        if (angle < _droneStatDataSo.MinAttackAngle || angle > _droneStatDataSo.MaxAttackAngle)
        {
            return false;
        }

        // 장애물 관통 체크
        RaycastHit2D hit = Physics2D.Raycast(_firePoint.position, direction.normalized, distance,
            _droneStatDataSo.ObstacleMask);
        return hit.collider == null;
    }

    private float DecideXPosition()
    {
        float directionSign = Mathf.Sign(transform.position.x - playerTransform.position.x);
        float currentDistanceX = Mathf.Abs(transform.position.x - playerTransform.position.x);
        float distanceY = playerTransform.position.y - _firePoint.position.y;
        float requiredXMove = 0f;

        // -60도 각도를 맞추기 위한 최소 X거리 tan계산
        if (distanceY < 0)
        {
            float angleAbs = Mathf.Abs(_droneStatDataSo.MinAttackAngle);
            float angleRad = angleAbs * Mathf.Deg2Rad;
            float tangentValue = Mathf.Tan(angleRad);
            requiredXMove = Mathf.Abs(distanceY) / tangentValue;
        }

        // 플레이어가 바로 밑에 있는 경우 거리를 벌려서 각도 확보
        if (currentDistanceX < requiredXMove)
        {
            // 뒤로 물러나 거리 벌리기
            return playerTransform.position.x + (requiredXMove * directionSign);
        }
        else
        {
            // 평범하게 플레이어의 X좌표를 추적
            return playerTransform.position.x;
        }
    }
}