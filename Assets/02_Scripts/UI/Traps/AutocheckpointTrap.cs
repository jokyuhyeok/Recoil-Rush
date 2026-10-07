using UnityEngine;

public class AutoCheckpoint : MonoBehaviour
{
    [SerializeField] private Transform _respawnPoint;

    private bool _activated;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_activated)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (RespawnManager.Instance == null)
            return;

        _activated = true;

        RespawnManager.Instance.UpdateCheckpoint(_respawnPoint.position);

        Debug.Log($"[AutoCheckpoint] 체크포인트 저장: {_respawnPoint.position}");
    }
}