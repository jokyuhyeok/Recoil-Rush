using UnityEngine;

public class Pistol : Weapon
{
    protected override void Fire()
    {
        Bullet bullet = Instantiate(_bulletPrefab, _firePoint.position, _firePoint.rotation);
        bullet.SetWeapon(this);
    }
}