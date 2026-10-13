using UnityEngine;

public class TrapResetManager : MonoBehaviour
{
    private void Start()
    {
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.OnRespawnPlayerEvent += ResetAllTraps;
        }
    }

    private void OnDestroy()
    {
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.OnRespawnPlayerEvent -= ResetAllTraps;
        }
    }

    private void ResetAllTraps(Vector3 _)
    {
        MonoBehaviour[] behaviours =
            FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IResettable resettable)
            {
                resettable.ResetTrap();
            }
        }
    }
}