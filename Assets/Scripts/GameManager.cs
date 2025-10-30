using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

// Controla la UI del juego y el sistema de economía del juego
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Global")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI quotaText;

    [Header("Economía Global")]
    public int totalMoney = 0;
    public int quotaToReach = 150;

    [Header("Prefabs de efectos")]
    public GameObject floatingTextPrefab;

    // Actualizamos la UI al inicio
    private void Start()
    {
        UpdateUI();
    }

    // Inicializamos el singleton
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Lógica para sumar el dinero y actualizamos la UI
    public void AddMoney(int amount)
    {
        totalMoney += amount;
        UpdateUI();
    }

    // Actualizamos la UI con el monto del dinero acumulado y el objetivo a alcanzar
    private void UpdateUI()
    {
        if (moneyText != null)
            moneyText.text = $"Diners: {totalMoney}";
        if (quotaText != null)
            quotaText.text = $"Objectiu: {quotaToReach}";
        

    }

}
