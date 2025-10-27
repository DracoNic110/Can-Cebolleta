using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Global")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI quotaText;

    [Header("Economía Global")]
    public int totalMoney = 0;
    public int quotaToReach = 150;

    private void Start()
    {
        UpdateUI();
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddMoney(int amount)
    {
        totalMoney += amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (moneyText != null)
            moneyText.text = $"Money: {totalMoney}";
        if (quotaText != null)
            quotaText.text = $"Quota: {quotaToReach}";
    }
}
