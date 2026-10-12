using UnityEngine;
using System.Collections.Generic;

// 몬스터 전용 창고
public class EnemyPoolManager : MonoBehaviour
{
    public static EnemyPoolManager Instance;

    [System.Serializable]
    public class Pool
    {
        public string Tag;
        public GameObject Prefab;
        public int Size;
    }

    public List<Pool> Pools;

    // tag 별로 몬스터들이 대기
    private Dictionary<string, Queue<GameObject>> _poolDictionary;

    // 창고가 동났을 때 추가로 찍기 위한 원본 기억
    private Dictionary<string, GameObject> _prefabDictionary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        InitializePools();
    }

    private void Start()
    {
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.OnRespawnUIEvent += ResetEnemy;
        }
    }

    private void OnDestroy()
    {
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.OnRespawnUIEvent -= ResetEnemy;
        }
    }

    private void InitializePools()
    {
        _poolDictionary = new Dictionary<string, Queue<GameObject>>();
        _prefabDictionary = new Dictionary<string, GameObject>();

        foreach (Pool pool in Pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            _prefabDictionary.Add(pool.Tag, pool.Prefab);

            for (int i = 0; i < pool.Size; i++)
            {
                GameObject enemy = Instantiate(pool.Prefab, transform);
                enemy.name = pool.Tag;
                enemy.SetActive(false);

                objectPool.Enqueue(enemy);
            }

            _poolDictionary.Add(pool.Tag, objectPool);
        }
    }

    public GameObject GetEnemy(string tag, Vector3 spawnPosition)
    {
        if (!_poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"[EnemyPoolManager] '{tag}' 이라는 이름의 몬스터 서랍이 없습니다!");
            return null;
        }

        if (_poolDictionary[tag].Count > 0)
        {
            GameObject enemy = _poolDictionary[tag].Dequeue();
            enemy.transform.position = spawnPosition;
            enemy.SetActive(true);
            return enemy;
        }
        else
        {
            GameObject enemy = Instantiate(_prefabDictionary[tag], spawnPosition, Quaternion.identity, transform);
            enemy.name = tag;
            return enemy;
        }
    }

    public void ReturnEnemy(GameObject enemy)
    {
        if (!_poolDictionary.ContainsKey(enemy.name))
        {
            Debug.LogWarning($"[Pool] '{enemy.name}'은(는) 창고 소속이 아니므로 완전히 파괴합니다.");
            Destroy(enemy);
            return;
        }

        enemy.SetActive(false);
        _poolDictionary[enemy.name].Enqueue(enemy);
    }

    public void ResetEnemy()
    {
        // 맵에 살아있는 모든 적을 창고에 반납
        EnemyBase[] activeEnemies = FindObjectsOfType<EnemyBase>();
        foreach (EnemyBase enemy in activeEnemies)
        {
            ReturnEnemy(enemy.gameObject);
        }

        // 스폰 트리거들 초기화
        EnemySpawnTrigger[] triggers = FindObjectsOfType<EnemySpawnTrigger>();
        foreach (EnemySpawnTrigger trigger in triggers)
        {
            trigger.ResetTrigger();
        }

        Debug.Log("<color=green>[EnemyPoolManager] 부활 UI 타이밍에 맞춰 적 리셋 완료!</color>");
    }
}