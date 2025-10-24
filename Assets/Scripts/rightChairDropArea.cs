using UnityEngine;

public class rightChairDropArea : MonoBehaviour, Table
{
    [SerializeField] private Vector3 sitOffset = new Vector3(0f, 0f, 0f);
    [SerializeField] private Transform parentTable;

    public void OnClientDrop(ClientBehavior client)
    {
        client.transform.position = transform.position + sitOffset;
        client.SitDown("sitRight", this);
        client.seatSide = ClientBehavior.SeatSide.Right;
        if (parentTable != null) client.assignedTableTransform = parentTable;
    }

}