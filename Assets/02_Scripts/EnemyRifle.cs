using UnityEngine;

// 기본 적 1 (소총수)
// 이동속도, 감지범위, 공격범위, 경고 시간을 가집니다.
public class EnemyRifle : EnemyBase
{
    [SerializeField] private float _speed = 3f;
    [SerializeField] private float _detectRange = 8f;
    [SerializeField] private float _attackRange = 5f;
    [SerializeField] private float _alarmTime = 1f;

    private float _currentAlarmTime = 0f;
    
    protected override void UpdateIdle()
    {
        if (playerTransform == null) return;
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        
        // 플레이어가 감지범위 안으로 들어왔을 시, 경고 단계로 넘어갑니다.
        if (distance <= _detectRange)
        {
            _currentAlarmTime = 0f;         // 경고 시간 초기화
            ChangeState(EnemyState.Alarm);
        }
    }

    // 경고 단계: "!", "플레이어 바라보기" 등으로 플레이어를 감지함을 알려주는 상태입니다.
    protected override void UpdateAlarm()
    {
        if (playerTransform == null) return;
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        LookAtPlayer();
        
        // 플레이어가 감지범위를 벗어나면 Idle로 돌아갑니다. 
        if (distance > _detectRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        // 플레이어가 경고 기간 내에 공격 범위로 들어오면 즉시 Attack으로 진입합니다.
        if (distance <= _attackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }
        
        _currentAlarmTime += Time.deltaTime;
        
        // 경고 시간이 끝나면 Trace로 진입합니다.
        if (_currentAlarmTime >= _alarmTime)
        {
            ChangeState(EnemyState.Trace);
        }
    }
    
    protected override void UpdateTrace()
    {
        if (playerTransform == null) return;
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        // 플레이어가 감지범위를 벗어나면 Idle 상태로 돌아갑니다.
        if (distance > _detectRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }
        
        // 공격 범위 안으로 들어오면 Attack으로 전환합니다.
        if (distance <= _attackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, _speed * Time.deltaTime);
        LookAtPlayer();
    }
    
    protected override void UpdateAttack()
    {
        if (playerTransform != null)
        {
            LookAtPlayer();
        }
        
        Debug.Log("사거리 도달! 총알 장전 중...");
    }
    
    // x 좌표 비교를 통해 플레이어를 바라보도록 설정합니다. 
    private void LookAtPlayer()
    {
        if (playerTransform.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

 
}
