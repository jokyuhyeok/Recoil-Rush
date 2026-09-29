using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _recoilRadius = 2f;
    [SerializeField] private float _recoilPower = 3f;

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // GetContact(0): 첫 번째 충돌 지점
        // .point: 그 지점의 월드 좌표
        Vector2 impactPoint = collision.GetContact(0).point;

        AddImpactRecoil(impactPoint);

        Destroy(gameObject);
    }

    private void AddImpactRecoil(Vector2 impactPoint)
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(impactPoint, _recoilRadius);

        foreach (Collider2D target in targets)
        {
            Rigidbody2D targetRigidbody = target.attachedRigidbody;

            if (targetRigidbody == null)
            {
                continue;
            }

            if (!targetRigidbody.CompareTag("Player"))
            {
                continue;
            }

            Vector2 offset = targetRigidbody.worldCenterOfMass - impactPoint;

            float distance = offset.magnitude;

            if (distance <= 0f)
            {
                continue;
            }

            float falloff = 1f - Mathf.Clamp01(distance / _recoilRadius);

            Vector2 recoil = offset.normalized * (_recoilPower * falloff);

            Player player = targetRigidbody.GetComponent<Player>();

            if (player != null)
            {
                player.AddRecoil(recoil);
            }
        }
    }
}