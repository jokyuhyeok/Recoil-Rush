using UnityEngine;

public class DoorOpenTrigger : MonoBehaviour
{
    [SerializeField] private DoorAutoMove _door;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _door.Open();
        }
    }
}
