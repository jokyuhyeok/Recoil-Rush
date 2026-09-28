using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] private Weapon _weapon;
    [SerializeField] private Transform _aimPoint;
    // [SerializeField] private LineRenderer _aimLine;

    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        Aim();

        if (Input.GetMouseButtonDown(0))
        {
            _weapon.TryFire();
        }
    }

    private void Aim()
    {
        // Input.mousePosition: 마우스의 화면 좌표
        // ScreenToWorldPoint(Input.mousePosition): 화면에서 마우스가 있는 위치를 Unity World 좌표로 변환
        Vector3 mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        // 보통 Camera는 Z -10 이기 때문에 이 값을 0으로 바꿔줌
        mousePosition.z = 0f;

        // 목표로 향하는 방향 = 목표 위치 - 현재 위치
        Vector2 direction = mousePosition - _weapon.transform.position;

        // Atan2 = X/Y 방향을 각도로 변환 / Atan2는 라디안 이라는 단위라 Rad2Deg로 일반 각도로 변환
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Quaternion.Euler: X, Y, Z 회전 각도를 Unity가 사용하는 회전값인 Quaternion으로 변환
        _weapon.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        _aimPoint.position = mousePosition;

        // SetPosition 0 - 1 사이에 레이저를 연결시킨다
        // _aimLine.SetPosition(0, _weapon.FirePoint.position);
        // _aimLine.SetPosition(1, mousePosition);
    }
}