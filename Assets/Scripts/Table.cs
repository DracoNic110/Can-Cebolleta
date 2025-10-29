using UnityEngine;

public interface Table
{
    void OnClientDrop(ClientBehavior client);
    bool IsOccupied { get; }
}
