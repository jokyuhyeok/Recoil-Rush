using UnityEngine;
using System;

public abstract class Weapon : MonoBehaviour
{
    [Header("Ammo")]
    [SerializeField] private int _magazine = 6;

    [Header("Fire")]
    [SerializeField] protected Transform _firePoint;

    [SerializeField] private float _fireSpeed = 0.2f;

    [Header("Bullet")]
    [SerializeField] protected Bullet _bulletPrefab;

    [SerializeField] private float _bulletSpeed = 20f;
    [SerializeField] private int _damage = 1;

    [Header("Recoil")]
    [SerializeField] private float _recoilPower = 5f;

    private int _currentAmmo;
    private float _fireCooldown;

    public Transform FirePoint => _firePoint;

    public float BulletSpeed => _bulletSpeed;
    public int Damage => _damage;

    public int Magazine => _magazine;
    public int CurrentAmmo => _currentAmmo;
    public bool AmmoReload => _currentAmmo < _magazine;
    public bool IsEmptyMagazine => _currentAmmo <= 0;

    //UI 연동할 이벤트 추가
    public event Action<int> OnAmmoChanged;
    public static event Action<Weapon> OnWeaponActivated;

    private Player _player;

    protected virtual void Awake()
    {
        _player = GetComponentInParent<Player>();

        _currentAmmo = _magazine;
    }

    // 무기 활성화 시 이벤트
    protected virtual void OnEnable()
    {
        OnWeaponActivated?.Invoke(this);
    }

    protected virtual void Update()
    {
        if (_fireCooldown > 0f)
        {
            _fireCooldown -= Time.deltaTime;
        }
    }

    public bool TryFire()
    {
        if (_currentAmmo <= 0 || _fireCooldown > 0f || _player.IsReloading)
        {
            return false;
        }

        Fire();
        AddRecoil();

        _currentAmmo--;

        OnAmmoChanged?.Invoke(_currentAmmo);

        _fireCooldown = _fireSpeed;

        return true;
    }

    private void AddRecoil()
    {
        // .right: X축 방향
        Vector2 recoil = -(Vector2)_firePoint.right * _recoilPower;

        _player.AddRecoil(recoil);
    }

    public void Reload()
    {
        _currentAmmo = _magazine;

        OnAmmoChanged?.Invoke(_currentAmmo);
    }

    protected abstract void Fire();
}