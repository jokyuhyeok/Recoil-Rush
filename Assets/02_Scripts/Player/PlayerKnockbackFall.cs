using UnityEngine;

public class PlayerKnockbackFall : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rb;

    [Header("Launch")]
    [SerializeField] private float _launchSpeed = 25f;

    [Header("Death")]
    [SerializeField] private float _screenExitMargin = 0.05f;

    private bool _isKnocked;
    private bool _deathSent;

    private Vector2 _launchDirection;

    private Camera _mainCamera;
    private CameraController _cameraController;

    private bool _cameraControllerWasEnabled;

    private Transform _cameraOriginalParent;
    private int _cameraOriginalSiblingIndex;
    private bool _cameraDetached;


    private void Awake()
    {
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        _mainCamera = Camera.main;
    }


    private void Start()
    {
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.OnRespawnEvent
                += ResetKnockback;
        }
    }


    private void OnDestroy()
    {
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.OnRespawnEvent
                -= ResetKnockback;
        }
    }


    private void Update()
    {
        if (!_isKnocked)
            return;


        // ★ 물리 충돌 없이 직접 날림
        transform.position +=
            (Vector3)(_launchDirection *
                      _launchSpeed *
                      Time.deltaTime);


        if (_mainCamera == null)
            _mainCamera = Camera.main;

        if (_mainCamera == null)
            return;


        // ★ 화면 밖으로 나갔는지 검사
        Vector3 viewport =
            _mainCamera.WorldToViewportPoint(
                transform.position
            );


        if (viewport.x < -_screenExitMargin ||
            viewport.x > 1f + _screenExitMargin ||
            viewport.y < -_screenExitMargin ||
            viewport.y > 1f + _screenExitMargin)
        {
            DieByKnockback();
        }
    }


    public void Launch(Vector2 hitDirection)
    {
        if (_isKnocked)
            return;

        if (hitDirection.sqrMagnitude < 0.001f)
            return;


        _isKnocked = true;
        _deathSent = false;

        // ★ 실제 맞은 방향
        _launchDirection = hitDirection.normalized;


        // 기존 물리 운동 제거
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;


        // ★ Rigidbody 물리 자체를 꺼버림
        // 벽, Ground, Dangerous 등 아무것과도 충돌 안 함
        _rb.simulated = false;


        FreezeCamera();
    }


    private void FreezeCamera()
    {
        _mainCamera = Camera.main;

        _cameraController =
            CameraController.Instance;


        // CameraController 정지
        if (_cameraController != null)
        {
            _cameraControllerWasEnabled =
                _cameraController.enabled;

            _cameraController.enabled = false;
        }


        // ★ Camera가 Player의 자식이면
        // Player가 날아갈 때 Camera도 따라가므로 분리
        if (_mainCamera != null &&
            _mainCamera.transform.IsChildOf(transform))
        {
            _cameraOriginalParent =
                _mainCamera.transform.parent;

            _cameraOriginalSiblingIndex =
                _mainCamera.transform.GetSiblingIndex();

            _mainCamera.transform.SetParent(
                null,
                true
            );

            _cameraDetached = true;
        }
    }


    private void DieByKnockback()
    {
        if (_deathSent)
            return;

        _deathSent = true;
        _isKnocked = false;


        // 죽기 직전 물리 복구
        _rb.simulated = true;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;


        // ★ Player.cs에 이미 있는 private Die() 실행
        // Player.cs 수정 필요 없음
        gameObject.SendMessage(
            "Die",
            SendMessageOptions.RequireReceiver
        );
    }


    private void ResetKnockback(Vector3 _)
    {
        _isKnocked = false;
        _deathSent = false;

        _rb.simulated = true;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;


        RestoreCamera();
    }


    private void RestoreCamera()
    {
        // Camera가 Player에서 분리됐었다면 복구
        if (_cameraDetached &&
            _mainCamera != null)
        {
            _mainCamera.transform.SetParent(
                _cameraOriginalParent,
                true
            );

            _mainCamera.transform.SetSiblingIndex(
                _cameraOriginalSiblingIndex
            );

            _cameraDetached = false;
        }


        // CameraController 다시 활성화
        if (_cameraController != null)
        {
            _cameraController.enabled =
                _cameraControllerWasEnabled;
        }
    }
}