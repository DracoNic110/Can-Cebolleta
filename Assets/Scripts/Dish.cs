using UnityEngine;

public class Dish : MonoBehaviour
{
    public ClientBehavior clientOwner;
    public Food FoodData;

    public void AssignOrder(ClientBehavior client)
    {
        clientOwner = client;
        FoodData = client.CurrentOrder;
    }

    public void Initialize(Food food)
    {
        FoodData = food;
    }
}
