using UnityEngine;

// 소총, 기관총에서 같이 사용할 Bullet 스크립트입니다.
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

        Destroy(gameObject);
    }
}
