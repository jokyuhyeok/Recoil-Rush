using UnityEngine;

// 소총, 기관총, 드론에서 같이 사용할 Bullet 스크립트입니다.
// 인스펙터에서 크기와 속도, 데미지 등을 조정해서 사용하면 됩니다.
public class EnemyBullet : MonoBehaviour
{
    [SerializeField] private float _bulletSpeed = 10f;
    [SerializeField] private float _bulletLifeTime = 3f;
    [SerializeField] private GameObject _impactParticle;

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
        if (collision.isTrigger) return;

        if (collision.CompareTag("Player"))
        {
            Destroy(gameObject);
            return;
        }

        if (_impactParticle == null)
        {
            Debug.LogError("적 총알이 부서질 때의 파티클을 넣어주세요!");
            return;
        }

        Vector2 contactPoint = collision.ClosestPoint(transform.position);
        Vector2 bulletDirection = _rigidbody.linearVelocity.normalized;

        RaycastHit2D hit =
            Physics2D.Raycast(transform.position - (Vector3)bulletDirection * 0.5f, bulletDirection, 1.5f);
        Quaternion impactRotation = Quaternion.identity;

        if (hit.collider != null)
        {
            // 벽의 바깥 방향(normal)으로 파티클 꺾기
            impactRotation = Quaternion.FromToRotation(Vector2.up, hit.normal);
        }
        else
        {
            // 예외: 튕겨나온 반대 방향으로 꺾기
            impactRotation = Quaternion.FromToRotation(Vector2.up, -bulletDirection);
        }

        Instantiate(_impactParticle, contactPoint, impactRotation);

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