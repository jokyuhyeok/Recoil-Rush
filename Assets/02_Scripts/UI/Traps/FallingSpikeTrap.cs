using System.Collections;
using UnityEngine;

public class FallingSpikeTrap : MonoBehaviour, IResettable
{
    [Header("Spike")]
    [SerializeField] private GameObject _spikePrefab;

    [Header("Position")]
    [SerializeField] private Transform _topPoint;
    [SerializeField] private Transform _bottomPoint;

    [Header("Fall")]
    [SerializeField] private float _gravityScale = 10f;
    [SerializeField] private float _initialFallSpeed = 3f;

    [Header("Return")]
    [SerializeField] private float _returnDuration = 0.3f;

    private GameObject _currentSpike;
    private Coroutine _returnCoroutine;

    private bool _isDown;


    public void DropTrap()
    {
        if (_isDown)
            return;

        _isDown = true;

        _currentSpike = Instantiate(
            _spikePrefab,
            _topPoint.position,
            _topPoint.rotation
        );

        Rigidbody2D rb =
            _currentSpike.GetComponent<Rigidbody2D>();

        Collider2D col =
            _currentSpike.GetComponent<Collider2D>();

        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = true;
        }

        rb.simulated = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = _gravityScale;

        rb.linearVelocity =
            Vector2.down * _initialFallSpeed;
    }


    public void ReturnTrap()
    {
        if (!_isDown || _returnCoroutine != null)
            return;

        // 떨어진 가시가 남아 있으면 제거
        if (_currentSpike != null)
            Destroy(_currentSpike);

        // 새 가시 생성
        _currentSpike = Instantiate(
            _spikePrefab,
            _bottomPoint.position,
            _topPoint.rotation
        );

        Rigidbody2D rb =
            _currentSpike.GetComponent<Rigidbody2D>();

        Collider2D col =
            _currentSpike.GetComponent<Collider2D>();

        // 복귀 중에는 물리 사용 안 함
        rb.simulated = false;

        // 복귀 중 플레이어 즉사 판정도 끔
        if (col != null)
            col.enabled = false;

        // ★ 화면 밖 자동 삭제 스크립트가 있으면 잠시 끄기
        MonoBehaviour[] behaviours =
            _currentSpike.GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null)
                continue;

            if (behaviour.GetType().Name.Contains("Destroy") ||
                behaviour.GetType().Name.Contains("Despawn"))
            {
                behaviour.enabled = false;
            }
        }

        _returnCoroutine =
            StartCoroutine(ReturnRoutine());
    }


    private IEnumerator ReturnRoutine()
    {
        Vector3 start = _bottomPoint.position;
        Vector3 end = _topPoint.position;

        float timer = 0f;

        while (timer < _returnDuration)
        {
            if (_currentSpike == null)
            {
                _returnCoroutine = null;
                yield break;
            }

            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(timer / _returnDuration);

            _currentSpike.transform.position =
                Vector3.Lerp(start, end, t);

            yield return null;
        }

        _currentSpike.transform.SetPositionAndRotation(
            _topPoint.position,
            _topPoint.rotation
        );

        // 위에 도착하면 숨김
        Destroy(_currentSpike);

        _currentSpike = null;
        _isDown = false;
        _returnCoroutine = null;
    }


    public void ResetTrap()
    {
        if (_returnCoroutine != null)
        {
            StopCoroutine(_returnCoroutine);
            _returnCoroutine = null;
        }

        if (_currentSpike != null)
        {
            Destroy(_currentSpike);
            _currentSpike = null;
        }

        _isDown = false;
    }
}