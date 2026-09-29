using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    [Header("Gravity")]
    [SerializeField] private float _gravity = 9f;

    [Header("Reload")]
    [SerializeField] private float _reloadTime = 2f;
    [SerializeField] private float _reloadGroundDelay = 0.2f;

    private List<Weapon> _weapons = new List<Weapon>();
    private Weapon _currentWeapon;

    private float _reloadTimer;
    private float _reloadGroundTimer;
    private bool _isReloading;
    private bool _isGrounded;

    private Rigidbody2D _rigidbody;

    public Weapon CurrentWeapon => _currentWeapon;
    public bool IsReloading => _isReloading;

    private void Awake()
    {
        // AddRange: 여러 개를 한번에 추가 / List
        // 자신과 모든 자식 오브젝트에서 Weapon을 상속한 컴포넌트를 전부 찾음 / true: 비활성화된 오브젝트도 포함
        _weapons.AddRange(GetComponentsInChildren<Weapon>(true));

        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.gravityScale = 0f;

        FindCurrentWeapon();
    }

    private void Start()
    {
        RespawnManager.Instance.OnRespawnEvent += OnRespawn;
    }

    private void Update()
    {
        ReloadUpdate();
        ReloadGroundCheck();
    }

    private void FixedUpdate()
    {
        ApplyGravity();
    }

    private void FindCurrentWeapon()
    {
        foreach (Weapon weapon in _weapons)
        {
            if (weapon.gameObject.activeInHierarchy)
            {
                _currentWeapon = weapon;
                return;
            }
        }
    }

    private void ApplyGravity()
    {
        Vector2 velocity = _rigidbody.linearVelocity;

        velocity.y -= _gravity * Time.fixedDeltaTime;

        _rigidbody.linearVelocity = velocity;
    }

    public void AddRecoil(Vector2 recoil)
    {
        _rigidbody.linearVelocity += recoil;
    }

    private void ReloadUpdate()
    {
        if (!_isReloading)
        {
            return;
        }

        _reloadTimer -= Time.deltaTime;

        if (_reloadTimer > 0f)
        {
            return;
        }

        foreach (Weapon weapon in _weapons)
        {
            weapon.Reload();
        }

        _isReloading = false;
    }

    private void ReloadGroundCheck()
    {
        if (!_isGrounded || _isReloading)
        {
            _reloadGroundTimer = 0f;
            return;
        }

        bool ammoReload = false;

        foreach (Weapon weapon in _weapons)
        {
            if (weapon.AmmoReload)
            {
                ammoReload = true;
                break;
            }
        }

        if (!ammoReload)
        {
            _reloadGroundTimer = 0f;
            return;
        }

        _reloadGroundTimer += Time.deltaTime;

        if (_reloadGroundTimer < _reloadGroundDelay)
        {
            return;
        }

        _isReloading = true;
        _reloadTimer = _reloadTime;
        _reloadGroundTimer = 0f;
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    public void Reset()
    {
        _reloadTimer = 0f;
        _reloadGroundTimer = 0f;
        _isReloading = false;

        _rigidbody.linearVelocity = Vector2.zero;

        foreach (Weapon weapon in _weapons)
        {
            weapon.Reload();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = true;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Dangerous"))
        {
            Die();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = false;
            _reloadGroundTimer = 0f;
        }
    }

    private void OnRespawn(Vector3 respawnPosition)
    {
        foreach (Weapon weapon in _weapons)
        {
            weapon.Reload();
        }

        transform.position = respawnPosition;
    }
}