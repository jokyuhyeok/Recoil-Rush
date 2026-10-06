using System.Collections;
using UnityEngine;

public class FallingSpikeTrap : MonoBehaviour, IResettable
{
    [Header("Spike")]
    [SerializeField] private Rigidbody2D _spikeRb;
    [SerializeField] private Collider2D _spikeCollider;

    [Header("Timing")]
    [SerializeField] private float _fallDelay = 0.1f;

    [Header("Fall")]
    [SerializeField] private float _gravityScale = 10f;
    [SerializeField] private float _initialFallSpeed = 3f;

    private Vector2 _startPosition;
    private float _startRotation;

    private bool _triggered;
    private Coroutine _fallCoroutine;


    private void Awake()
    {
        _startPosition = _spikeRb.position;
        _startRotation = _spikeRb.rotation;

        ResetTrap();
    }


    public void ActivateTrap()
    {
        if (_triggered)
            return;

        _triggered = true;

        _fallCoroutine = StartCoroutine(FallRoutine());
    }


    private IEnumerator FallRoutine()
    {
        // 잠깐 대기
        yield return new WaitForSeconds(_fallDelay);

        // 떨어지기 시작
        _spikeRb.bodyType = RigidbodyType2D.Dynamic;
        _spikeRb.gravityScale = _gravityScale;

        _spikeRb.linearVelocity =
            Vector2.down * _initialFallSpeed;

        _fallCoroutine = null;
    }


    public void ResetTrap()
    {
        if (_fallCoroutine != null)
        {
            StopCoroutine(_fallCoroutine);
            _fallCoroutine = null;
        }

        _triggered = false;

        // 물리 잠시 정지
        _spikeRb.simulated = false;

        // 속도 제거
        _spikeRb.linearVelocity = Vector2.zero;
        _spikeRb.angularVelocity = 0f;

        // 원래 위치
        _spikeRb.position = _startPosition;
        _spikeRb.rotation = _startRotation;

        // 다시 고정
        _spikeRb.bodyType = RigidbodyType2D.Kinematic;
        _spikeRb.gravityScale = 0f;

        // ★ 중요: 천장/바닥과 물리 충돌하지 않음
        if (_spikeCollider != null)
        {
            _spikeCollider.enabled = true;
            _spikeCollider.isTrigger = true;
        }

        _spikeRb.simulated = true;

        Physics2D.SyncTransforms();
    }
}