using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    public void SetMusicVolume(float sliderValue)
    {
        float volume = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat("ExposedParamMusic", volume);
        PlayerPrefs.SetFloat("ExposedParamMusic", sliderValue); 
    }

    public void SetSFXVolume(float sliderValue)
    {
        float volume = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat("ExposedParamSFX", volume);
        PlayerPrefs.SetFloat("ExposedParamSFX", sliderValue);
    }

    void Start()
    {
        float musicVol = PlayerPrefs.GetFloat("ExposedParamMusic", 1f);
        float sfxVol = PlayerPrefs.GetFloat("ExposedParamSFX", 1f);

        SetMusicVolume(musicVol);
        SetSFXVolume(sfxVol);
    }
}
