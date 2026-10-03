using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStat", menuName = "Enemy/Stat/Rifle")]
public class RifleStatDataSO : EnemyStatDataSO
{
    [Header("소총수 전용 데이터")] public float FireRate = 1f;

    [Header("사격 및 AI 밸런스")] [Tooltip("총을 쏜 반동에 밀려날 힘의 정도입니다.")]
    public float ShootRecoilPower = 3f;

    [Tooltip("Player와 Y좌표가 일정 이상의 차이가 나면 사격을 못하게 할지에 대한 수치입니다.")]
    public float ShootHeightDifference = 3f;
}