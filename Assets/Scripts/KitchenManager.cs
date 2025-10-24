using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KitchenManager : MonoBehaviour
{
    [Header("Cocina")]
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float defaultMinCookTime = 10f;
    [SerializeField] private float defaultMaxCookTime = 15f;

    private List<CookingOrder> activeOrders = new List<CookingOrder>();

    public void PrepareOrder(Food food)
    {
        Transform spawnPoint = GetAvailableSpawnPoint();

        if (spawnPoint == null)
        {
            Debug.LogWarning("No hay puntos de spawn disponibles para un nuevo pedido.");
            return;
        }
        CookingOrder order = new CookingOrder(food, spawnPoint);
        activeOrders.Add(order);

        StartCoroutine(CookRoutine(order));

        Debug.Log($"Pedido recibido: {food.name}");
    }

    private IEnumerator CookRoutine(CookingOrder order)
    {
        float cookTime = Random.Range(
            order.food.minCookTime > 0 ? order.food.minCookTime : defaultMinCookTime,
            order.food.maxCookTime > 0 ? order.food.maxCookTime : defaultMaxCookTime
        );

        Debug.Log($"Cocinando {order.food.name} durante {cookTime:F1} segundos...");
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

        Debug.Log($"Pedido listo: {order.food.name}");
    }


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

    public void RemoveOrder(GameObject foodObj)
    {
        for (int i = activeOrders.Count - 1; i >= 0; i--)
        {
            if (activeOrders[i].instance == foodObj)
            {
                Debug.Log($"Pedido {activeOrders[i].food.name} retirado de la barra.");
                Destroy(foodObj);
                activeOrders.RemoveAt(i);
            }
        }
    }
}
