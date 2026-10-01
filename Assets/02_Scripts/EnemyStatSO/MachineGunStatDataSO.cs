using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStat", menuName = "Enemy/Stat/MachineGun")]
public class MachineGunStatDataSO : EnemyStatDataSO
{
    [Header("기관총 전용 데이터")]
    public float FireRate = 1f;

    public int MagazineSize = 1;
    public float ReloadTime = 1f;
}