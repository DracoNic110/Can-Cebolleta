using UnityEngine;

public class leftChairDropArea : MonoBehaviour, Table
{
    [SerializeField] private Vector3 sitOffset = new Vector3(0f, 0f, 0f);
    [SerializeField] private Transform parentTable;

    public void OnClientDrop(ClientBehavior client)
    {
        client.transform.position = transform.position + sitOffset;
        client.SitDown("sitLeft", this);
        client.seatSide = ClientBehavior.SeatSide.Left;
        if (parentTable != null) client.assignedTableTransform = parentTable;
    }

}