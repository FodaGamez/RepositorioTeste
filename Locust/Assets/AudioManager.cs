using UnityEngine;
using System.Collections.Generic;
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    private AudioSource systemSource;
    private List<AudioSource> activeSources;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
            
    }
    
    //Funções de gerenciamento de áudio 2D
   
    public void Playsound(AudioClip clip)
    {
        systemSource.Stop();
        systemSource.clip = clip;
        systemSource.Play();
    }

    public void StopSound()
    {
        systemSource.Stop();
    }
    
    public void PauseSound()
    {
        systemSource.Pause();
    }
    
    public void ResumeSound()
    {
        systemSource.UnPause();
    }
    
    public void PlayOneShot(AudioClip clip)
    {
        systemSource.PlayOneShot(clip);
    }
    
    //Funções de gerenciaimento de áudio 3D

    public void PlaySound3d(AudioClip clip, AudioSource source)
    {
        if (!activeSources.Contains(source)) activeSources.Add(source);
        
        source.Stop();
        source.clip = clip;
        source.Play();
    }
    
    public void StopSound3d(AudioSource source)
    {
        systemSource.Stop();
        activeSources.Remove(source);
    }
    
    public void PauseSound3d(AudioSource source)
    {
        systemSource.Pause();
    }
    
    public void ResumeSound3d(AudioSource source)
    {
        systemSource.UnPause();
    }
}
