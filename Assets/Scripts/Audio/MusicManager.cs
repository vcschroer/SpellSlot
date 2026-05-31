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

[System.Serializable]
public struct SFXGroup
{
    public string sfxName;
    public List<AudioClip> clips;
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
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private List<SFXGroup> sfxList;

    private AudioSource sfxOneShotSource;

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

        if (sfxSource == null || sfxSource == bgmSource)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        if (sfxGroup != null) sfxSource.outputAudioMixerGroup = sfxGroup;

        if (sfxOneShotSource == null || sfxOneShotSource == bgmSource || sfxOneShotSource == sfxSource)
        {
            sfxOneShotSource = gameObject.AddComponent<AudioSource>();
        }
        sfxOneShotSource.loop = false;
        sfxOneShotSource.playOnAwake = false;
        if (sfxGroup != null) sfxOneShotSource.outputAudioMixerGroup = sfxGroup;
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
        if (isPlaying)
        {
            PlaySFX("rolljackpotsound");
            PlayMusicManual("jackpotsong");
        }
        else
        {
            PlayMusicForScene(SceneManager.GetActiveScene().name);
        }
    }

    public void PlaySFX(string name)
    {
        if (sfxList == null) return;

        foreach (var group in sfxList)
        {
            if (group.sfxName.Equals(name, System.StringComparison.OrdinalIgnoreCase))
            {
                if (group.clips != null && group.clips.Count > 0)
                {
                    int randomIndex = Random.Range(0, group.clips.Count);

                    sfxOneShotSource.PlayOneShot(group.clips[randomIndex]);
                    return;
                }
            }
        }
    }

    public void PlayLoopingSFX(string name)
    {
        if (sfxList == null) return;

        foreach (var group in sfxList)
        {
            if (group.sfxName.Equals(name, System.StringComparison.OrdinalIgnoreCase))
            {
                if (group.clips != null && group.clips.Count > 0)
                {
                    sfxSource.clip = group.clips[0];
                    sfxSource.loop = true;
                    sfxSource.Play();
                    return;
                }
            }
        }
    }

    public void StopLoopingSFX() => sfxSource.Stop();

    public void PlayMusicManual(string trackIdentifier)
    {
        foreach (var track in musicList)
        {
            if (track.sceneName.Equals(trackIdentifier, System.StringComparison.OrdinalIgnoreCase))
            {
                if (bgmSource.clip == track.clip && bgmSource.isPlaying) return;
                bgmSource.clip = track.clip;
                bgmSource.Play();
                return;
            }
        }
    }
}