using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    private static RespawnManager _instance;
    public static RespawnManager Instance => _instance;

    public delegate void RespawnEvent(Vector3 respawnPosition);
    public event RespawnEvent OnRespawnEvent;

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
        }
    }

    public void UpdateCheckpoint(Vector3 checkpointPosition)
    {
        _respawnPosition = checkpointPosition;
    }
}