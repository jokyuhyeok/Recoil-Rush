using System;
using System.Collections.Generic;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    [Serializable]
    private class WeaponAmmoUI
    {
        public Weapon weapon;
        public BulletsAmountUI bulletPrefab;
    }

    [Header("UI")]
    [SerializeField] private Transform _bulletContainer;

    [Header("Weapon UI")]
    [SerializeField] private List<WeaponAmmoUI> _weaponAmmoUIList;

    private readonly Dictionary<Weapon, List<BulletsAmountUI>> _bulletPools
        = new Dictionary<Weapon, List<BulletsAmountUI>>();

    private Weapon _currentWeapon;
    private List<BulletsAmountUI> _currentBullets;
    private int _previousAmmo = -1;
    
    private void Update()
    {
        CheckCurrentWeapon();
        CheckAmmo();
    }
    
    private void CreateBulletsUI(WeaponAmmoUI data)
    {
        if (data.weapon == null || data.bulletPrefab == null)
            return;

        // 이미 만들어져 있으면 재생성 안 함
        if (_bulletPools.ContainsKey(data.weapon))
            return;

        List<BulletsAmountUI> bullets =
            new List<BulletsAmountUI>();

        for (int i = 0; i < data.weapon.Magazine; i++)
        {
            BulletsAmountUI bullet = Instantiate(data.bulletPrefab, _bulletContainer);

            bullet.gameObject.SetActive(false);

            bullets.Add(bullet);
        }

        _bulletPools.Add(data.weapon, bullets);
    }

    // 현재 활성화된 총 체크
    private void CheckCurrentWeapon()
    {
        foreach (WeaponAmmoUI data in _weaponAmmoUIList)
        {
            if (data.weapon == null)
                continue;

            // 현재 활성화된 총이 아니면 패스
            if (!data.weapon.gameObject.activeSelf)
                continue;

            // 이미 현재 총이면 아무것도 안 함
            if (_currentWeapon == data.weapon)
                return;

            // 이 총의 UI Pool이 아직 없으면 최초 1회 생성
            CreateBulletsUI(data);

            // UI 교체
            ChangeWeapon(data);

            return;
        }
    }

    private void ChangeWeapon(WeaponAmmoUI data)
    {
        // 이전 무기 총알 UI 끄기
        if (_currentBullets != null)
        {
            foreach (BulletsAmountUI bullet in _currentBullets)
            {
                bullet.gameObject.SetActive(false);
            }
        }

        _currentWeapon = data.weapon;

        if (!_bulletPools.TryGetValue(_currentWeapon, out _currentBullets))
        {
            return;
        }

        // 현재 무기 총알 UI 켜기
        foreach (BulletsAmountUI bullet in _currentBullets)
        {
            bullet.gameObject.SetActive(true);
        }

        // 현재 남아있는 탄약 상태 반영
        RefreshAmmoState();

        _previousAmmo = _currentWeapon.CurrentAmmo;
    }

    // 발사 및 재장전 확인
    private void CheckAmmo()
    {
        if (_currentWeapon == null || _currentBullets == null)
        {
            return;
        }

        int currentAmmo = _currentWeapon.CurrentAmmo;

        if (currentAmmo == _previousAmmo)
            return;

        // 탄 감소 = 발사
        if (currentAmmo < _previousAmmo)
        {
            int usedBefore = _currentWeapon.Magazine - _previousAmmo;

            int usedNow = _currentWeapon.Magazine - currentAmmo;

            for (int i = usedBefore; i < usedNow; i++)
            {
                if (i < 0 || i >= _currentBullets.Count)
                    continue;

                _currentBullets[i].Fire();
            }
        }

        // 탄 증가 = 재장전
        else
        {
            RefreshAmmoState();
        }

        _previousAmmo = currentAmmo;
    }

    private void RefreshAmmoState()
    {
        if (_currentWeapon == null || _currentBullets == null)
        {
            return;
        }

        int usedAmmo = _currentWeapon.Magazine - _currentWeapon.CurrentAmmo;

        for (int i = 0; i < _currentBullets.Count; i++)
        {
            if (i < usedAmmo)
            {
                _currentBullets[i].SetSpentInstant();
            }
            else
            {
                _currentBullets[i].ResetBullet();
            }
        }
    }
}