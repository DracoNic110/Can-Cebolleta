using UnityEngine;

public class Dish : MonoBehaviour
{
    ClientBehavior clientOwner;
    Food food;

    public void AssignOrder(ClientBehavior client)
    {
        clientOwner = client;
        food = client.CurrentOrder;
    }
}