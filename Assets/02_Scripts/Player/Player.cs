using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float _reloadTime = 2f;

    private bool _isGrounded;
    private bool _isReloading;
    private float _reloadTimer;

    private Weapon[] _weapons;
    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        // 자신과 모든 자식 오브젝트에서 <Weapon>을 상속한 컴포넌트를 전부 찾아라. / true: 비활성화된 것도 전부
        _weapons = GetComponentsInChildren<Weapon>(true);
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        ReloadUpdate();
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

    private void TryReload()
    {
        if (!_isGrounded || _isReloading)
        {
            return;
        }

        foreach (Weapon weapon in _weapons)
        {
            if (weapon.AmmoReload)
            {
                _isReloading = true;
                _reloadTimer = _reloadTime;
                return;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = true;

            TryReload();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = false;
        }
    }
}