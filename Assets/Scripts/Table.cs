using UnityEngine;

// Interfaz para definir el comportamiento de las mesas
public interface Table
{
    // Método que se llama cuando un cliente es dropeado en una silla
    void OnClientDrop(ClientBehavior client);
    // Obtenemos el booleano IsOccupied para evitar que clientes se sienten en el mismo lugar a la vez
    bool IsOccupied { get; }
}
