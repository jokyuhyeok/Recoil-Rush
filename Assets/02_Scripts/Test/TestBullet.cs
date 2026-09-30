using UnityEngine;

public class TestBullet : MonoBehaviour
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
        if (collision.CompareTag("Player") == false)
            return;

        Destroy(gameObject);
    }
}