using UnityEngine;
using System;

public abstract class Weapon : MonoBehaviour
{
    [Header("Weapon Type")]
    [SerializeField] private WeaponType _weaponType;

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

    [Header("Effect")]
    [SerializeField] private SpriteEffect _fireEffect;

    [Header("Sound")]
    [SerializeField] private SoundEffectType _fireSound;

    [SerializeField] private StartUI _startUi;

    private int _currentAmmo;
    private float _fireCooldown;

    public WeaponType WeaponType => _weaponType;

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

        // _fireEffect가 비어있으면 자식에서 총구 이펙트 자동 검색 (false 포함) <- 실수 방지
        if (_fireEffect == null)
        {
            _fireEffect = GetComponentInChildren<SpriteEffect>(true);
        }

        if (_fireEffect != null)
        {
            _fireEffect.gameObject.SetActive(false);
        }
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
        // 옵션창이 열려 있으면 발사 금지
        if (_startUi != null && _startUi.IsOptionOpen)
        {
            return false;
        }

        if (_currentAmmo <= 0 || _fireCooldown > 0f || _player.IsReloading)
        {
            return false;
        }

        Fire();
        AddRecoil();

        if (_fireEffect != null)
        {
            _fireEffect.Play();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(_fireSound);
        }

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