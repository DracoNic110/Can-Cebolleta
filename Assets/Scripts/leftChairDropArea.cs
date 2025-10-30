using UnityEngine;

// Clase que representa el área en la que podremos dropear el cliente en la silla izquierda de alguna mesa
public class leftChairDropArea : MonoBehaviour, Table
{
    [SerializeField] private Vector3 sitOffset = Vector3.zero;
    [SerializeField] private Transform parentTable;
    public Transform TableTransform => parentTable;
    public bool IsOccupied { get; private set; } = false;

    // Método para cuando el cliente se sienta en una silla
    public void OnClientDrop(ClientBehavior client)
    {
        if (parentTable == null) return;

        // si está ocupada el cliente retorna al WaitingPoint
        if (IsOccupied)
        {
            client.ReturnToWaitingPoint();
            return;
        }

        // Si la mesa tiene dinero sobre ella, el cliente no puede sentarse en ella
        if (HasMoneyOnTable(parentTable))
        {
            client.ReturnToWaitingPoint();
            return;
        }

        // Si todas las condiciones se cumplen activamos la animación del cliente con respecto a su dirección
        // y ajustamos correctamente su posición para que se vea bien
        client.transform.position = transform.position + sitOffset;
        client.SitDown("sitLeft", this);
        client.seatSide = ClientBehavior.SeatSide.Left;
        client.assignedTableTransform = parentTable;
        client.assignedSeat = this;
        IsOccupied = true;

    }

    // Verificamos si hay dinero en la mesa
    private bool HasMoneyOnTable(Transform table)
    {
        Transform left = table.Find("moneyPointLeft");
        Transform right = table.Find("moneyPointRight");
        return (left != null && left.childCount > 0) || (right != null && right.childCount > 0);
    }

    // Liberamos la silla para que otro cliente pueda sentarse en ella
    public void FreeSeat()
    {
        IsOccupied = false;
    }
}
