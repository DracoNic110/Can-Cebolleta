using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Dish : MonoBehaviour
{
    public ClientBehavior clientOwner;
    public Food FoodData;

    [Header("Configuración del restaurante")]
    public Transform exitPoint;

    public void AssignOrder(ClientBehavior client)
    {
        clientOwner = client;
        FoodData = client.CurrentOrder;
    }

    public void Initialize(Food food)
    {
        FoodData = food;
    }

    public void StartEatingRoutine()
    {
        StartCoroutine(EatingCoroutine());
    }

    private IEnumerator EatingCoroutine()
    {
        float eatTime = Random.Range(10f, 15f);
        yield return new WaitForSeconds(eatTime);

        clientOwner.GetComponent<ClientSatisfaction>()?.OnFinishedEating();

        if (exitPoint == null)
        {
            GameObject exit = GameObject.Find("ClientPoints/ExitPoint");
            if (exit != null)
                exitPoint = exit.transform;
        }

        if (exitPoint != null)
            clientOwner.LeaveRestaurant(exitPoint.position);

        Destroy(gameObject);
    }
}
