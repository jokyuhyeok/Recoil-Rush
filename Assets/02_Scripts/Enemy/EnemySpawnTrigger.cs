using UnityEngine;
using System.Collections.Generic;

// 어떤 몬스터를 어디 위치에 소환할 것인가?
public class EnemySpawnTrigger : MonoBehaviour
{
    [System.Serializable]
    public class SpawnData
    {
        public string EnemyTag;
        public Transform SpawnPoint;
    }

    [SerializeField] private List<SpawnData> _spawnList;

    private bool _hasSpawned = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_hasSpawned || !collision.CompareTag("Player")) return;

        _hasSpawned = true;

        Debug.Log($"{gameObject.name} 구역 진입: 몬스터 소환!");

        foreach (SpawnData data in _spawnList)
        {
            if (data.SpawnPoint == null)
            {
                Debug.LogError("소환될 위치가 없습니다...");
            }

            EnemyPoolManager.Instance.GetEnemy(data.EnemyTag, data.SpawnPoint.position);
        }
    }

    public void ResetTrigger()
    {
        _hasSpawned = false;
    }
}