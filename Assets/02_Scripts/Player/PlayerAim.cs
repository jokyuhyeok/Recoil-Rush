using System.Collections;
using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [Header("Aim Point")]
    [SerializeField] private Transform _aimPoint;
    [SerializeField] private GameObject _normalAimPoint;
    [SerializeField] private GameObject _fireAimPoint;
    [SerializeField] private float _fireAimDuration = 0.1f;

    private Coroutine _fireAimCoroutine;

    // [SerializeField] private LineRenderer _aimLine;

    private Player _player;
    private Weapon _weapon;
    private SpriteRenderer _weaponSprite;

    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
        _player = GetComponent<Player>();

        _normalAimPoint.SetActive(true);
        _fireAimPoint.SetActive(false);
    }

    private void Start()
    {
        _weapon = _player.CurrentWeapon;
        _weaponSprite = _weapon.GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        Aim();

        if (Input.GetMouseButtonDown(0))
        {
            if (_weapon.TryFire())
            {
                FireAimPoint();
            }
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

        _weaponSprite.flipY = direction.x < 0f;

        _aimPoint.position = mousePosition;

        // SetPosition 0 - 1 사이에 레이저를 연결시킨다
        // _aimLine.SetPosition(0, _weapon.FirePoint.position);
        // _aimLine.SetPosition(1, mousePosition);
    }

    private void FireAimPoint()
    {
        if (_fireAimCoroutine != null)
        {
            StopCoroutine(_fireAimCoroutine);
        }

        _fireAimCoroutine = StartCoroutine(FireAimPointRoutine());
    }

    private IEnumerator FireAimPointRoutine()
    {
        _normalAimPoint.SetActive(false);
        _fireAimPoint.SetActive(true);

        yield return new WaitForSeconds(_fireAimDuration);

        _normalAimPoint.SetActive(true);
        _fireAimPoint.SetActive(false);

        _fireAimCoroutine = null;
    }
}