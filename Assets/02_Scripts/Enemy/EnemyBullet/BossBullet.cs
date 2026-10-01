using UnityEngine;

// 보스 전용 총알 스크립트입니다.
public class BossBullet : MonoBehaviour
{
    [SerializeField] private int _bulletDamage = 1;
    [SerializeField] private float _bulletLifeTime = 3f;

    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void FireBullet(Vector2 direction, float speed)
    {
        _rb.linearVelocity = direction.normalized * speed;

        // 총알이 날아가는 방향과 바라보는 방향을 일치시킵니다. 
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        Destroy(gameObject, _bulletLifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어나 부서지는 장애물 등, 데미지를 받을 수 있는 대상인지 확인합니다.
        IDamageable damageable = collision.GetComponent<IDamageable>();

        if (damageable != null)
        {
            // 플레이어에게 데미지를 줍니다!
            damageable.TakeDamage(_bulletDamage, Vector3.zero);
        }

        // 총알이 어딘가에(벽이든 플레이어든) 부딪히면 자신을 파괴합니다.
        Destroy(gameObject);
    }
}
