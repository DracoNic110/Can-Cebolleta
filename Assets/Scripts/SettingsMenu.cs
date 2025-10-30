using UnityEngine;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;
    public void setVolume(float volume)
    {
        audioMixer.SetFloat("masterVolume", volume);
    }

    public void setQuality(int QIndex)
    { 
        QualitySettings.SetQualityLevel(QIndex);
    }
}
