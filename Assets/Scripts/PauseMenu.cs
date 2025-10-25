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
        if ((Input.GetKeyDown(KeyCode.Escape) && gamePaused == false) && timer.getGameOver() == false)
        {
            gamePaused = true;
            pauseMenu.SetActive(true);
            Time.timeScale = 0;
            

        }
        else if ((Input.GetKeyDown(KeyCode.Escape) && gamePaused == true) && timer.getGameOver() == false) 
        {
            gamePaused = false;
            pauseMenu.SetActive(false);
            Time.timeScale = 1;
            
        }    
    }
    public void Resume()
    {
        gamePaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }
    public void Home()
    {
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync(0);
        
    }

    public void ReStart() 
    {
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync(1);
        
    }

}
 