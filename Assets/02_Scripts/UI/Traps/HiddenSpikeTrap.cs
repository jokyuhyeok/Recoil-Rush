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

    private Vector3 _startPosition;
    private Quaternion _startRotation;

    private bool _triggered;
    private Coroutine _fallCoroutine;


    private void Awake()
    {
        _startPosition = _tileRb.transform.position;
        _startRotation = _tileRb.transform.rotation;

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
        yield return new WaitForSeconds(_fallDelay);

        foreach (GameObject spike in _spikes)
        {
            if (spike != null)
                spike.SetActive(true);
        }

        if (_groundCollider != null && _tileCollider != null)
        {
            Physics2D.IgnoreCollision(
                _tileCollider,
                _groundCollider,
                true
            );
        }

        _tileRb.bodyType = RigidbodyType2D.Dynamic;
        _tileRb.gravityScale = _gravityScale;
        _tileRb.linearVelocity =
            Vector2.down * _initialFallSpeed;

        yield return new WaitForSeconds(_colliderOffDelay);

        if (_tileCollider != null)
            _tileCollider.enabled = false;

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

        _tileRb.simulated = false;

        _tileRb.linearVelocity = Vector2.zero;
        _tileRb.angularVelocity = 0f;

        _tileRb.bodyType = RigidbodyType2D.Kinematic;
        _tileRb.gravityScale = 0f;

        _tileRb.transform.SetPositionAndRotation(
            _startPosition,
            _startRotation
        );

        _tileRb.position = _startPosition;
        _tileRb.rotation = _startRotation.eulerAngles.z;

        if (_tileCollider != null)
            _tileCollider.enabled = true;

        if (_groundCollider != null && _tileCollider != null)
        {
            Physics2D.IgnoreCollision(
                _tileCollider,
                _groundCollider,
                false
            );
        }

        foreach (GameObject spike in _spikes)
        {
            if (spike != null)
                spike.SetActive(false);
        }

        _tileRb.simulated = true;

        Physics2D.SyncTransforms();
    }
}