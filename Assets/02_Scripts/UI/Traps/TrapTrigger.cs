using UnityEngine;
using UnityEngine.Events;

public class TrapTrigger : MonoBehaviour, IResettable
{
    [SerializeField] private string _targetTag = "Player";
    [SerializeField] private bool _triggerOnce = true;

    [Header("After Trigger")]
    [SerializeField] private bool _disableAfterTrigger = true;

    [Header("Event")]
    [SerializeField] private UnityEvent _onTriggered;

    private bool _triggered;
    private Collider2D _triggerCollider;

    private void Awake()
    {
        _triggerCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("★ 트리거 접촉: " + collision.name);

        if (!collision.CompareTag(_targetTag))
        {
            Debug.Log("★ 태그 불일치: " + collision.tag);
            return;
        }

        if (_triggerOnce && _triggered)
            return;

        Debug.Log("★ GenericTrigger 발동!");

        _triggered = true;

        _onTriggered?.Invoke();

        if (_disableAfterTrigger && _triggerCollider != null)
        {
            _triggerCollider.enabled = false;
        }
    }
    public void ResetTrap()
    {
        _triggered = false;

        if (_triggerCollider != null)
        {
            _triggerCollider.enabled = true;
        }
    }
}