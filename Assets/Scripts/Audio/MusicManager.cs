using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using System.Collections.Generic;

[System.Serializable]
public struct MusicTrack
{
    public string sceneName;
    public AudioClip clip;
}

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Configurações de BGM (Música)")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private List<MusicTrack> musicList;

    [Header("Configurações de SFX (Efeitos)")]
    [SerializeField] private AudioSource sfxSource; 
    [SerializeField] private AudioClip jackpotClip;
    [SerializeField] private AudioMixerGroup sfxGroup;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        if (musicGroup != null) bgmSource.outputAudioMixerGroup = musicGroup;

        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = true;
        sfxSource.playOnAwake = false;
        if (sfxGroup != null) sfxSource.outputAudioMixerGroup = sfxGroup;
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => PlayMusicForScene(scene.name);

    private void PlayMusicForScene(string sceneName)
    {
        foreach (var track in musicList)
        {
            if (track.sceneName == sceneName)
            {
                if (bgmSource.clip == track.clip && bgmSource.isPlaying) return;
                bgmSource.clip = track.clip;
                bgmSource.Play();
                return;
            }
        }
        bgmSource.Stop();
    }

    public void PlayJackpotSound(bool isPlaying)
    {
        if (jackpotClip == null) return;

        if (isPlaying)
        {
            if (bgmSource.isPlaying) bgmSource.Pause();

            sfxSource.clip = jackpotClip;
            sfxSource.loop = true;
            sfxSource.Play();
        }
        else
        {
            sfxSource.Stop();

            bgmSource.UnPause();
        }
    }
}