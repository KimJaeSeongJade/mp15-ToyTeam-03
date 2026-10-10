using UnityEngine;

public class SceneSoundPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip _music;
    [SerializeField, Min(0f)] private float _fadeTime = 0.5f;

    private void Start() => PlayMusic();
    public void PlayMusic()
    {
        if (SoundManager.Instance == null) return;
        SoundManager.Instance.PlayMusic(_music, _fadeTime);
    }
}
