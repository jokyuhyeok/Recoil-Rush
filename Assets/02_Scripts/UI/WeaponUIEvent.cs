using System;
using UnityEngine;

public class WeaponUIEvent : MonoBehaviour
{
    private Weapon _weapon;

    public static event Action<Weapon> OnWeaponActivated;
    public static event Action<Weapon> OnWeaponFired;
    public static event Action<Weapon> OnWeaponReloaded;

    private void Awake()
    {
        _weapon = GetComponent<Weapon>();
    }

    private void OnEnable()
    {
        if (_weapon != null)
        {
            OnWeaponActivated?.Invoke(_weapon);
        }
    }

    public void NotifyFire()
    {
        if (_weapon != null)
        {
            OnWeaponFired?.Invoke(_weapon);
        }
    }

    public void NotifyReload()
    {
        if (_weapon != null)
        {
            OnWeaponReloaded?.Invoke(_weapon);
        }
    }
}