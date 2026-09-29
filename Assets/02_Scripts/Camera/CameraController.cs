using UnityEngine;

public class CameraController : MonoBehaviour
{
    private static CameraController _instance;
    public static CameraController Instance => _instance;

    [SerializeField] private Transform _target;
    [Tooltip("카메라 최소/최대 높이 제한. 음수일 경우 제한이 없습니다.")]
    [SerializeField] private Vector2 _yLimit = new Vector2(2.5f, -1f);

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
        if (_target == null)
        {
            _target = transform.parent ?? GameObject.FindGameObjectWithTag("Player").transform;
        }

        //transform.position = _target.position + new Vector3(0f, 0f, transform.position.z);
        _baseOffset = transform.position - _target.position;
    }

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

        // 최소 높이 제한이 걸려있고, 최소 높이 제한보다 낮으면 Clamp
        if (_yLimit.x >= 0 && nextPosition.y < -_yLimit.x)
        {
            nextPosition.y = -_yLimit.x;
        }

        // 최대 높이 제한이 걸려있고, 최대 높이 제한보다 높으면 Clamp
        if (_yLimit.y >= 0 && nextPosition.y > _yLimit.y)
        {
            nextPosition.y = _yLimit.y;
        }

        transform.position = nextPosition;
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