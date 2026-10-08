using UnityEngine;

public class Pistol : Weapon
{
    [SerializeField] private StartUI _startUI;

    protected override void Fire()
    {
        // 옵션창이 열려 있으면 발사하지 않음
        if (_startUI != null && _startUI.IsOptionOpen)
            return;

        CameraController.Instance?.StartPerlinShake(0.15f, 15f, 0.3f);

        Bullet bullet = Instantiate(
            _bulletPrefab,
            _firePoint.position,
            _firePoint.rotation
        );

        bullet.SetWeapon(this);
    }
}