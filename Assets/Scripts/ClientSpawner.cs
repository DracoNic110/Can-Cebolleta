using UnityEngine;
using System.Collections.Generic;

public class ClientSpawner : MonoBehaviour
{

    [Header("Points")]
    [SerializeField] private GameObject clientPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform waitingPoint;

    [Header("Spawn controller")]
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private int maxClients = 2;

    private float timer;
    private int currentClients = 0;
    private List<GameObject> spawned = new List<GameObject> ();

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval && currentClients < maxClients)
        {
            spawnClient();
            timer = 0f;
        }
    }

    public void spawnClient() {
        if (clientPrefab == null || spawnPoint == null || waitingPoint == null) return;

        GameObject client = Instantiate(clientPrefab, spawnPoint.position, Quaternion.identity);
        spawned.Add(client);
        currentClients++;


        ClientBehavior clientBehavior = client.GetComponent<ClientBehavior>();
        if (clientBehavior != null)
        {
            clientBehavior.Initialize(waitingPoint.position, this);
        }
    }

    public void NotifyClientLeft(GameObject client) {
        currentClients = Mathf.Max(0, currentClients - 1);
        spawned.Remove(client);
    }
}
