using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private CameraLookAhead _lookAhead;
    [SerializeField] private CameraShake _shake;

    private Vector3 _baseOffset;


    private void Start()
    {
        _baseOffset = transform.position - _target.position;
    }

    private void LateUpdate()
    {
        transform.position = _target.position + _baseOffset + (Vector3)_lookAhead.Offset + (Vector3)_shake.Offset;
    }
}