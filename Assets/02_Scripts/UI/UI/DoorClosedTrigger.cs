using UnityEngine;

public class DoorClosedTrigger : MonoBehaviour
{
    [SerializeField] private DoorAutoMove _door;

    private bool _triggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_triggered)
            return;

        if (collision.CompareTag("Player"))
        {
            _triggered = true;
            _door.CloseAndLock();
        }
    }
}