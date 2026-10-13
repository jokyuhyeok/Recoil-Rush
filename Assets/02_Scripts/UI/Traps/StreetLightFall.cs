using System.Collections;
using UnityEngine;

public class StreetLightFall : MonoBehaviour, IResettable
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D _rb;

    // Player 타격 전용 Trigger Collider
    [SerializeField] private Collider2D _hitCollider;

    [SerializeField] private HingeJoint2D _hinge;


    [Header("Fall")]
    [SerializeField] private float _hitDelay = 0.03f;
    [SerializeField] private float _torquePower = 80f;
    [SerializeField] private bool _fallRight = true;


    private bool _triggered;

    private Vector3 _startPosition;
    private Quaternion _startRotation;

    private Coroutine _fallCoroutine;


    private void Awake()
    {
        _startPosition = transform.position;
        _startRotation = transform.rotation;

        _rb.bodyType = RigidbodyType2D.Kinematic;

        // 처음에는 공격 판정 OFF
        if (_hitCollider != null)
        {
            _hitCollider.enabled = false;
        }
    }


    public void StartFall()
    {
        if (_triggered)
            return;

        _triggered = true;

        _fallCoroutine =
            StartCoroutine(FallRoutine());
    }


    private IEnumerator FallRoutine()
    {
        // 가로등 넘어지기 시작
        _rb.bodyType = RigidbodyType2D.Dynamic;

        float direction =
            _fallRight ? -1f : 1f;

        _rb.AddTorque(
            direction * _torquePower,
            ForceMode2D.Impulse
        );


        // 살짝 움직인 다음 공격 판정 활성화
        yield return new WaitForSeconds(
            _hitDelay
        );

        if (_hitCollider != null)
        {
            _hitCollider.enabled = true;
        }

        _fallCoroutine = null;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (GameManager.Instance.IsCutscene)
            return;

        PlayerKnockbackFall player =
            collision.GetComponentInParent<PlayerKnockbackFall>();

        if (player == null)
            return;


        // 플레이어 위치에서 가로등이 실제로 움직이는 방향
        Vector2 hitDirection =
            _rb.GetPointVelocity(
                collision.bounds.center
            );


        // 속도가 너무 작게 계산됐을 경우
        if (hitDirection.sqrMagnitude < 0.01f)
        {
            hitDirection =
                _fallRight
                    ? Vector2.right
                    : Vector2.left;
        }


        // 한 번 때리면 공격판정 OFF
        if (_hitCollider != null)
        {
            _hitCollider.enabled = false;
        }


        player.Launch(
            hitDirection.normalized
        );
    }


    public void ResetTrap()
    {
        if (_fallCoroutine != null)
        {
            StopCoroutine(_fallCoroutine);
            _fallCoroutine = null;
        }

        _triggered = false;


        // 공격판정 다시 OFF
        if (_hitCollider != null)
        {
            _hitCollider.enabled = false;
        }


        // Joint 잠깐 끄기
        if (_hinge != null)
        {
            _hinge.enabled = false;
        }


        // 물리 정지
        _rb.bodyType = RigidbodyType2D.Kinematic;

        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;


        // 처음 위치/각도로 복구
        transform.SetPositionAndRotation(
            _startPosition,
            _startRotation
        );

        _rb.position = _startPosition;
        _rb.rotation =
            _startRotation.eulerAngles.z;


        Physics2D.SyncTransforms();


        // Joint 다시 ON
        if (_hinge != null)
        {
            _hinge.enabled = true;
        }
    }
}