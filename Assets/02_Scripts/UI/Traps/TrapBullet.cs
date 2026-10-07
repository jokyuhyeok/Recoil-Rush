using UnityEngine;

public class TrapBullet : MonoBehaviour
{
    [SerializeField] private Transform _visual;

    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void Fire(Vector2 direction, float speed)
    {
        direction.Normalize();

        _rb.linearVelocity = direction * speed;

        float angle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 루트가 아니라 가로등 비주얼 + HitBox만 회전
        _visual.rotation =
            Quaternion.Euler(0f, 0f, angle);
    }
}