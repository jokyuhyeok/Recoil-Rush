using System.Collections;
using UnityEngine;

public class HiddenSpikeTrap : MonoBehaviour, IResettable
{
    [Header("Fake Tile")]
    [SerializeField] private Rigidbody2D _tileRb;
    [SerializeField] private Collider2D _tileCollider;

    [Header("Ground")]
    [SerializeField] private Collider2D _groundCollider;

    [Header("Spikes")]
    [SerializeField] private GameObject[] _spikes;

    [Header("Timing")]
    [SerializeField] private float _fallDelay = 0.1f;
    [SerializeField] private float _colliderOffDelay = 0.08f;

    [Header("Fall")]
    [SerializeField] private float _gravityScale = 10f;
    [SerializeField] private float _initialFallSpeed = 3f;

    private Vector2 _startPosition;
    private float _startRotation;

    private bool _triggered;
    private Coroutine _fallCoroutine;


    private void Awake()
    {
        _startPosition = _tileRb.position;
        _startRotation = _tileRb.rotation;

        ResetTrap();
    }


    public void ActivateTrap()
    {
        Debug.Log("★ HiddenSpikeTrap 발동!");

        if (_triggered)
            return;

        _triggered = true;

        _fallCoroutine = StartCoroutine(FallRoutine());
    }


    private IEnumerator FallRoutine()
    {
        // 1. 밟고 잠깐 버팀
        yield return new WaitForSeconds(_fallDelay);


        // 2. 가시 등장
        foreach (GameObject spike in _spikes)
        {
            if (spike != null)
            {
                spike.SetActive(true);
            }
        }


        // 3. FakeTile이 기존 Ground Collider에 걸리지 않도록 함
        if (_groundCollider != null && _tileCollider != null)
        {
            Physics2D.IgnoreCollision(
                _tileCollider,
                _groundCollider,
                true
            );
        }


        // 4. 타일 떨어뜨리기
        _tileRb.bodyType = RigidbodyType2D.Dynamic;
        _tileRb.gravityScale = _gravityScale;

        // 처음부터 아래쪽으로 툭
        _tileRb.linearVelocity =
            Vector2.down * _initialFallSpeed;


        // 5. 타일이 먼저 내려가는 모습을 조금 보여줌
        yield return new WaitForSeconds(_colliderOffDelay);


        // 6. 플레이어가 타일을 뚫고 떨어질 수 있도록
        if (_tileCollider != null)
        {
            _tileCollider.enabled = false;
        }


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


        // 물리 연산 잠깐 정지
        _tileRb.simulated = false;


        // 속도 제거
        _tileRb.linearVelocity = Vector2.zero;
        _tileRb.angularVelocity = 0f;


        // 원래 위치 / 회전으로
        _tileRb.position = _startPosition;
        _tileRb.rotation = _startRotation;


        // 처음 상태로
        _tileRb.bodyType = RigidbodyType2D.Kinematic;
        _tileRb.gravityScale = 0f;


        // 타일 Collider 복구
        if (_tileCollider != null)
        {
            _tileCollider.enabled = true;
        }


        // Ground와 충돌 다시 활성화
        if (_groundCollider != null && _tileCollider != null)
        {
            Physics2D.IgnoreCollision(
                _tileCollider,
                _groundCollider,
                false
            );
        }


        // 가시 다시 숨김
        foreach (GameObject spike in _spikes)
        {
            if (spike != null)
            {
                spike.SetActive(false);
            }
        }


        _tileRb.simulated = true;

        Physics2D.SyncTransforms();
    }
}