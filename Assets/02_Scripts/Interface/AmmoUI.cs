using System;
using System.Collections.Generic;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    [Serializable]
    private class WeaponAmmoUI
    {
        public Weapon _weapon;
        public BulletsAmountUI _bulletPrefab;
    }

    [Header("UI")]
    [SerializeField] private Transform _bulletContainer;

    [Header("Weapon UI")]
    [SerializeField] private List<WeaponAmmoUI> _weaponAmmoUIList;

    private readonly List<BulletsAmountUI> _bullets
        = new List<BulletsAmountUI>();

    private Weapon _currentWeapon;
    private int _previousAmmo = -1;

    private void Start()
    {
        CheckCurrentWeapon();
    }

    private void Update()
    {
        CheckCurrentWeapon();
        CheckAmmo();
    }

    // 현재 활성화된 총 체크
    private void CheckCurrentWeapon()
    {
        foreach (WeaponAmmoUI data in _weaponAmmoUIList)
        {
            if (data._weapon == null)
                continue;

            // 현재 활성화되어 있는 총
            if (data._weapon.gameObject.activeInHierarchy)
            {
                // 기존 총과 같으면 다시 만들 필요 없음
                if (_currentWeapon == data._weapon)
                    return;

                ChangeWeapon(data._weapon, data._bulletPrefab);
                return;
            }
        }
    }

    private void ChangeWeapon(
        Weapon weapon,
        BulletsAmountUI bulletPrefab)
    {
        _currentWeapon = weapon;

        ClearAmmoUI();

        // 현재 총의 Magazine만큼 생성
        for (int i = 0; i < _currentWeapon.Magazine; i++)
        {
            BulletsAmountUI bullet = Instantiate(
                bulletPrefab,
                _bulletContainer
            );

            _bullets.Add(bullet);
        }

        RefreshAmmoState();

        _previousAmmo = _currentWeapon.CurrentAmmo;
    }

    private void CheckAmmo()
    {
        if (_currentWeapon == null)
            return;

        int currentAmmo = _currentWeapon.CurrentAmmo;

        if (currentAmmo == _previousAmmo)
            return;

        // 발사
        if (currentAmmo < _previousAmmo)
        {
            int usedBefore =
                _currentWeapon.Magazine - _previousAmmo;

            int usedNow =
                _currentWeapon.Magazine - currentAmmo;

            for (int i = usedBefore; i < usedNow; i++)
            {
                if (i >= 0 && i < _bullets.Count)
                    _bullets[i].Fire();
            }
        }

        // 재장전
        else
        {
            RefreshAmmoState();
        }

        _previousAmmo = currentAmmo;
    }

    private void RefreshAmmoState()
    {
        if (_currentWeapon == null)
            return;

        int usedAmmo =
            _currentWeapon.Magazine -
            _currentWeapon.CurrentAmmo;

        for (int i = 0; i < _bullets.Count; i++)
        {
            if (i < usedAmmo)
                _bullets[i].SetSpentInstant();
            else
                _bullets[i].ResetBullet();
        }
    }

    private void ClearAmmoUI()
    {
        foreach (BulletsAmountUI bullet in _bullets)
        {
            if (bullet != null)
                Destroy(bullet.gameObject);
        }

        _bullets.Clear();
    }
}