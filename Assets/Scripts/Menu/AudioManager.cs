using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("SFX Clips")]
    public AudioClip buttonClick;         
    public AudioClip boxDrop;             
    public AudioClip flaskSound;          
    public AudioClip pipeSound;           
    public AudioClip pipeLaunch;          

    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadVolumes();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadVolumes()
    {
        musicSource.volume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        sfxSource.volume = PlayerPrefs.GetFloat(SFXVolumeKey, 1f);
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
        PlayerPrefs.SetFloat(MusicVolumeKey, volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
        PlayerPrefs.SetFloat(SFXVolumeKey, volume);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlayButtonClick() => PlaySFX(buttonClick);
    public void PlayBoxDrop() => PlaySFX(boxDrop);
    public void PlayFlask() => PlaySFX(flaskSound);
    public void PlayPipe() => PlaySFX(pipeSound);
    public void PlayPipeLaunch() => PlaySFX(pipeLaunch);
}