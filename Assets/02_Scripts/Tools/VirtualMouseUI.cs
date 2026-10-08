using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualMouseUI : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform _cursor;

    private EventSystem _eventSystem;
    private PointerEventData _pointerData;

    private readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();

    private GameObject _hoveredObject;


    private void Start()
    {
        _eventSystem = EventSystem.current;
        _pointerData = new PointerEventData(_eventSystem);
    }

    private void Update()
    {
        Vector2 previousPosition = _pointerData.position;
        Vector2 currentPosition = VirtualMouse.Instance.ScreenPosition;

        _pointerData.position = currentPosition;
        _pointerData.delta = currentPosition - previousPosition;
        _pointerData.button = PointerEventData.InputButton.Left;

        UpdateCursorPosition(currentPosition);
        UpdateRaycast();
        UpdateHover();

        if (Input.GetMouseButtonDown(0))
        {
            PointerDown();
        }

        if (Input.GetMouseButton(0))
        {
            PointerDrag();
        }

        if (Input.GetMouseButtonUp(0))
        {
            PointerUp();
        }
    }

    private void UpdateCursorPosition(Vector2 screenPosition)
    {
        RectTransform canvasRect = _canvas.transform as RectTransform;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, _canvas.worldCamera, out Vector2 localPosition))
        {
            _cursor.localPosition = localPosition;
        }
    }

    private void UpdateRaycast()
    {
        _raycastResults.Clear();

        _eventSystem.RaycastAll(_pointerData, _raycastResults);

        _pointerData.pointerCurrentRaycast = _raycastResults.Count > 0 ? _raycastResults[0] : new RaycastResult();
    }

    private void UpdateHover()
    {
        GameObject currentObject = _pointerData.pointerCurrentRaycast.gameObject;

        GameObject newHoveredObject = currentObject != null ? ExecuteEvents.GetEventHandler<IPointerEnterHandler>(currentObject) : null;

        if (_hoveredObject == null || _hoveredObject == newHoveredObject)
        {
            return;
        }

        ExecuteEvents.Execute(_hoveredObject, _pointerData, ExecuteEvents.pointerExitHandler);

        _hoveredObject = newHoveredObject;

        if (_hoveredObject != null)
        {
            ExecuteEvents.Execute(_hoveredObject, _pointerData, ExecuteEvents.pointerEnterHandler);
        }
    }

    private void PointerDown()
    {
        GameObject currentObject = _pointerData.pointerCurrentRaycast.gameObject;

        if (currentObject == null)
        {
            return;
        }

        _pointerData.pressPosition = _pointerData.position;
        _pointerData.pointerPressRaycast = _pointerData.pointerCurrentRaycast;
        _pointerData.eligibleForClick = true;
        _pointerData.dragging = false;

        GameObject pressedObject = ExecuteEvents.ExecuteHierarchy(currentObject, _pointerData, ExecuteEvents.pointerDownHandler);

        if (pressedObject == null)
        {
            pressedObject = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentObject);
        }

        _pointerData.pointerPress = pressedObject;
        _pointerData.rawPointerPress = currentObject;

        GameObject dragObject = ExecuteEvents.GetEventHandler<IDragHandler>(currentObject);

        _pointerData.pointerDrag = dragObject;

        if (dragObject != null)
        {
            ExecuteEvents.Execute(dragObject, _pointerData, ExecuteEvents.initializePotentialDrag);
        }
    }

    private void PointerDrag()
    {
        if (_pointerData.pointerDrag == null)
        {
            return;
        }

        if (!_pointerData.dragging)
        {
            float dragThreshold = _eventSystem.pixelDragThreshold;

            if ((_pointerData.position - _pointerData.pressPosition).sqrMagnitude < dragThreshold * dragThreshold)
            {
                return;
            }

            ExecuteEvents.Execute(_pointerData.pointerDrag, _pointerData, ExecuteEvents.beginDragHandler);

            _pointerData.dragging = true;
            _pointerData.eligibleForClick = false;
        }

        ExecuteEvents.Execute(_pointerData.pointerDrag, _pointerData, ExecuteEvents.dragHandler);
    }

    private void PointerUp()
    {
        if (_pointerData.pointerPress == null)
            return;

        ExecuteEvents.Execute(_pointerData.pointerPress, _pointerData, ExecuteEvents.pointerUpHandler);

        GameObject currentObject = _pointerData.pointerCurrentRaycast.gameObject;

        GameObject clickObject = currentObject != null ? ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentObject) : null;

        if (_pointerData.pointerPress == clickObject && _pointerData.eligibleForClick)
        {
            ExecuteEvents.Execute(_pointerData.pointerPress, _pointerData, ExecuteEvents.pointerClickHandler);
        }

        if (_pointerData.pointerDrag != null && _pointerData.dragging)
        {
            ExecuteEvents.Execute(_pointerData.pointerDrag, _pointerData, ExecuteEvents.endDragHandler);
        }

        _pointerData.eligibleForClick = false;
        _pointerData.dragging = false;
        _pointerData.pointerPress = null;
        _pointerData.rawPointerPress = null;
        _pointerData.pointerDrag = null;
    }
}