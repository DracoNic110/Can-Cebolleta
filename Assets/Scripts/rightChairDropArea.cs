using UnityEngine;

public class rightChairDropArea : MonoBehaviour, Table
{
    [SerializeField] private Vector3 sitOffset = new Vector3(0f, 0f, 0f);

    public void OnClientDrop(ClientBehavior client)
    {
        client.transform.position = transform.position + sitOffset;
        client.SitDown("sitRight");
        Debug.Log("Client dropped on right chair");
    }
}