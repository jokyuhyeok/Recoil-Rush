using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance => _instance;

    private bool _isCutscene;
    public bool IsCutscene => _isCutscene;
    public System.Action<bool> OnCutsceneModeChanged;


    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
    }

    public void SetCutsceneMode(bool isCutscene)
    {
        _isCutscene = isCutscene;
        OnCutsceneModeChanged?.Invoke(_isCutscene);

        Debug.Log($"GameManager {_isCutscene}");
    }
}