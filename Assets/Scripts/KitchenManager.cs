using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Gestiona la cocina del restaurante
public class KitchenManager : MonoBehaviour
{
    [Header("Cocina")]
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float defaultMinCookTime = 10f;
    [SerializeField] private float defaultMaxCookTime = 15f;

    private List<CookingOrder> activeOrders = new List<CookingOrder>();

    // Inicia la preparación de una orden de comida.
    public void PrepareOrder(Food food)
    {
        Transform spawnPoint = GetAvailableSpawnPoint();

        if (spawnPoint == null)
        {
            return;
        }
        CookingOrder order = new CookingOrder(food, spawnPoint);
        activeOrders.Add(order);

        StartCoroutine(CookRoutine(order));
    }

    // Rutina para cocinar una orden
    private IEnumerator CookRoutine(CookingOrder order)
    {
        float cookTime = Random.Range(
            order.food.minCookTime > 0 ? order.food.minCookTime : defaultMinCookTime,
            order.food.maxCookTime > 0 ? order.food.maxCookTime : defaultMaxCookTime
        );
        yield return new WaitForSeconds(cookTime);

        GameObject foodObj = Instantiate(foodPrefab, order.spawnPoint.position, Quaternion.identity);

        SpriteRenderer sr = foodObj.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sprite = order.food.dishSprite;

        Dish dishComp = foodObj.GetComponent<Dish>();
        if (dishComp == null)
            dishComp = foodObj.AddComponent<Dish>();
        dishComp.Initialize(order.food);

        order.ready = true;
        order.instance = foodObj;
    }

    // Método para aignar un spawnPoint para el pedido
    private Transform GetAvailableSpawnPoint()
    {
        foreach (Transform point in spawnPoints)
        {
            bool occupied = false;
            foreach (CookingOrder order in activeOrders)
            {
                if (order.spawnPoint == point)
                {
                    occupied = true;
                    break;
                }
            }

            if (!occupied)
                return point;
        }

        return null;
    }


    // Elimina un plato de la cocina
    public void RemoveOrder(GameObject foodObj)
    {
        for (int i = activeOrders.Count - 1; i >= 0; i--)
        {
            if (activeOrders[i].instance == foodObj)
            {
                Destroy(foodObj);
                activeOrders.RemoveAt(i);
            }
        }
    }
}
