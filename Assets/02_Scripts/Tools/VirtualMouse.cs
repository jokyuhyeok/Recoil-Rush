using UnityEngine;

[DefaultExecutionOrder(-100)]
public class VirtualMouse : MonoBehaviour
{
    private const float MouseSpeed = 15f;

    public static VirtualMouse Instance { get; private set; }

    [SerializeField] private float _sensitivity = 1f;

    private Vector2 _screenPosition;

    public Vector2 ScreenPosition => _screenPosition;
    public float Sensitivity => _sensitivity;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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

        _screenPosition += mouseDelta * (MouseSpeed * _sensitivity);

        _screenPosition.x = Mathf.Clamp(_screenPosition.x, 0f, Screen.width);
        _screenPosition.y = Mathf.Clamp(_screenPosition.y, 0f, Screen.height);
    }

    public void SetSensitivity(float sensitivity)
    {
        _sensitivity = sensitivity;
    }
}