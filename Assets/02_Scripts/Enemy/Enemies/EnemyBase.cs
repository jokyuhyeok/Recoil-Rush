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
    [SerializeField] protected EnemyStatDataSO _baseStatData;
    protected Animator anim;

    protected Rigidbody2D rb;
    protected Transform playerTransform;
    protected float distanceToPlayer; // 플레이어와의 거리를 저장해 둘 공통 변수입니다.

    // 자식들의 피격 게임필 연출에 사용할 데미지와 벡터 힘 데이터를 보관하는 변수입니다.
    protected Vector2 receivedKnockbackForce;
    protected int receivedDamage;
    protected int _currentHP;

    // 게임 상태를 관리하는 게임 매니저 변수
    protected GameManager _gameManager;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        PlayerCaching();
        GameManagerCaching();

        if (_baseStatData != null)
        {
            _currentHP = _baseStatData.MaxHp;
        }
    }

    protected virtual void Update()
    {
        if (playerTransform == null || !playerTransform.gameObject.activeInHierarchy || _gameManager.IsCutscene)
        {
            // 플레이어가 사망 등으로 오브젝트가 없을 때, 대기상태로 전환합니다. 
            if (currentState != EnemyState.Idle && currentState != EnemyState.Dead)
            {
                ChangeState(EnemyState.Idle);
            }

            return;
        }

        distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position); // 플레이어와의 거리를 계산합니다.

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

    protected virtual void UpdateIdle()
    {
    }

    protected virtual void UpdateAlarm()
    {
    }

    protected virtual void UpdateTrace()
    {
    }

    protected virtual void UpdateAttack()
    {
    }

    protected virtual void UpdateKnockback()
    {
    }

    protected virtual void UpdateDead()
    {
    }

    // 총알에 맞으면 총알에 있는 두 매개변수를 받아서 hp에 반영합니다.
    public virtual void TakeDamage(int damage, Vector3 knockbackForce)
    {
        if (currentState == EnemyState.Dead) return;

        _currentHP -= damage;
        Debug.Log($"{gameObject.name} 피격! 남은 HP: {_currentHP}");

        // 물리 적용을 위해 넘어온 데이터들을 캐싱합니다.
        receivedDamage = damage;
        receivedKnockbackForce = new Vector2(knockbackForce.x, knockbackForce.y);

        if (_currentHP <= 0)
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

    private void GameManagerCaching()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("게임 매니저가 씬에 없습니다!");
            return;
        }

        _gameManager = GameManager.Instance;
    }

    // 상태를 변경할 때 이 메서드를 거쳐서 안전하게 변경할게요.
    public void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;

        if (currentState == EnemyState.Trace)
        {
            anim.SetBool("isMove", false);
        }

        currentState = newState;
        Debug.Log($"{gameObject.name} 상태 변경 -> {currentState}");

        switch (currentState)
        {
            case EnemyState.Trace:
                anim.SetBool("isMove", true);
                break;
            case EnemyState.Dead:
                anim.SetTrigger("Die");
                break;
        }
    }

    protected virtual void OnEnable()
    {
        if (_baseStatData != null)
        {
            _currentHP = _baseStatData.MaxHp;
        }

        currentState = EnemyState.Idle;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
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