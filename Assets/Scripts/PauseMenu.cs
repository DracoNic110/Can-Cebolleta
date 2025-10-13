using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    bool gamePaused = false;
    [SerializeField] GameObject pauseMenu; 

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && gamePaused == false)
        {
            gamePaused = true;
            pauseMenu.SetActive(true);
            Time.timeScale = 0;
            

        }
        else if (Input.GetKeyDown(KeyCode.Escape) && gamePaused == true) 
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
        SceneManager.LoadSceneAsync(0);
    }
}
