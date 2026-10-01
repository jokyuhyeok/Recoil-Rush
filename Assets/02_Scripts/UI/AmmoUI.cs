using System;
using System.Collections.Generic;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    //출력할 무기 값 확인
    [Serializable]
    private class WeaponAmmoUI
    {
        public Weapon weapon;
        public BulletsAmountUI bulletPrefab;
    }

    //출력 위치
    [Header("UI")]
    [SerializeField] private Transform _bulletContainer;

    // 집어넣는 곳
    [Header("Weapon UI")]
    [SerializeField] private List<WeaponAmmoUI> _weaponAmmoUIList;

    // 무기별로 최초 한 번 생성한 총알 UI 저장
    private readonly Dictionary<Weapon, List<BulletsAmountUI>> _bulletPools
        = new Dictionary<Weapon, List<BulletsAmountUI>>();

    private Weapon _currentWeapon;
    private List<BulletsAmountUI> _currentBullets;

    private int _previousAmmo = -1;


    private void OnEnable()
    {
        Weapon.OnWeaponActivated += HandleWeaponActivated;
    }

    private void Start()
    {
        // 시작 시 이미 활성화되어 있던 무기가
        // OnEnable 이벤트를 먼저 발생시켰을 수도 있으므로 최초 1회 확인
        foreach (WeaponAmmoUI data in _weaponAmmoUIList)
        {
            if (data.weapon == null)
                continue;

            if (data.weapon.gameObject.activeSelf)
            {
                HandleWeaponActivated(data.weapon);
                break;
            }
        }
    }

    private void OnDisable()
    {
        Weapon.OnWeaponActivated -= HandleWeaponActivated;

        if (_currentWeapon != null)
        {
            _currentWeapon.OnAmmoChanged -= HandleAmmoChanged;
        }
    }

    // 무기 활성화

    private void HandleWeaponActivated(Weapon weapon)
    {
        if (weapon == null)
            return;

        if (_currentWeapon == weapon)
            return;

        WeaponAmmoUI data = FindWeaponData(weapon);

        if (data == null)
            return;

        // 이전 무기의 탄약 이벤트 구독 해제
        if (_currentWeapon != null)
        {
            _currentWeapon.OnAmmoChanged -= HandleAmmoChanged;
        }

        // 해당 무기의 UI가 아직 없으면 최초 1회 생성
        CreateBulletsUI(data);

        // 현재 UI 변경
        ChangeWeapon(data);

        // 새 무기의 탄약 변경 이벤트 구독
        _currentWeapon.OnAmmoChanged += HandleAmmoChanged;
    }

    private WeaponAmmoUI FindWeaponData(Weapon weapon)
    {
        foreach (WeaponAmmoUI data in _weaponAmmoUIList)
        {
            if (data.weapon == weapon)
            {
                return data;
            }
        }

        return null;
    }

    // UI Pool 생성
    private void CreateBulletsUI(WeaponAmmoUI data)
    {
        if (data.weapon == null || data.bulletPrefab == null)
            return;

        // 이미 생성한 무기면 다시 Instantiate 하지 않음
        if (_bulletPools.ContainsKey(data.weapon))
            return;

        List<BulletsAmountUI> bullets = new List<BulletsAmountUI>();

        for (int i = 0; i < data.weapon.Magazine; i++)
        {
            BulletsAmountUI bullet = Instantiate(data.bulletPrefab, _bulletContainer);

            bullet.gameObject.SetActive(false);

            bullets.Add(bullet);
        }

        _bulletPools.Add(data.weapon, bullets);
    }

    // 무기 UI 교체

    private void ChangeWeapon(WeaponAmmoUI data)
    {
        // 이전 무기 UI 끄기
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

        // 현재 무기 UI 켜기
        foreach (BulletsAmountUI bullet in _currentBullets)
        {
            bullet.gameObject.SetActive(true);
        }

        RefreshAmmoState();

        _previousAmmo = _currentWeapon.CurrentAmmo;
    }

    // 탄약 변경 이벤트

    private void HandleAmmoChanged(int currentAmmo)
    {
        if (_currentWeapon == null || _currentBullets == null)
        {
            return;
        }

        // 정상적인 1발 발사
        if (currentAmmo == _previousAmmo - 1)
        {
            int firedIndex =
                _currentWeapon.Magazine - currentAmmo - 1;

            if (firedIndex >= 0 &&
                firedIndex < _currentBullets.Count)
            {
                _currentBullets[firedIndex].Fire();
            }
            else
            {
                // 예상하지 못한 값이면 전체 상태 재동기화
                RefreshAmmoState();
            }
        }
        else
        {
            // 재장전 또는 한 번에 여러 발 변화한 예외 상황
            RefreshAmmoState();
        }

        _previousAmmo = currentAmmo;
    }

    // 현재 탄약 상태 전체 반영
    private void RefreshAmmoState()
    {
        if (_currentWeapon == null || _currentBullets == null)
        {
            return;
        }

        int usedAmmo = _currentWeapon.Magazine - _currentWeapon.CurrentAmmo;

        usedAmmo = Mathf.Clamp(usedAmmo, 0, _currentBullets.Count);

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