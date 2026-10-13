using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(Collider2D))]
public class CutsceneTrigger : MonoBehaviour
{
    [SerializeField] private PlayableDirector _director;
    [SerializeField] private float _startCutsceneTime = 0f;

    private bool _isPlayed = false;


    protected virtual void StartCutscene()
    {
        GameManager.Instance?.SetCutsceneMode(true);
        _director.time = _startCutsceneTime;
        _director.Play();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (_isPlayed)
            return;

        if (collider.CompareTag("Player") == false)
            return;

        _isPlayed = true;
        StartCutscene();
    }
}