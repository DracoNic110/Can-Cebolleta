using UnityEngine;

[System.Serializable]
public class CookingOrder
{
    public Food food;
    public Transform spawnPoint;
    public bool ready;
    public GameObject instance;

    public CookingOrder(Food food, Transform spawnPoint)
    {
        this.food = food;
        this.spawnPoint = spawnPoint;
        this.ready = false;
        this.instance = null;
    }
}