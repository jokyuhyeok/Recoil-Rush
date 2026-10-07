using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _recoilRadius = 2f;
    [SerializeField] private float _recoilPower = 3f;

    private Rigidbody2D _rigidbody;

    private Weapon _weapon;

    private int _damage;

    private Vector2 _previousPosition;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        _previousPosition = _rigidbody.position;
    }

    public void SetWeapon(Weapon weapon)
    {
        _weapon = weapon;

        // linearVelocity: Rigidbody2D 현재 이동 속도 및 방향
        // transform.right: 오브젝트 방향
        _rigidbody.linearVelocity = transform.right * _weapon.BulletSpeed;
        _damage = _weapon.Damage;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("PlayerBullet"))
        {
            return;
        }

        Vector2 currentPosition = _rigidbody.position;
        Vector2 move = currentPosition - _previousPosition;

        RaycastHit2D[] hits = Physics2D.RaycastAll(
            _previousPosition,
            move.normalized,
            move.magnitude
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null || hit.collider != other)
            {
                continue;
            }

            // 부딪힌 게 적이면 데미지를 입힌다.
            if (hit.collider.CompareTag("Enemy"))
            {
                // Todo: 이중 넉백 수정
                // 넉백은 임시로 사용
                hit.collider.GetComponent<IDamageable>().TakeDamage(_damage, move.normalized * _recoilPower);
            }

            AddImpactRecoil(hit.point);
        }

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