using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [SerializeField] private float _destroyTime = 0.5f;

    private void Start()
    {
        Destroy(gameObject, _destroyTime);
    }
}
