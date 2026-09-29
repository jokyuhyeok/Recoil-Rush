using UnityEngine;

public class Pistol : Weapon
{
    protected override void Fire()
    {
        CameraController.Instance.StartPerlinShake(0.15f, 15f, 0.3f);

        Bullet bullet = Instantiate(_bulletPrefab, _firePoint.position, _firePoint.rotation);
        bullet.SetWeapon(this);
    }
}