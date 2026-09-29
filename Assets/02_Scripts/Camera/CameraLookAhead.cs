using UnityEngine;

public class CameraLookAhead : MonoBehaviour
{
    // 카메라가 앞서서 볼 최대 거리
    [SerializeField] private float _maxDistance = 4f;
    // 해당 위치까지 카메라가 도달할 시간 (높을 수록 느리지만 부드러움)
    [SerializeField] private float _smoothTime = 0.3f;
    // 마우스 위치 참고 시 마우스가 최소한으로 움직여야 하는 거리
    [SerializeField, Range(0, 0.999f)] private float _minMouseDistance = 0.2f;

    // 최종 Look ahead 오프셋 저장하는 변수
    private Vector2 _offset = Vector2.zero;
    public Vector2 Offset => _offset;

    // SmoothDamp에 넘겨주는 용도, 따로 사용 X
    private Vector2 _lookVelocity = Vector2.zero;


    private void Update()
    {
        LookAhead();
    }

    private void LookAhead()
    {
        // 마우스의 위치를 받고 화면 중앙점 기준 -1 ~ 1 범위로 변환
        float mouseX = (Input.mousePosition.x - Screen.width / 2f) / (Screen.width / 2f);
        float mouseY = (Input.mousePosition.y - Screen.height / 2f) / (Screen.height / 2f);
        Vector2 mousePosition = new Vector2(mouseX, mouseY);

        // 대각선 이동이 루트 2가 나오지 않게 크기를 1로 제한
        // (정규화하면 얼마나 먼지 거리 정보가 사라지므로 Clamp를 사용해야 함)
        mousePosition = Vector2.ClampMagnitude(mousePosition, 1f);

        // 마우스가 충분히 이동하지 않았다면 무시함
        if (mousePosition.sqrMagnitude < _minMouseDistance * _minMouseDistance)
        {
            mousePosition = Vector2.zero;
        }
        // 경계에서 확 튀지 않게 경계값 보정        
        else
        {
            // _minMouseDistance ~ 1 범위를 0 ~ 1 범위로 보정
            float magnitude = (mousePosition.magnitude - _minMouseDistance) / (1f - _minMouseDistance);

            // 거리 구했으니까 방향 벡터 추출하고 거리 곱해주면 보정된 벡터 산출 완료!
            mousePosition = mousePosition.normalized * magnitude;
        }

        // 실제로 Look ahead하는 거리 계산
        _offset = Vector2.SmoothDamp(_offset, mousePosition * _maxDistance, ref _lookVelocity, _smoothTime);
    }
}