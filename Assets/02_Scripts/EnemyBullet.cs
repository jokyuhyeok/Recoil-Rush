using UnityEngine;

// 소총, 기관총, 드론에서 같이 사용할 Bullet 스크립트입니다.
// 인스펙터에서 크기와 속도, 데미지 등을 조정해서 사용하면 됩니다.
public class EnemyBullet : MonoBehaviour
{
    [SerializeField] private float _bulletSpeed = 10f;
    [SerializeField] private int _bulletDamage = 1;
    [SerializeField] private float _bulletLifeTime = 3f;

    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    // Enemy가 총알을 생성하고 날릴 때 호출하는 메서드입니다.
    public void FireBullet(Vector2 direction)
    {
        _rigidbody.linearVelocity = direction.normalized * _bulletSpeed;
        Destroy(gameObject, _bulletLifeTime); // 일정 시간 후 총알은 자동 파괴됩니다. (인스펙터에서 조정가능)
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 부딪힌 오브젝트가 맞을 수 있는 대상(IDamageable)인지 확인합니다.
        IDamageable damageable = collision.GetComponent<IDamageable>();

        if (damageable != null)
        {
            // Player는 넉백이 없으므로 총알 데미지만 넘겨주겠습니다.
            damageable.TakeDamage(_bulletDamage, Vector3.zero);
        }

        // -------------------------------------------------------------
        // 🎯 [Game Feel: 총알이 어딘가에 부딪혔을 때 나올 연출입니다.]
        // 1. Hit Flash & Hit Particle (총알이 어딘가에 충돌했을 때 나올 파티클입니다.)
        // (추천 구현 방법: transform.position 위치에 벽/바닥 충돌용 스파크 파티클을 Instantiate)
        //
        // 2. Hit SFX (총알이 부딪혔을 때의 사운드입니다.)
        // (추천 구현 방법: 챙! 하는 총알 튕기는 소리 재생. 플레이어가 맞았을 땐 데미지 처리 쪽에서 소리가 나므로 조건부 재생이 좋습니다)
        // -------------------------------------------------------------
        
        Destroy(gameObject);
    }
}
