using UnityEngine;

public class CameraLookAhead : MonoBehaviour
{
    [SerializeField] private GameObject _target;

    [Tooltip("Look ahead할 거리")] [SerializeField]
    private float _lookAheadDistance = 4f;
    [Tooltip("카메라가 도착할 시간")] [SerializeField]
    private float _smoothTime = 0.3f;

    [Tooltip("Look ahead 가중치\n0 = 플레이어 진행 방향만 반영\n1 = 마우스 위치만 반영")] [SerializeField, Range(0, 1)]
    private float _mouseWeightRatio = 0.65f;

    private Vector3 _basePosition;
    public Vector3 BasePosition => _basePosition;
    private Vector3 _cameraOffset;
    private Vector2 _lookAheadOffset = Vector2.zero;
    private Vector2 _lookVelocity = Vector2.zero;

    private void Start()
    {
        _cameraOffset = transform.position - _target.transform.position;
    }

    private void Update()
    {
        LookAhead();
    }

    private void LookAhead()
    {
        // 플레이어 진행 방향, 임시로 Input 그대로 가져오기
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 playerDirection = Vector2.ClampMagnitude(new Vector2(h, v), 1f);

        // 마우스의 위치를 받고 화면 중앙점 기준 -1 ~ 1 범위로 변환
        float mouseX = (Input.mousePosition.x - Screen.width / 2f) / (Screen.width / 2f);
        float mouseY = (Input.mousePosition.y - Screen.height / 2f) / (Screen.height / 2f);
        Vector2 mousePosition = new Vector2(mouseX, mouseY);

        // 대각선 이동이 루트 2가 나오지 않게 크기를 1로 제한
        // (정규화하면 얼마나 먼지 거리 정보가 사라지므로 Clamp를 사용해야 함)
        mousePosition = Vector2.ClampMagnitude(mousePosition, 1f);

        // 플레이어 진행 방향과 마우스 위치를 비율에 맞게 더하여 최종 벡터 계산 (크기는 -1 ~ 1 사이)
        Vector2 lookAheadVector = playerDirection * (1 - _mouseWeightRatio) + mousePosition * _mouseWeightRatio;

        // 실제로 Look ahead하는 거리 계산
        _lookAheadOffset = Vector2.SmoothDamp(_lookAheadOffset, lookAheadVector * _lookAheadDistance, ref _lookVelocity, _smoothTime);

        // 현재 플레이어 위치 + Look ahead 보정치 + 원래 카메라 보정치로 이동
        _basePosition = new Vector3(
            _target.transform.position.x + _lookAheadOffset.x + _cameraOffset.x,
            _target.transform.position.y + _lookAheadOffset.y + _cameraOffset.y,
            _cameraOffset.z);

        transform.position = _basePosition;
    }

    public void ToggleLookAhead(bool isOn)
    {
        _lookAheadDistance = isOn ? 4f : 0f;
        _mouseWeightRatio = isOn ? 0.65f : 0f;
    }
}