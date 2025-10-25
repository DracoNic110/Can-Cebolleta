using UnityEngine;

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

    private System.Collections.IEnumerator EatingCoroutine()
    {
        float eatTime = Random.Range(10f, 15f);
        Debug.Log($"{clientOwner.name} está comiendo {FoodData.name} durante {eatTime:F1}s...");
        yield return new WaitForSeconds(eatTime);

        Debug.Log($"{clientOwner.name} ha terminado de comer y se levanta.");

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
