using UnityEngine;

public class leftChairDropArea : MonoBehaviour, Table
{
    [SerializeField] private Vector3 sitOffset = Vector3.zero;
    [SerializeField] private Transform parentTable;

    public void OnClientDrop(ClientBehavior client)
    {
        if (parentTable == null)
        {
            Debug.LogWarning($"⚠ La silla izquierda de {name} no tiene mesa asignada.");
            return;
        }

        if (HasMoneyOnTable(parentTable))
        {
            Debug.LogWarning($"🪙 La mesa '{parentTable.name}' tiene dinero, el cliente {client.name} no puede sentarse aún.");
            client.ReturnToWaitingPoint();
            return;
        }

        client.transform.position = transform.position + sitOffset;
        client.SitDown("sitLeft", this);
        client.seatSide = ClientBehavior.SeatSide.Left;
        client.assignedTableTransform = parentTable;
    }

    private bool HasMoneyOnTable(Transform table)
    {
        Transform left = table.Find("moneyPointLeft");
        Transform right = table.Find("moneyPointRight");
        return (left != null && left.childCount > 0) || (right != null && right.childCount > 0);
    }
}
