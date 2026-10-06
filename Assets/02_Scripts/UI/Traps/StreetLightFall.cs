using System.Collections;
using UnityEngine;

public class StreetLightFall : MonoBehaviour, IResettable
{
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Collider2D _dangerCollider;

    // HingeJoint2D를 사용 중이면 연결
    [SerializeField] private HingeJoint2D _hinge;

    [Header("Fall")]
    [SerializeField] private float _dangerDelay = 0.15f;
    [SerializeField] private float _torquePower = 80f;
    [SerializeField] private bool _fallRight = true;

    private bool _triggered;

    private Vector3 _startPosition;
    private Quaternion _startRotation;

    private Coroutine _fallCoroutine;


    private void Awake()
    {
        // 처음 상태 저장
        _startPosition = transform.position;
        _startRotation = transform.rotation;

        _rb.bodyType = RigidbodyType2D.Kinematic;

        _dangerCollider.enabled = false;
        gameObject.tag = "Untagged";
    }


    public void StartFall()
    {
        if (_triggered)
            return;

        _triggered = true;

        _fallCoroutine = StartCoroutine(FallRoutine());
    }


    private IEnumerator FallRoutine()
    {
        // 넘어지기 시작
        _rb.bodyType = RigidbodyType2D.Dynamic;

        float direction = _fallRight ? -1f : 1f;

        _rb.AddTorque(
            direction * _torquePower,
            ForceMode2D.Impulse
        );

        yield return new WaitForSeconds(_dangerDelay);

        gameObject.tag = "Dangerous";
        _dangerCollider.enabled = true;
    }


    public void ResetTrap()
    {
        Debug.Log("가로등 ResetTrap 실행!");

        // 코루틴 제거
        if (_fallCoroutine != null)
        {
            StopCoroutine(_fallCoroutine);
            _fallCoroutine = null;
        }

        _triggered = false;

        // ★ Joint가 있으면 잠깐 OFF
        if (_hinge != null)
            _hinge.enabled = false;

        // ★ 먼저 Kinematic으로 바꾸기
        _rb.bodyType = RigidbodyType2D.Kinematic;

        // 속도 제거
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;

        // ★ Transform 자체를 처음 상태로 복구
        transform.SetPositionAndRotation(
            _startPosition,
            _startRotation
        );

        // Rigidbody에도 한 번 더 적용
        _rb.position = _startPosition;
        _rb.rotation = _startRotation.eulerAngles.z;

        // Dangerous 해제
        gameObject.tag = "Untagged";
        _dangerCollider.enabled = false;

        Physics2D.SyncTransforms();

        // Joint 다시 ON
        if (_hinge != null)
            _hinge.enabled = true;
    }
}