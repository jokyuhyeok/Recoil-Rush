using System.Collections.Generic;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform _bulletContainer;

    [Header("Test")]
    [SerializeField] private Weapon _testWeapon;
    [SerializeField] private BulletsAmountUI _testBulletPrefab;

    private Weapon _currentWeapon;
    private BulletsAmountUI _currentBulletPrefab;

    private readonly List<BulletsAmountUI> _bullets
        = new List<BulletsAmountUI>();

    private int _previousAmmo = -1;

    private void Start()
    {
        SetWeapon(_testWeapon, _testBulletPrefab);
    }

    private void Update()
    {
        if (_currentWeapon == null)
            return;

        int currentAmmo = _currentWeapon.CurrentAmmo;

        if (currentAmmo == _previousAmmo)
            return;

        // 총알 감소 = 발사
        if (currentAmmo < _previousAmmo)
        {
            HandleAmmoDecrease(_previousAmmo, currentAmmo);
        }

        // 총알 증가 = 재장전
        else
        {
            RefreshAmmoState();
        }

        _previousAmmo = currentAmmo;
    }

    public void SetWeapon(
        Weapon weapon,
        BulletsAmountUI bulletPrefab)
    {
        if (weapon == null || bulletPrefab == null)
            return;

        _currentWeapon = weapon;
        _currentBulletPrefab = bulletPrefab;

        CreateAmmoUI();

        _previousAmmo = _currentWeapon.CurrentAmmo;
    }

    private void CreateAmmoUI()
    {
        foreach (BulletsAmountUI bullet in _bullets)
        {
            if (bullet != null)
                Destroy(bullet.gameObject);
        }

        _bullets.Clear();

        for (int i = 0; i < _currentWeapon.Magazine; i++)
        {
            BulletsAmountUI bullet = Instantiate(
                _currentBulletPrefab,
                _bulletContainer
            );

            _bullets.Add(bullet);
        }

        RefreshAmmoState();
    }

    private void HandleAmmoDecrease(
        int previousAmmo,
        int currentAmmo)
    {
        int magazine = _currentWeapon.Magazine;

        int usedBefore = magazine - previousAmmo;
        int usedNow = magazine - currentAmmo;

        for (int i = usedBefore; i < usedNow; i++)
        {
            if (i < 0 || i >= _bullets.Count)
                continue;

            _bullets[i].Fire();
        }
    }

    private void RefreshAmmoState()
    {
        int usedAmmo =
            _currentWeapon.Magazine -
            _currentWeapon.CurrentAmmo;

        for (int i = 0; i < _bullets.Count; i++)
        {
            if (i < usedAmmo)
            {
                _bullets[i].SetSpentInstant();
            }
            else
            {
                _bullets[i].ResetBullet();
            }
        }
    }
}