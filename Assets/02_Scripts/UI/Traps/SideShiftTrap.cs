using System.Collections;
using UnityEngine;

public class SideShiftTrap : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform _startPoint;
    [SerializeField] private Transform _endPoint;
    [SerializeField] private float _speed = 25f;
    [SerializeField] private float _startDelay = 0.1f;

    [Header("Hit")]
    [SerializeField] private float _hitForce = 25f;
    [SerializeField] private float _upForce = 5f;

    private Rigidbody2D _rb;
    private Collider2D _collider;
    private SpriteRenderer _spriteRenderer;

    private Vector2 _moveDirection;

    // ★ 처음 위치 저장
    private Vector2 _startPosition;
    private Vector2 _endPosition;

    private bool _activated;
    private bool _moving;
    private bool _hitPlayer;

    private Coroutine _moveCoroutine;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // ★ 처음 월드 좌표를 저장
        _startPosition = _startPoint.position;
        _endPosition = _endPoint.position;

        ResetTrap();
    }


    // =========================================
    // ★ RespawnManager 연결
    // =========================================
    private IEnumerator Start()
    {
        while (RespawnManager.Instance == null)
            yield return null;

        RespawnManager.Instance.OnRespawnPlayerEvent += OnRespawnPlayer;
    }

    private void OnDestroy()
    {
        if (RespawnManager.Instance != null)
            RespawnManager.Instance.OnRespawnPlayerEvent -= OnRespawnPlayer;
    }

    private void OnRespawnPlayer(Vector3 respawnPosition)
    {
        ResetTrap();
    }


    private void FixedUpdate()
    {
        if (!_moving)
            return;

        Vector2 nextPosition =
            _rb.position +
            _moveDirection * _speed * Time.fixedDeltaTime;

        _rb.MovePosition(nextPosition);


        // 끝 지점 통과
        float distanceToEnd =
            Vector2.Dot(
                _endPosition - _rb.position,
                _moveDirection
            );

        if (distanceToEnd <= 0f)
        {
            FinishTrap();
        }
    }


    // 기존 TrapTrigger에서 호출
    public void ActivateTrap()
    {
        if (_activated)
            return;

        _activated = true;

        _moveCoroutine = StartCoroutine(StartMove());
    }


    private IEnumerator StartMove()
    {
        yield return new WaitForSeconds(_startDelay);

        // ★ 저장해놓은 시작 위치 사용
        _rb.position = _startPosition;

        _moveDirection =
            (_endPosition - _startPosition).normalized;

        _hitPlayer = false;

        _spriteRenderer.enabled = true;
        _collider.enabled = true;

        _moving = true;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_moving || _hitPlayer)
            return;

        if (!other.CompareTag("Player"))
            return;

        _hitPlayer = true;

        Rigidbody2D playerRb = other.attachedRigidbody;

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;

            Vector2 force =
                _moveDirection * _hitForce +
                Vector2.up * _upForce;

            playerRb.AddForce(force, ForceMode2D.Impulse);
        }

        // 네 사망 처리 연결
    }


    private void FinishTrap()
    {
        _moving = false;

        _rb.linearVelocity = Vector2.zero;

        _spriteRenderer.enabled = false;
        _collider.enabled = false;
    }


    // =========================================
    // ★ 리스폰하면 여기 실행
    // =========================================
    public void ResetTrap()
    {
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }

        // ★ 다시 발동 가능하도록 전부 초기화
        _activated = false;
        _moving = false;
        _hitPlayer = false;

        if (_rb != null)
        {
            _rb.bodyType = RigidbodyType2D.Kinematic;

            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;

            // ★ 처음 위치로 복귀
            _rb.position = _startPosition;
        }

        // 처음에는 안 보이게
        if (_spriteRenderer != null)
            _spriteRenderer.enabled = false;

        if (_collider != null)
            _collider.enabled = false;
    }
}