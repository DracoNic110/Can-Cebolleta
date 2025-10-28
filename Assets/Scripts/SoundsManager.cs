using UnityEngine;
using System.Collections.Generic;

public class SoundsManager : MonoBehaviour
{
    public static SoundsManager Instance;

    [Header("Audios")]
    [SerializeField] private AudioSource audioSource;
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
    }

    public void PlaySound(string name)
    {
        if (clipsDict.TryGetValue(name, out AudioClip clip))
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"🔊 SoundsManager: no se encontró el clip '{name}'");
        }
    }
}
