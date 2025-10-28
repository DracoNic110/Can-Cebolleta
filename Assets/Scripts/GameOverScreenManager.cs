using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GameOverScreenManager : MonoBehaviour
{

    [SerializeField] Star[] Stars;
    [SerializeField] float EnlargeScale = 1.5f;
    [SerializeField] float ShrinkScale = 1.0f;
    [SerializeField] float EnlargeDuration = 0.25f;
    [SerializeField] float ShrinkDuration = 0.25f;
    [SerializeField] GameManager Gmanager;

    

    void Start()
    {
        int score = Gmanager.score;
        ShowStars(score);
    }

    public void ShowStars(int numStars)
    {
        StartCoroutine(ShowStarsRoutine(numStars));
    }

    private IEnumerator ShowStarsRoutine(int numStars)
    {
        foreach (Star star in Stars)
        {
            star.YellowStar.transform.localScale = Vector3.zero;
        }
        for (int i = 0; i < numStars; i++)
        {
            yield return StartCoroutine(EnlargeAndShrinkStar(Stars[i]));
        }
    }

    private IEnumerator EnlargeAndShrinkStar(Star star)
    {
        yield return StartCoroutine(ChangeStarScale(star, EnlargeScale, EnlargeDuration));
        yield return StartCoroutine(ChangeStarScale(star, ShrinkScale, ShrinkDuration));
    }

    private IEnumerator ChangeStarScale(Star star, float targetScale, float duration)
    {
        Vector3 initialScale = star.YellowStar.transform.localScale;
        Vector3 finalScale = new Vector3(targetScale, targetScale, targetScale);

        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            star.YellowStar.transform.localScale = Vector3.Lerp(initialScale, finalScale, (elapsedTime/duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        star.YellowStar.transform.localScale = finalScale;

    }

}
