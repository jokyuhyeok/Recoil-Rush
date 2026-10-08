using System;
using UnityEngine;

public class VirtualMouse : MonoBehaviour
{
    private static VirtualMouse _instance;
    public static VirtualMouse Instance => _instance;

    [SerializeField] private float _mouseSpeed = 10f;
    [SerializeField] private float _sensitivity = 1f;
    public float Sensitivity => _sensitivity;

    private Vector2 _screenPosition;
    public Vector2 ScreenPosition => _screenPosition;


    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        _screenPosition = Input.mousePosition;
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        _sensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
    }

    private void Update()
    {
        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        _screenPosition += mouseDelta * _mouseSpeed * _sensitivity;

        _screenPosition.x = Mathf.Clamp(_screenPosition.x, 0f, Screen.width);
        _screenPosition.y = Mathf.Clamp(_screenPosition.y, 0f, Screen.height);
    }

    public void SetSensitivity(float sensitivity)
    {
        _sensitivity = sensitivity;
    }
}