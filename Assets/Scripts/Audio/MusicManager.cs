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
    [SerializeField] private AudioClip derrotadoClip;
    [SerializeField] private AudioClip jackpotVozClip;
    [SerializeField] private AudioClip losetelaClip;
    [SerializeField] private AudioClip magicSlotClip;
    [SerializeField] private AudioClip menuIradoClip;
    [SerializeField] private AudioClip trashPotClip;
    [SerializeField] private AudioClip vitoriaDubClip;
    [SerializeField] private AudioClip winTelaClip;

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

    public void PlayDerrotadoDub(bool isPlaying)
    {
        if (derrotadoClip == null) return;

        if (isPlaying)
        {
            if (bgmSource.isPlaying) bgmSource.Pause();

            sfxSource.clip = derrotadoClip;
            sfxSource.loop = true;
            sfxSource.Play();
        }
        else
        {
            sfxSource.Stop();

            bgmSource.UnPause();
        }
    }

    public void PlayJackpotVozDub(bool isPlaying)
    {
        if (jackpotVozClip == null) return;

        if (isPlaying)
        {
            if (bgmSource.isPlaying) bgmSource.Pause();

            sfxSource.clip = jackpotVozClip;
            sfxSource.loop = true;
            sfxSource.Play();
        }
        else
        {
            sfxSource.Stop();

            bgmSource.UnPause();
        }
    }

    public void PlayLosetela(bool isPlaying)
    {
        if (losetelaClip == null) return;

        if (isPlaying)
        {
            if (bgmSource.isPlaying) bgmSource.Pause();

            sfxSource.clip = losetelaClip;
            sfxSource.loop = true;
            sfxSource.Play();
        }
        else
        {
            sfxSource.Stop();

            bgmSource.UnPause();
        }
    }

    public void PlayMagicSlot(bool isPlaying)
    {
        if (magicSlotClip == null) return;

        if (isPlaying)
        {
            if (bgmSource.isPlaying) bgmSource.Pause();

            sfxSource.clip = magicSlotClip;
            sfxSource.loop = true;
            sfxSource.Play();
        }
        else
        {
            sfxSource.Stop();

            bgmSource.UnPause();
        }
    }

    public void PlayMenuIrado(bool isPlaying)
    {
        if (menuIradoClip == null) return;

        if (isPlaying)
        {
            if (bgmSource.isPlaying) bgmSource.Pause();

            sfxSource.clip = menuIradoClip;
            sfxSource.loop = true;
            sfxSource.Play();
        }
        else
        {
            sfxSource.Stop();

            bgmSource.UnPause();
        }
    }

    public void PlayTrashPot(bool isPlaying)
    {
        if (trashPotClip == null) return;

        if (isPlaying)
        {
            if (bgmSource.isPlaying) bgmSource.Pause();

            sfxSource.clip = trashPotClip;
            sfxSource.loop = true;
            sfxSource.Play();
        }
        else
        {
            sfxSource.Stop();

            bgmSource.UnPause();
        }
    }

    public void PlayVitoriaDub(bool isPlaying)
    {
        if (vitoriaDubClip == null) return;

        if (isPlaying)
        {
            if (bgmSource.isPlaying) bgmSource.Pause();

            sfxSource.clip = vitoriaDubClip;
            sfxSource.loop = true;
            sfxSource.Play();
        }
        else
        {
            sfxSource.Stop();

            bgmSource.UnPause();
        }
    }

    public void PlayWinTela(bool isPlaying)
    {
        if (winTelaClip == null) return;

        if (isPlaying)
        {
            if (bgmSource.isPlaying) bgmSource.Pause();

            sfxSource.clip = winTelaClip;
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