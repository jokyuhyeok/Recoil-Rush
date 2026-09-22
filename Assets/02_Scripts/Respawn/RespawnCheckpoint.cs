using UnityEngine;

public class RespawnCheckpoint : MonoBehaviour
{
    private bool _isActive = false;


    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (_isActive)
            return;

        if (collider.CompareTag("Player") == false)
            return;

        _isActive = true;
        RespawnManager.Instance.UpdateCheckpoint(transform.position);
    }
}