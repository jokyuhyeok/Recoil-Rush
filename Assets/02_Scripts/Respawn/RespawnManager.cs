using System;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    private static RespawnManager _instance;
    public static RespawnManager Instance => _instance;

    public event Action<Vector3> OnRespawnPlayerEvent;
    public event Action OnRespawnEvent;

    private Vector3 _respawnPosition;

    private GameManager _gameManager;


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

    private void Start()
    {
        if (GameManager.Instance == null)
            return;

        _gameManager = GameManager.Instance;
    }

    private void Update()
    {
        if (_gameManager.IsCutscene)
            return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            OnRespawnPlayerEvent?.Invoke(_respawnPosition);
            OnRespawnEvent?.Invoke();
        }
    }

    public void UpdateCheckpoint(Vector3 checkpointPosition)
    {
        _respawnPosition = checkpointPosition;
    }
}