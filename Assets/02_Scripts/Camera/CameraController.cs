using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _target;

    private CameraLookAhead _lookAhead;
    private CameraShake _shake;

    private Vector3 _baseOffset;


    private void Awake()
    {
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
            if (transform.parent != null)
            {
                _target = transform.parent;
            }
            else
            {
                _target = GameObject.FindGameObjectWithTag("Player").transform;

                transform.SetParent(_target);
                transform.position = _target.position + new Vector3(0f, 0f, transform.position.z);
            }
        }

        _baseOffset = transform.position - _target.position;
    }

    private void LateUpdate()
    {
        if (_target == null)
            return;

        transform.position = _target.position + _baseOffset + (Vector3)_lookAhead.Offset + (Vector3)_shake.Offset;
    }
}