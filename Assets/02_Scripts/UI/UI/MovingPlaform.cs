using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Points")]
    [SerializeField] private Transform _startPoint;
    [SerializeField] private Transform _endPoint;

    [Header("Movement Settings")]
    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _waitTime = 0.5f;

    private Rigidbody2D _rb;

    private Vector2 _startPosition;
    private Vector2 _endPosition;
    private Vector2 _targetPosition;

    private bool _movingToEnd = true;
    private bool _waiting;
    private float _waitTimer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        _startPosition = _startPoint.position;
        _endPosition = _endPoint.position;

        _rb.position = _startPosition;
        _targetPosition = _endPosition;
    }

    private void FixedUpdate()
    {
        if (_waiting)
        {
            _waitTimer -= Time.fixedDeltaTime;

            if (_waitTimer <= 0f)
            {
                _waiting = false;

                _movingToEnd = !_movingToEnd;

                _targetPosition = _movingToEnd
                    ? _endPosition
                    : _startPosition;
            }

            return;
        }

        Vector2 nextPosition = Vector2.MoveTowards(
            _rb.position,
            _targetPosition,
            _speed * Time.fixedDeltaTime
        );

        _rb.MovePosition(nextPosition);

        if (Vector2.Distance(_rb.position, _targetPosition) <= 0.02f)
        {
            _rb.position = _targetPosition;

            _waiting = true;
            _waitTimer = _waitTime;
        }
    }
}