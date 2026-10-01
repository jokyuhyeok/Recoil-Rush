using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStat", menuName = "Enemy/Stat/MachineGun")]
public class MachineGunStatDataSO : EnemyStatDataSO
{
    [Header("기관총 전용 데이터")]
    public float fireRate = 1f;
    public int magazineSize = 1;
    public float reloadTime = 1f;
}
