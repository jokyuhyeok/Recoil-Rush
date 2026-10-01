using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStat", menuName = "Enemy/Stat/Rifle")]
public class RifleStatDataSO : EnemyStatDataSO
{
    [Header("소총수 전용 데이터")]
    public float fireRate = 1f;
}
