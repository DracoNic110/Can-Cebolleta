using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void Start()
    {
        if (SoundsManager.Instance != null)
            SoundsManager.Instance.PlayMusic("Hackbeat");
    }

    public void PlayGame() 
    {
        if (SoundsManager.Instance != null)
        {
            SoundsManager.Instance.StopMusic("Hackbeat");
            SoundsManager.Instance.PlayMusic("Le Grand Chase");
        }

        SceneManager.LoadSceneAsync(1);
    }

    public void QuitGame() 
    {
        Application.Quit();
    }
}
