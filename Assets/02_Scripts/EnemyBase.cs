using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    // 적 상태를 6종류로 구분
    public enum EnemyState
    {
        Idle,
        Alarm,
        Trace,
        Attack,
        Knockback,
        Dead
    }

    [SerializeField] protected EnemyState currentState = EnemyState.Idle;
    [SerializeField] protected int hp = 1;  // 몬스터 체력 (일반 : 1, 공중 : 2, 보스 : N) (숫자는 변경 예정)
    
    protected Rigidbody2D rb;
    protected Transform playerTransform;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Start()
    {
        PlayerCaching();
    }

    protected virtual void Update()
    {
        // 현재 상태에 맞는 메서드를 실행합니다.
        switch (currentState)
        {
            case EnemyState.Idle:
                UpdateIdle();
                break;
            case EnemyState.Alarm:
                UpdateAlarm();
                break;
            case EnemyState.Trace:
                UpdateTrace();
                break;
            case EnemyState.Attack:
                UpdateAttack();
                break;
            case EnemyState.Knockback:
                UpdateKnockback();
                break;
            case EnemyState.Dead:
                UpdateDead();
                break;
        }
    }

    protected virtual void UpdateIdle(){}

    protected virtual void UpdateAlarm(){}

    protected virtual void UpdateTrace(){}

    protected virtual void UpdateAttack(){}

    protected virtual void UpdateKnockback(){}

    protected virtual void UpdateDead(){}

    // 나중에 public class EnemyBase : MonoBehaviour, IDamageable로 상속 연결할 예정입니다.
    // 총알에 맞으면 총알에 있는 두 매개변수를 받아서 hp에 반영합니다.
    public virtual void TakeDamage(int damage, Vector3 knockbackForce)
    {
        if (currentState == EnemyState.Dead) return;

        hp -= damage;
        Debug.Log($"{gameObject.name} 피격! 남은 HP: {hp}");
        
        // 중력은 Collision에서 GetContact로 조정할 예정입니다. 아래 3줄은 임시 물리적용.
        Vector2 safeForce = new Vector2(knockbackForce.x, knockbackForce.y);    // Z축 날아감(화면 밖으로 사라짐) 방지를 위한 Vector3 -> Vector2 변환
        rb.linearVelocity = Vector2.zero; 
        rb.AddForce(safeForce, ForceMode2D.Impulse);                            // 밀려나기 전에 기존에 걸어오던 관성을 0으로 초기화해서 정확한 거리만큼 밀려나게 합니다.

        if (hp <= 0)
        {
            ChangeState(EnemyState.Dead);
        }
        else
        {
            ChangeState(EnemyState.Knockback);
        }
    }
    
    private void PlayerCaching()
    {
        // 플레이어 transform 컴포넌트를 처음에 캐싱해놓고 계속 추적하게 했습니다. 
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("플레이어가 씬에 없습니다!");
        }
    }
    
    // 상태를 변경할 때 이 메서드를 거쳐서 안전하게 변경할게요.
    public void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        Debug.Log($"{gameObject.name} 상태 변경 -> {currentState}");
    }
}
