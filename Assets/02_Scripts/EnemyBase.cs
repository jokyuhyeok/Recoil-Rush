using UnityEngine;

public class EnemyBase : MonoBehaviour, IDamageable
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
    [SerializeField] protected int _hp = 1;  // 몬스터 체력 (일반 : 1, 공중 : 2, 보스 : N) (숫자는 변경 예정)
    
    // 자식들이 공통으로 사용하는 행동 패턴 관련 변수들입니다.
    [SerializeField] protected float _speed = 1f;
    [SerializeField] protected float _detectRange = 1f;
    [SerializeField] protected float _attackRange = 1f;
    [SerializeField] protected float _alarmTime = 1f;
    
    protected Rigidbody2D rb;
    protected Transform playerTransform;
    protected float distanceToPlayer;   // 플레이어와의 거리를 저장해 둘 공통 변수입니다.

    // 자식들의 피격 게임필 연출에 사용할 데미지와 벡터 힘 데이터를 보관하는 변수입니다.
    protected Vector2 receivedKnockbackForce;
    protected int receivedDamage;
    
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
        if (playerTransform == null) return;
        distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);  // 플레이어와의 거리를 계산합니다.
        
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

        _hp -= damage;
        Debug.Log($"{gameObject.name} 피격! 남은 HP: {_hp}");
        
        // -------------------------------------------------------------
        // 🎯 [Game Feel: 적이 맞았을 때의 연출 효과들입니다.]
        // 1. Hit Flash & Hit Particle (적이 맞았을 때 나올 섬광 및 파티클입니다.)
        // (추천 구현 방법: 적 스프라이트의 Material을 0.05초간 하얗게 변경(코루틴) 및 타격 지점에 불꽃/먼지 파티클 Instantiate)
        // 
        // 2. Hit SFX (피격 사운드입니다.)
        // (추천 구현 방법: AudioSource.PlayClipAtPoint()로 쇳소리나 둔탁한 살점 타격음 재생)
        // 
        // 3. Hit Stop (피격 시 잠시 멈춤)
        // (추천 구현 방법: Time.timeScale = 0.05f 등으로 게임을 아주 짧게 느리게 만들었다가 되돌리는 글로벌 코루틴 호출)
        //
        // 4. Camera Shake (카메라 진동)
        // (추천 구현 방법: 데미지가 3 이상(샷건 등)일 때만 Cinemachine Impulse Source를 호출해 화면을 강하게 떪)
        // -------------------------------------------------------------
        
        // 물리 적용을 위해 넘어온 데이터들을 캐싱합니다.
        receivedDamage = damage;
        receivedKnockbackForce = new Vector2(knockbackForce.x, knockbackForce.y);

        if (_hp <= 0)
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
    
    // 공통으로 사용하는 방향 전환 로직입니다. (x 좌표 비교를 통해 플레이어를 바라보도록 설정합니다.)
    protected virtual void LookAtPlayer()
    {
        Vector3 scale = transform.localScale;

        if (playerTransform.position.x < transform.position.x)
        {
            scale.x = Mathf.Abs(scale.x);
        }
        else
        {
            scale.x = -Mathf.Abs(scale.x);
        }

        transform.localScale = scale;
    }
}
