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


        if (bgmSource == null)
            bgmSource = CreateAudioChannel("BGM_Channel", musicGroup, true);
        else
            ConfigureSource(bgmSource, musicGroup, true);

        if (sfxSource == null || sfxSource == bgmSource)
            sfxSource = CreateAudioChannel("SFX_Loop_Channel", sfxGroup, false);
        else
            ConfigureSource(sfxSource, sfxGroup, false);

        sfxOneShotSource = CreateAudioChannel("SFX_OneShot_Channel", sfxGroup, false);
    }

    private AudioSource CreateAudioChannel(string channelName, AudioMixerGroup group, bool loop)
    {
        GameObject child = new GameObject(channelName);
        child.transform.SetParent(transform);
        AudioSource source = child.AddComponent<AudioSource>();
        ConfigureSource(source, group, loop);
        return source;
    }

    private void ConfigureSource(AudioSource source, AudioMixerGroup group, bool loop)
    {
        source.loop = loop;
        source.playOnAwake = false;
        source.volume = 1f;
        source.spatialBlend = 0f; 
        if (group != null) source.outputAudioMixerGroup = group;
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
        if (sfxList == null || sfxList.Count == 0)
        {
            Debug.LogWarning("MusicManager: A lista 'sfxList' está vazia ou nula no Inspector!");
            return;
        }

        foreach (var group in sfxList)
        {
            if (group.sfxName.Equals(name, System.StringComparison.OrdinalIgnoreCase))
            {
                if (group.clips != null && group.clips.Count > 0)
                {
                    int randomIndex = Random.Range(0, group.clips.Count);
                    AudioClip clipToPlay = group.clips[randomIndex];

                    if (clipToPlay != null)
                    {
                        // Mensagem de sucesso no Console para você saber que o código funcionou
                        Debug.Log($"MusicManager: Tocando SFX '{name}' -> Clipe: {clipToPlay.name}");
                        sfxOneShotSource.PlayOneShot(clipToPlay);
                    }
                    else
                    {
                        Debug.LogError($"MusicManager: O slot de áudio no grupo '{name}' está vazio (null)!");
                    }
                    return;
                }
                Debug.LogWarning($"MusicManager: O grupo de SFX '{name}' foi encontrado, mas não contém nenhum áudio na lista.");
                return;
            }
        }
        Debug.LogWarning($"MusicManager: Nenhum SFX com o nome '{name}' foi encontrado na lista.");
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
                    Debug.Log($"MusicManager: Tocando SFX em Loop '{name}'");
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