using UnityEngine;

public class CameraController : MonoBehaviour
{
    private static CameraController _instance;
    public static CameraController Instance => _instance;

    [SerializeField] private Transform _target;
    [SerializeField] private Vector2 _minPosition = new Vector2(-7f, -2.5f);
    [SerializeField] private Vector2 _maxPosition = new Vector2(14f, 8f);

    private CameraLookAhead _lookAhead;
    private CameraShake _shake;

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
        /*
        if (_target == null)
        {
            _target = transform.parent ?? GameObject.FindGameObjectWithTag("Player").transform;
        }
        */

        //transform.position = _target.position + new Vector3(0f, 0f, transform.position.z);
        _baseOffset = transform.position - _target.position;
    }

    /*
    private void LateUpdate()
    {
        if (_target == null)
            return;

        Vector3 nextPosition = _target.position + _baseOffset;

        if (_lookAhead != null)
        {
            nextPosition += (Vector3)_lookAhead.Offset;
        }

        if (_shake != null)
        {
            nextPosition += (Vector3)_shake.Offset;
        }

        // 최소 좌표를 넘어가려고 하면 Clamp
        if (nextPosition.x < _minPosition.x)
        {
            nextPosition.x = _minPosition.x;
        }

        if (nextPosition.y < _minPosition.y)
        {
            nextPosition.y = _minPosition.y;
        }

        // 최대 좌표를 넘어가려고 하면 Clamp
        if (nextPosition.x > _maxPosition.x)
        {
            nextPosition.x = _maxPosition.x;
        }

        if (nextPosition.y > _maxPosition.y)
        {
            nextPosition.y = _maxPosition.y;
        }

        transform.position = nextPosition;
    }
    */

    public void StartShake(float force, float duration)
    {
        _shake?.StartShake(force, duration);
    }

    public void StartPerlinShake(float force, float roughness, float duration)
    {
        _shake?.StartPerlinShake(force, roughness, duration);
    }
}