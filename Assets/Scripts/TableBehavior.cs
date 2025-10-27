using UnityEngine;

public class TableBehaviour : MonoBehaviour, Table
{
    [Header("Configuración de la mesa")]
    [SerializeField] private int maxSeats = 2;

    private int occupiedSeats = 0;

    public bool HasFreeSeat() => occupiedSeats < maxSeats;

    public bool TryAssignClient(ClientBehavior client)
    {
        if (!HasFreeSeat())
            return false;

        occupiedSeats++;
        client.assignedTableTransform = this.transform;
        return true;
    }

    public void ReleaseSeat()
    {
        occupiedSeats = Mathf.Max(0, occupiedSeats - 1);
    }

    // Implementación de la interfaz
    public void OnClientDrop(ClientBehavior client)
    {
        if (TryAssignClient(client))
            Debug.Log($"{client.name} se ha sentado en la mesa {name}");
        else
            Debug.LogWarning($"Mesa {name} llena. No se puede sentar {client.name}");
    }

    public int GetFreeSeats() => maxSeats - occupiedSeats;
}
