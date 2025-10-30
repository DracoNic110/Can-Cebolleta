using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    bool gamePaused = false;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] Timer timer;

    public void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Escape) && !gamePaused) && !timer.getGameOver())
        {
            gamePaused = true;
            pauseMenu.SetActive(true);
            Time.timeScale = 0;

            if (SoundsManager.Instance != null)
                SoundsManager.Instance.ReduceVolume("Le Grand Chase");
        }
        else if ((Input.GetKeyDown(KeyCode.Escape) && gamePaused) && !timer.getGameOver())
        {
            gamePaused = false;
            pauseMenu.SetActive(false);
            Time.timeScale = 1;
            if (SoundsManager.Instance != null)
                SoundsManager.Instance.RestoreVolume("Le Grand Chase");
        }
    }


    public void Resume()
    {
        gamePaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        if (SoundsManager.Instance != null)
            SoundsManager.Instance.RestoreVolume("Le Grand Chase");
    }
    public void Home()
    {
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync(0);

        if (SoundsManager.Instance != null) {
            SoundsManager.Instance.StopMusic("Le Grand Chase");
        }
    }

    public void ReStart() 
    {
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync(1);
        
    }

}
 