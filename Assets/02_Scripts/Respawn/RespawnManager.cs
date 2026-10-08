using System;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    private static RespawnManager _instance;
    public static RespawnManager Instance => _instance;

    public event Action<Vector3> OnRespawnEvent;
    public event Action OnRespawnUIEvent;

    private Vector3 _respawnPosition;


    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        _respawnPosition = transform.position;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            OnRespawnEvent?.Invoke(_respawnPosition);
            OnRespawnUIEvent?.Invoke();
        }
    }

    public void UpdateCheckpoint(Vector3 checkpointPosition)
    {
        _respawnPosition = checkpointPosition;
    }
}