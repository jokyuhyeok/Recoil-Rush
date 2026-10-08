using System.Collections.Generic;
using UnityEngine;

public class SpawnTrigger : MonoBehaviour, IResettable
{
    [SerializeField] private GameObject _onOffTarget;
    [SerializeField] private GameObject _monsterPrefab;
    [SerializeField] private Transform[] _monsterSpawnPoints;

    private bool _triggered;
    private List<GameObject> _spawnMonsters = new List<GameObject>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_triggered || !collision.CompareTag("Player"))
        {
            return;
        }

        _triggered = true;
        _onOffTarget.SetActive(true);

        foreach (Transform monsterSpawnPoint in _monsterSpawnPoints)
        {
            _spawnMonsters.Add(Instantiate(_monsterPrefab, monsterSpawnPoint.position, Quaternion.identity));
        }
    }

    public void ResetTrap()
    {
        foreach (GameObject monster in _spawnMonsters)
        {
            if (monster != null)
            {
                Destroy(monster);
            }
        }

        _spawnMonsters.Clear();
        _onOffTarget.SetActive(false);
        _triggered = false;
    }
}
