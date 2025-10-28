using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Timer : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] float remainingTime;
    public bool gameOver = false;
    [SerializeField] GameObject gameOverScreen;

    public bool getGameOver() { 
        return gameOver;
    }

    void endGame()
    {
        gameOver = true;
        Time.timeScale = 0;
        gameOverScreen.SetActive(true);
        timerText.text = "Closed";

        if (SoundsManager.Instance != null) {
            SoundsManager.Instance.StopMusic("Le Grand Chase");
            SoundsManager.Instance.PlayMusic("Long Stroll");
        }
            

    }

    void Start() {
        Time.timeScale = 1;
        if (SoundsManager.Instance != null)
            SoundsManager.Instance.PlayMusic("Le Grand Chase");
    }

    // Update is called once per frame
    void Update()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);


        if (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        else if (remainingTime < 0)
        {
            remainingTime = 0;
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            endGame();
        }
        if (remainingTime < 1)
        {
            timerText.color = Color.red;
        }
    }
}
