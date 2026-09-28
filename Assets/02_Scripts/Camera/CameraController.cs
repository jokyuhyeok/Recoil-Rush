using UnityEngine;

public class CameraController : MonoBehaviour
{
    private static CameraController _instance;
    public static CameraController Instance => _instance;

    [SerializeField] private Transform _target;
    [SerializeField] private CameraLookAhead _lookAhead;
    [SerializeField] private CameraShake _shake;

    private Vector3 _baseOffset;


    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        if (_lookAhead == null)
        {
            _lookAhead = GetComponent<CameraLookAhead>() ?? GetComponentInChildren<CameraLookAhead>();
        }

        if (_shake == null)
        {
            _shake = GetComponent<CameraShake>() ?? GetComponentInChildren<CameraShake>();
        }
    }

    private void Start()
    {
        _baseOffset = transform.position - _target.position;
    }

    private void LateUpdate()
    {
        transform.position = _target.position + _baseOffset + (Vector3)_lookAhead.Offset + (Vector3)_shake.Offset;
    }

    public void StartShake(float force, float duration)
    {
        _shake.StartShake(force, duration);
    }

    public void StartPerlinShake(float force, float roughness, float duration)
    {
        _shake.StartPerlinShake(force, roughness, duration);
    }
}