using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.U2D.Animation;

// Esta clase implementa la lógica de spawnear clientes con cierto control
public class ClientSpawner : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject clientPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform waitingPoint;

    [Header("Control de Spawn")]
    [SerializeField] private int maxClients = 8;
    [SerializeField] private float baseSpawnInterval = 8f;
    [SerializeField] private float minSpawnInterval = 3f;
    [SerializeField] private float restTime = 10f;
    [SerializeField] private bool autoSpawn = true;

    [Header("Dificultad Dinámica")]
    [SerializeField, Range(0f, 1f)] private float difficultyProgress = 0f;
    [SerializeField] private float difficultyIncreaseRate = 0.05f;

    [Header("Fila de Espera")]
    [SerializeField] private float spacing = 1.2f;
    [SerializeField] private Vector2 direction = new Vector2(0f, -1f);

    [Header("random Sprite selector")]
    [SerializeField] private List<SpriteLibraryAsset> skins;

    private int currentClients = 0;
    private List<GameObject> spawned = new List<GameObject>();
    private bool canSpawn = true;
    private bool resting = false;

    // Empezamos la rutina del spawnLoop
    private void Start()
    {
        if (autoSpawn)
            StartCoroutine(SpawnLoop());
    }

    // Rutina que establece el bucle de spawn y su lógica
    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (!canSpawn || resting)
            {
                yield return null;
                continue;
            }

            // siempre y cuando los clientes actuales sean menores que la capacidad del restaurante
            // seguirá spawneando clientes
            if (currentClients < maxClients)
            {
                SpawnClient();
            }
            else
            {
                resting = true;
                yield return new WaitForSeconds(restTime);
                resting = false;
            }

            // Calculamos la dificultad con respecto a la ocupación total del restaurante y ajustamos el intervalo de spawn
            float crowdFactor = (float)currentClients / maxClients;
            float difficultyFactor = Mathf.Lerp(1f, 0.5f, difficultyProgress);
            float adjustedInterval = Mathf.Lerp(baseSpawnInterval, minSpawnInterval, difficultyProgress);
            adjustedInterval *= Mathf.Lerp(1.2f, 0.8f, 1f - crowdFactor);

            yield return new WaitForSeconds(adjustedInterval);

            difficultyProgress = Mathf.Clamp01(difficultyProgress + difficultyIncreaseRate * Time.deltaTime);
        }
    }

    // Método para spawnear clientes y asignarles las referencias correctas para su funcionamiento
    public void SpawnClient()
    {
        if (clientPrefab == null || spawnPoint == null) return;


        GameObject client = Instantiate(clientPrefab, spawnPoint.position, Quaternion.identity);
        client.GetComponent<SpriteLibrary>().spriteLibraryAsset = skins[Random.Range(0, skins.Count)];
        spawned.Add(client);
        currentClients++;

        ClientSatisfaction satisfaction = client.GetComponent<ClientSatisfaction>();
        if (satisfaction != null)
        {
            if (satisfaction.coinPilePrefab == null)
                satisfaction.coinPilePrefab = Resources.Load<GameObject>("Prefabs/CoinPile");

            if (satisfaction.dollarsPrefab == null)
                satisfaction.dollarsPrefab = Resources.Load<GameObject>("Prefabs/Dollars");
        }

        Vector3 waitPos = GetWaitingPosition(spawned.Count - 1);

        ClientBehavior clientBehavior = client.GetComponent<ClientBehavior>();
        if (clientBehavior != null)
            clientBehavior.Initialize(waitPos, this);
    }

    // Obtenemos la posición de la fila
    private Vector3 GetWaitingPosition(int index)
    {
        Vector3 offset = new Vector3(direction.x * spacing * index, direction.y * spacing * index, 0f);
        return waitingPoint.position + offset;
    }

    // Si un cliente se sienta llamamos al método ReorderQueue para organizar la fila
    public void NotifyClientSeated(GameObject client)
    {
        if (spawned.Contains(client))
        {
            spawned.Remove(client);
        }

        if (spawned.Count > 0)
            ReorderQueue();
    }

    // Si algún cliente se va del restaurante lo removemos y reducimos el contador de clientes actuales
    public void NotifyClientLeft(GameObject client)
    {
        if (spawned.Contains(client))
            spawned.Remove(client);

        currentClients = Mathf.Max(0, currentClients - 1);

        ReorderQueue();
        AdvanceQueueIfFrontAvailable();
    }

    // Con este par de métodos reordenamos la fila
    private void ReorderQueue()
    {
        StartCoroutine(ReorderQueueRoutine());
    }

    
    private IEnumerator ReorderQueueRoutine()
    {
        for (int i = 0; i < spawned.Count; i++)
        {
            GameObject c = spawned[i];
            if (c == null) continue;

            ClientBehavior behavior = c.GetComponent<ClientBehavior>();
            if (behavior == null) continue;

            if (behavior.assignedTable != null || behavior.assignedSeat != null)
                continue;

            Vector3 newPos = GetWaitingPosition(i);
            yield return new WaitForSeconds(0.05f);

            behavior.MoveTo(newPos);
        }
    }

    // Adelantamos la fila si ya hay espacio 
    public void AdvanceQueueIfFrontAvailable()
    {
        if (spawned.Count == 0) return;

        GameObject firstClient = spawned[0];
        if (firstClient == null) return;

        // si el cliente no se asigna a una mesa, lo mueve al WaitingPoint
        ClientBehavior behavior = firstClient.GetComponent<ClientBehavior>();
        if (behavior != null && behavior.assignedTable == null && behavior.assignedSeat == null)
            behavior.MoveTo(waitingPoint.position);

        // con este bucle gestionamos el resto de la fila
        for (int i = 1; i < spawned.Count; i++)
        {
            GameObject c = spawned[i];
            if (c == null) continue;

            ClientBehavior b = c.GetComponent<ClientBehavior>();
            if (b != null && b.assignedTable == null && b.assignedSeat == null)
            {
                Vector3 newPos = GetWaitingPosition(i);
                b.MoveTo(newPos);
            }
        }
    }


    // Con deste método pausa el spawn de clientes
    public void StopSpawning() => canSpawn = false;

    // Con deste continúa pausa el spawn de clientes
    public void ResumeSpawning() => canSpawn = true;

    // Con este método forzamos un spawn
    public void ForceSpawn()
    {
        if (currentClients < maxClients)
            SpawnClient();
    }

    // Reseteamos la dificultad
    public void ResetDifficulty()
    {
        difficultyProgress = 0f;
    }
}
