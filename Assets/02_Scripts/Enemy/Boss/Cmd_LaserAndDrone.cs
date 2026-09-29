using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cmd_LaserAndDrone : IBossCommand
{
    private float _chargeDuration;
    private float _laserDuration;
    private float _droneSpawnDelay;
    private int _numberOfDrones;

    // 생성자입니다.
    public Cmd_LaserAndDrone(float chargeDuration, float laserDuration, float droneSpawnDelay, int numberOfDrones)
    {
        _chargeDuration = chargeDuration;
        _laserDuration = laserDuration;
        _droneSpawnDelay = droneSpawnDelay;
        _numberOfDrones = numberOfDrones;
    }

    public IEnumerator Execute(BossController boss)
    {
        Debug.Log("<color=magenta>보스: 레이저 포신 장전 (경고!)</color>");
        
        // [연출] 경고선을 켜는 코드 자리입니다.
        
        yield return new WaitForSeconds(_chargeDuration);   // 장전 시간동안 대기합니다.
        
        // [연출] 경고선을 끕니다.
        
        Debug.Log("<color=magenta>보스: 레이저 발사!</color>");

        if (boss.LaserBeamObject != null)
        {
            boss.LaserBeamObject.SetActive(true);
        }
        
        Debug.Log($"<color=magenta>보스: 레이저 발사 유지 중.. {_droneSpawnDelay}초 뒤에 드론이 등장합니다.</color>");

        yield return new WaitForSeconds(_droneSpawnDelay);
        
        SpawnRandomDrones(boss);

        float remainingLaserTime = Mathf.Max(0, _laserDuration - _droneSpawnDelay);
        yield return new WaitForSeconds(remainingLaserTime);
        
        Debug.Log("<color=magenta>보스: 레이저 발사 종료 후 잠시 쉽니다.</color>");

        if (boss.LaserBeamObject != null)
        {
            boss.LaserBeamObject.SetActive(false);
        }

        yield return new WaitForSeconds(1f);
    }

    private void SpawnRandomDrones(BossController boss)
    {
        // 배치해둔 여러개의 드론 생성 포인트 중에 랜덤으로 3개의 위치에서 생성합니다.
        Transform[] spawnPoints = boss.DroneSpawnPoints;
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        List<int> availableIndexes = new List<int>();
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            availableIndexes.Add(i);
        }

        int spawnCount = Mathf.Min(_numberOfDrones, spawnPoints.Length);

        for (int i = 0; i < spawnCount; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableIndexes.Count);
            int selectedIndexes = availableIndexes[randomIndex];

            Transform spawnPoint = spawnPoints[selectedIndexes];
            Object.Instantiate(boss.DronePrefab, spawnPoint.position, Quaternion.identity);
            
            availableIndexes.RemoveAt(randomIndex);
        }
        
        Debug.Log($"<color=magenta>보스: {spawnCount}개의 드론 랜덤 생성 완료</color>");
    }
}
