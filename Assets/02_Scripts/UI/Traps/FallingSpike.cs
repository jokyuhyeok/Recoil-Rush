using UnityEngine;

public class FallingSpike : MonoBehaviour
{
    [SerializeField] private float _gravityScale = 4f;

    private Rigidbody2D _rb;

    private Vector2 _startPosition;
    private float _startRotation;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        _startPosition = _rb.position;
        _startRotation = _rb.rotation;

        ResetSpike();
    }

    public void Drop()
    {
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.gravityScale = _gravityScale;
        _rb.WakeUp();
    }

    public void ResetSpike()
    {
        if (_rb == null)
            return;

        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.gravityScale = 0f;

        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;

        _rb.position = _startPosition;
        _rb.rotation = _startRotation;
    }
}