using System.Collections;
using UnityEngine;

public class FallingTileTrap : MonoBehaviour
{
    [Header("Tile")]
    [SerializeField] private Rigidbody2D _tileRb;

    [Header("Fall Settings")]
    [SerializeField] private float _fallDelay = 0.15f;
    [SerializeField] private float _gravityScale = 3f;

    private Vector2 _startPosition;
    private float _startRotation;

    private bool _activated;
    private Coroutine _fallCoroutine;
    private Coroutine _subscribeCoroutine;

    private void Awake()
    {
        _startPosition = _tileRb.position;
        _startRotation = _tileRb.rotation;

        SetInitialState();
    }

    private void Start()
    {
        // RespawnManager가 아직 생성 안 됐을 수도 있으므로 기다림
        _subscribeCoroutine = StartCoroutine(SubscribeRespawnEvent());
    }

    private IEnumerator SubscribeRespawnEvent()
    {
        while (RespawnManager.Instance == null)
            yield return null;

        RespawnManager.Instance.OnRespawnPlayerEvent += OnRespawnPlayer;

        Debug.Log("[FallingTileTrap] Respawn 이벤트 연결 완료");
    }

    private void OnDestroy()
    {
        if (RespawnManager.Instance != null)
            RespawnManager.Instance.OnRespawnPlayerEvent -= OnRespawnPlayer;
    }

    private void OnRespawnPlayer(Vector3 respawnPosition)
    {
        Debug.Log("[FallingTileTrap] 리스폰 감지");

        ResetTrap();
    }

    public void ActivateTrap()
    {
        if (_activated)
            return;

        _activated = true;

        _fallCoroutine = StartCoroutine(FallRoutine());
    }

    private IEnumerator FallRoutine()
    {
        yield return new WaitForSeconds(_fallDelay);

        _tileRb.bodyType = RigidbodyType2D.Dynamic;
        _tileRb.gravityScale = _gravityScale;
        _tileRb.WakeUp();
    }

    public void ResetTrap()
    {
        Debug.Log("[FallingTileTrap] 타일 리셋");

        if (_fallCoroutine != null)
        {
            StopCoroutine(_fallCoroutine);
            _fallCoroutine = null;
        }

        _activated = false;

        // 물리부터 정지
        _tileRb.linearVelocity = Vector2.zero;
        _tileRb.angularVelocity = 0f;

        _tileRb.bodyType = RigidbodyType2D.Kinematic;
        _tileRb.gravityScale = 0f;

        // Rigidbody 자체 위치를 복구
        _tileRb.position = _startPosition;
        _tileRb.rotation = _startRotation;

        _tileRb.linearVelocity = Vector2.zero;
        _tileRb.angularVelocity = 0f;

        _tileRb.WakeUp();
    }

    private void SetInitialState()
    {
        _tileRb.bodyType = RigidbodyType2D.Kinematic;
        _tileRb.gravityScale = 0f;
        _tileRb.linearVelocity = Vector2.zero;
        _tileRb.angularVelocity = 0f;
    }
}