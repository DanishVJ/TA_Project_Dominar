using UnityEngine;

public class BGMManager : Singleton<BGMManager>
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource musicAudioSource;

    [Header("Music Tracks")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameplayMusic;

    public void PlayMenuMusic()
    {
        PlayTrack(menuMusic);
    }

    public void PlayGameplayMusic()
    {
        PlayTrack(gameplayMusic);
    }

    private void PlayTrack(AudioClip clip)
    {
        if (clip == null) return;

        // If the exact track is already playing, don't restart it
        if (musicAudioSource.isPlaying && musicAudioSource.clip == clip) return;

        musicAudioSource.clip = clip;
        musicAudioSource.loop = true;
        musicAudioSource.Play();
    }
}