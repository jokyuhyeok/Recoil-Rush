using UnityEngine;

public class SignalHandler : MonoBehaviour
{
    public void SetCutsceneMode(bool isCutsceneMode)
    {
        GameManager.Instance?.SetCutsceneMode(isCutsceneMode);
    }
}