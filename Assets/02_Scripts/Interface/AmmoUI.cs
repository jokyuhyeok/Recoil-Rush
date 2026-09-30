using System.Collections.Generic;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    [SerializeField] private Transform _container;

    private Weapon _weapon;
    private GameObject _bulletUIPrefab;

    private readonly List<BulletsAmountUI> _bullets
        = new List<BulletsAmountUI>();

    public void SetWeapon(
        Weapon weapon,
        GameObject bulletUIPrefab)
    {
        // 이전 총 이벤트 해제
        if (_weapon != null)
        {
            _weapon.OnAmmoFired -= HandleFire;
            _weapon.OnReloaded -= HandleReload;
        }

        _weapon = weapon;
        _bulletUIPrefab = bulletUIPrefab;

        // 새 총 이벤트 연결
        _weapon.OnAmmoFired += HandleFire;
        _weapon.OnReloaded += HandleReload;

        CreateAmmoUI();
    }

    private void CreateAmmoUI()
    {
        foreach (BulletsAmountUI bullet in _bullets)
        {
            Destroy(bullet.gameObject);
        }

        _bullets.Clear();

        for (int i = 0; i < _weapon.Magazine; i++)
        {
            GameObject obj = Instantiate(
                _bulletUIPrefab,
                _container
            );

            BulletsAmountUI bullet =
                obj.GetComponent<BulletsAmountUI>();

            _bullets.Add(bullet);
        }

        // 이미 사용한 탄이 있다면 상태 맞추기
        int usedAmmo =
            _weapon.Magazine - _weapon.CurrentAmmo;

        for (int i = 0; i < usedAmmo; i++)
        {
            _bullets[i].Fire();
        }
    }

    private void HandleFire(int index)
    {
        if (index < 0 || index >= _bullets.Count)
            return;

        _bullets[index].Fire();
    }

    private void HandleReload()
    {
        foreach (BulletsAmountUI bullet in _bullets)
        {
            bullet.ResetBullet();
        }
    }

    private void OnDestroy()
    {
        if (_weapon == null)
            return;

        _weapon.OnAmmoFired -= HandleFire;
        _weapon.OnReloaded -= HandleReload;
    }
}