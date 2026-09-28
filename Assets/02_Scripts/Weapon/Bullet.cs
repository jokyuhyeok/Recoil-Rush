using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody2D _rigidbody;
    private Weapon _weapon;

    private int _damage;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    public void SetWeapon(Weapon weapon)
    {
        _weapon = weapon;
        // linearVelocity: Rigidbody2D 현재 이동 속도 및 방향
        // transform.right: 오브젝트 방향
        _rigidbody.linearVelocity = transform.right * _weapon.BulletSpeed;
        _damage = _weapon.Damage;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(gameObject);
    }
}