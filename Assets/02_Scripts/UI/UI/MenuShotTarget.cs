using UnityEngine;

public class MenuShotTarget : MonoBehaviour
{
    [SerializeField] private StartUI _startUI;
    [SerializeField] private MenuAction _action;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("PlayerBullet"))
            return;

        _startUI.OnMenuHit(_action);
    }
}