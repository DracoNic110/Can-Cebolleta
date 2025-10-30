using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Clase que gestiona y establece la relación con el plato de comida con el cliente que lo ha pedido
public class Dish : MonoBehaviour
{
    public ClientBehavior clientOwner;
    public Food FoodData;

    [Header("Configuración del restaurante")]
    public Transform exitPoint;

    // Asignamos el plato con la orden del cliente
    public void AssignOrder(ClientBehavior client)
    {
        clientOwner = client;
        FoodData = client.CurrentOrder;
    }

    //  Inicializa el plato directamente con un objeto Food.
    public void Initialize(Food food)
    {
        FoodData = food;
    }

    // Comenzamos la rutina de comer
    public void StartEatingRoutine()
    {
        StartCoroutine(EatingCoroutine());
    }

    // Con esta lógica implementamos la rutina de comer del cliente para determinar el tiempo
    // que pasa comiendo y notificar cuando haya acabado al ClientSatisfaction y a ClientBehavior
    // para irse del restaurante
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
