using System.Collections;
using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [Header("Aim Point")]
    [SerializeField] private Transform _aimPoint;
    [SerializeField] private Sprite _fireAimPoint;
    [SerializeField] private float _fireAimDuration = 0.1f;

    private Coroutine _fireAimCoroutine;

    // [SerializeField] private LineRenderer _aimLine;

    private Sprite _normalAimPoint;
    private SpriteRenderer _aimPointSprite;

    private Player _player;
    private SpriteRenderer _playerSprite;
    private Weapon _weapon;
    private SpriteRenderer _weaponSprite;

    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;

        _aimPointSprite = _aimPoint.GetComponent<SpriteRenderer>();
        _normalAimPoint = _aimPointSprite.sprite;

        // HS: ??부터는 테스트용 방어 코드로 넣어놨습니다. 추후 제거 예정입니다.
        _player = GetComponent<Player>() ?? GetComponentInParent<Player>();
        _playerSprite = _player.GetComponent<SpriteRenderer>();
    }

    // 플레이 중 OS 커서를 숨기고 창 안에 가둠
    private void OnEnable()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    // 비활성화 시 커서를 다시 보여줌
    private void OnDisable()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Start()
    {
        _weapon = _player.CurrentWeapon;
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

        Vector2 playerDirection = mousePosition - transform.position;
        bool isRight = playerDirection.x > 0f;

        _playerSprite.flipX = isRight;

        Vector3 weaponPosition = _weapon.transform.localPosition;

        // Mathf.Abs(값): 값을 절대값으로 바꿈
        weaponPosition.x = !isRight ? -Mathf.Abs(weaponPosition.x) : Mathf.Abs(weaponPosition.x);
        _weapon.transform.localPosition = weaponPosition;

        // 목표로 향하는 방향 = 목표 위치 - 현재 위치
        Vector2 direction = mousePosition - _weapon.transform.position;

        // Atan2 = X/Y 방향을 각도로 변환 / Atan2는 라디안 이라는 단위라 Rad2Deg로 일반 각도로 변환
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Quaternion.Euler: X, Y, Z 회전 각도를 Unity가 사용하는 회전값인 Quaternion으로 변환
        _weapon.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // 왼쪽이면 scale.y를 -로 뒤집음, 기존의 flipY와 달리 FirePoint인 자식도 같이 뒤집힘
        Vector3 weaponScale = _weapon.transform.localScale;
        weaponScale.y = !isRight ? -Mathf.Abs(weaponScale.y) : Mathf.Abs(weaponScale.y);
        _weapon.transform.localScale = weaponScale;

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
        _aimPointSprite.sprite = _fireAimPoint;

        yield return new WaitForSeconds(_fireAimDuration);

        _aimPointSprite.sprite = _normalAimPoint;

        _fireAimCoroutine = null;
    }

    public void ChangeWeapon(Weapon weapon)
    {
        _weapon = weapon;
    }
}