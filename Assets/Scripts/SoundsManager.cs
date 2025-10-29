using UnityEngine;
using System.Collections.Generic;

public class SoundsManager : MonoBehaviour
{
    public static SoundsManager Instance;

    [Header("Audios")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private List<AudioClip> audioClips;

    private Dictionary<string, AudioClip> clipsDict;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        clipsDict = new Dictionary<string, AudioClip>();
        foreach (var clip in audioClips)
        {
            if (clip != null && !clipsDict.ContainsKey(clip.name))
                clipsDict.Add(clip.name, clip);
        }

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();

        audioSource.loop = false;
        musicSource.loop = false;
        musicSource.playOnAwake = false;
    }

    // Método para reproducir un sonindo con el nombre del clip
    public void PlaySound(string name)
    {
        if (clipsDict.TryGetValue(name, out AudioClip clip))
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    // Método para reproducir música con el nombre del clip
    public void PlayMusic(string name)
    {
        if (clipsDict.TryGetValue(name, out AudioClip clip))
        {
            if (musicSource.clip == clip && musicSource.isPlaying) return;

            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.volume = 0.8f;
            musicSource.Play();
        }
    }

    // Método para reducir el volumen de la música
    public void ReduceVolume(string name)
    {
        if (musicSource.isPlaying && musicSource.clip != null && musicSource.clip.name == name)
            musicSource.volume = 0.3f;
    }

    // Método para restaurar el volumen normal de la música
    public void RestoreVolume(string name)
    {
        if (musicSource.isPlaying && musicSource.clip != null && musicSource.clip.name == name)
            musicSource.volume = 1f;
    }

    // Método para parar la música por completo
    public void StopMusic(string name)
    {
        if (musicSource.isPlaying && musicSource.clip != null && musicSource.clip.name == name)
            musicSource.Stop();
    }
}
