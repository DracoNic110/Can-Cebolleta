using UnityEngine;


// Representa una comida dentro del juego, de manera que tenga dos sprites (de pedido y de plato), un nombre y un identificador
[System.Serializable]
public class Food
{
    [Header("Identificación de la comida")]
    public string id;
    public string name;

    [Header("Sprites")]
    public Sprite orderSprite;
    public Sprite dishSprite;

    [Header("Tiempo")]
    public float minCookTime = 10f;
    public float maxCookTime = 15f;
}