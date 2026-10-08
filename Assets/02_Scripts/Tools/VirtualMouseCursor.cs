using UnityEngine;
using UnityEngine.UI;

public class VirtualMouseCursor : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;

    [Header("Cursor Sprite")]
    [SerializeField] private Sprite _normalSprite;
    [SerializeField] private Sprite _clickSprite;

    private RectTransform _rectTransform;
    private Image _image;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _image = GetComponent<Image>();

        _image.sprite = _normalSprite;
    }

    private void Update()
    {
        UpdatePosition();
        UpdateSprite();
    }

    private void UpdatePosition()
    {
        Vector2 screenPosition = VirtualMouse.Instance.ScreenPosition;

        RectTransform canvasRect = _canvas.transform as RectTransform;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, _canvas.worldCamera, out Vector2 localPosition))
        {
            _rectTransform.localPosition = localPosition;
        }
    }

    private void UpdateSprite()
    {
        _image.sprite = Input.GetMouseButton(0) ? _clickSprite : _normalSprite;
    }
}