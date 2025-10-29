using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Runtime.CompilerServices;


public class Timer : MonoBehaviour
{
    [Header("Star Ratings")]
    [SerializeField] GameManager Gmanager;
    [SerializeField] float score;

    [SerializeField] Star[] Stars;
    [SerializeField] float EnlargeScale = 1.5f;
    [SerializeField] float ShrinkScale = 1.0f;
    [SerializeField] float EnlargeDuration = 0.25f;
    [SerializeField] float ShrinkDuration = 0.25f;
    

    [Header("Timer Display")]
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] float remainingTime;
    public bool gameOver = false;
    [SerializeField] GameObject gameOverScreen;

    private IEnumerator ChangeStarScale(Star star, float targetScale, float duration)
    {
        Vector3 initialScale = star.YellowStar.transform.localScale;
        Vector3 finalScale = new Vector3(targetScale, targetScale, targetScale);

        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            star.YellowStar.transform.localScale = Vector3.Lerp(initialScale, finalScale, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        finalScale = new Vector3(1.0f,1.0f,1.0f);
    }
    private IEnumerator EnlargeAndShrinkStar(Star star)
    {
        yield return StartCoroutine(ChangeStarScale(star, EnlargeScale, EnlargeDuration));
        yield return StartCoroutine(ChangeStarScale(star, ShrinkScale, ShrinkDuration));
    }

    private IEnumerator ShowStarsRoutine(int numStars)
    {

        for (int i = 0; i < numStars; i++)
        {
            yield return StartCoroutine(EnlargeAndShrinkStar(Stars[i]));
        }
    }
    public void ShowStars(int numStars)
    {
        StartCoroutine(ShowStarsRoutine(numStars));
    }

    public bool getGameOver() { 
        return gameOver;
    }


    public float calculateScore() {
        float money = Gmanager.totalMoney;
        float quota = Gmanager.quotaToReach;

        return ((money / quota) * 5);
    }

    void endGame()
    {
        gameOver = true;
        gameOverScreen.SetActive(true);
        timerText.text = "Closed";

        if (SoundsManager.Instance != null) {
            SoundsManager.Instance.StopMusic("Le Grand Chase");
            SoundsManager.Instance.PlayMusic("Long Stroll");
        }
        score = calculateScore();

        ShowStars(Mathf.RoundToInt(score));
        
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
