using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [Header("Ammo")]
    [SerializeField] private int _magazine = 6;

    [Header("Fire")]
    [SerializeField] protected Transform _firePoint;
    [SerializeField] private float _fireSpeed = 0.2f;

    [Header("Bullet")]
    // [SerializeField] protected Bullet _bulletPrefab;
    [SerializeField] private float _bulletSpeed = 20f;
    [SerializeField] private int _damage = 1;

    private int _currentAmmo;
    private float _fireCooldown;

    public Transform FirePoint => _firePoint;

    public float BulletSpeed => _bulletSpeed;
    public int Damage => _damage;
    public bool AmmoReload => _currentAmmo < _magazine;

    protected virtual void Awake()
    {
        _currentAmmo = _magazine;
    }

    protected virtual void Update()
    {
        if (_fireCooldown > 0f)
        {
            _fireCooldown -= Time.deltaTime;
        }
    }

    public void TryFire()
    {
        if (_currentAmmo <= 0 || _fireCooldown > 0f)
        {
            return;
        }

        Fire();

        _currentAmmo--;
        _fireCooldown = _fireSpeed;
    }

    public void Reload()
    {
        _currentAmmo = _magazine;
    }

    protected abstract void Fire();
}