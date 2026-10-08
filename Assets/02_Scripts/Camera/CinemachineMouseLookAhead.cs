using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(CinemachineCamera))]
[RequireComponent(typeof(CinemachinePositionComposer))]
public class CinemachineMouseLookAhead : MonoBehaviour
{
    private CameraLookAhead _lookAhead;

    private CinemachinePositionComposer _positionComposer;
    private Vector3 _defaultTargetOffset;


    private void Awake()
    {
        _positionComposer = GetComponent<CinemachinePositionComposer>();

        // Inspector에서 설정해둔 기존 Target Offset 보존
        _defaultTargetOffset = _positionComposer.TargetOffset;
    }

    private void Start()
    {
        _lookAhead = Camera.main?.GetComponent<CameraLookAhead>();
    }

    private void LateUpdate()
    {
        if (_lookAhead == null)
        {
            return;
        }

        Vector2 lookAheadOffset = _lookAhead.Offset;

        _positionComposer.TargetOffset =
            _defaultTargetOffset +
            new Vector3(
                lookAheadOffset.x,
                lookAheadOffset.y,
                0f
            );
    }
}