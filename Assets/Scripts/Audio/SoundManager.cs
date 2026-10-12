using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-200)]
public class SoundManager : SingletonBehaviour<SoundManager>
{
    [SerializeField, Range(0f, 1f)] private float _masterVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float _musicVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float _effectVolume = 1f;

    private AudioSource _musicSource;
    private AudioSource _effectSource;
    private Coroutine _musicRoutine;
    private float _musicFade = 1f;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;
        Init();
    }
    private void Init()
    {
        _musicSource = CreateSource("BGM");
        _musicSource.loop = true;
        _effectSource = CreateSource("SFX");
        RefreshVolume();
    }
    private AudioSource CreateSource(string objectName)
    {
        var sourceObject = new GameObject(objectName);
        sourceObject.transform.SetParent(transform, false);
        AudioSource source = sourceObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        return source;
    }

    public void Play(AudioClip clip)
    {
        if (clip == null) return;
        _effectSource.PlayOneShot(clip);
    }
    public void StopEffects() => _effectSource.Stop();

    public void PlayMusic(AudioClip clip) => PlayMusic(clip, 0.5f);
    public void PlayMusic(AudioClip clip, float fadeTime)
    {
        if (clip == null) return;
        if (_musicSource.clip == clip && _musicSource.isPlaying)
        {
            // 전환 중 원래 곡을 다시 요청하면 전환을 취소하고 그대로 이어 간다.
            if (_musicRoutine != null) StopCoroutine(_musicRoutine);
            _musicRoutine = null;
            _musicFade = 1f;
            RefreshVolume();
            return;
        }
        if (_musicRoutine != null) StopCoroutine(_musicRoutine);
        _musicRoutine = StartCoroutine(ChangeMusic(clip, Mathf.Max(0f, fadeTime)));
    }
    private IEnumerator ChangeMusic(AudioClip clip, float duration)
    {
        if (_musicSource.isPlaying) yield return FadeMusic(0f, duration);
        _musicSource.Stop();
        _musicSource.clip = clip;
        _musicFade = duration > 0f ? 0f : 1f;
        RefreshVolume();
        _musicSource.Play();
        yield return FadeMusic(1f, duration);
        _musicRoutine = null;
    }
    private IEnumerator FadeMusic(float target, float duration)
    {
        float start = _musicFade;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _musicFade = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / duration));
            RefreshVolume();
            yield return null;
        }
        _musicFade = target;
        RefreshVolume();
    }
    public void StopMusic()
    {
        if (_musicRoutine != null) StopCoroutine(_musicRoutine);
        _musicRoutine = null;
        _musicSource.Stop();
        _musicSource.clip = null;
    }
    private void RefreshVolume()
    {
        if (_musicSource != null) _musicSource.volume = _masterVolume * _musicVolume * _musicFade;
        if (_effectSource != null) _effectSource.volume = _masterVolume * _effectVolume;
    }
    public void SetMasterVolume(float value)
    {
        _masterVolume = Mathf.Clamp01(value);
        RefreshVolume();
    }
    public void SetMusicVolume(float value)
    {
        _musicVolume = Mathf.Clamp01(value);
        RefreshVolume();
    }
    public void SetEffectVolume(float value)
    {
        _effectVolume = Mathf.Clamp01(value);
        RefreshVolume();
    }
    private void OnValidate() => RefreshVolume();
}
