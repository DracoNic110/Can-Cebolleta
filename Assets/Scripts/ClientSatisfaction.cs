using UnityEngine;

// Gestiona la satisfacción del cliente relativamente con el servicio que se le dé en el restaurante
public class ClientSatisfaction : MonoBehaviour
{
    private ClientBehavior client;

    private float waitingPointTime = 0f;
    private float waitingOrderTime = 0f;
    private float waitingFoodTime = 0f;

    private float currentTimer = 0f;

    private enum ClientState { WaitingPoint, WaitingOrder, WaitingFood, Eating, Leaving }
    private ClientState currentState = ClientState.WaitingPoint;

    [Header("Configuración")]
    [SerializeField] private float maxWaitTime = 30f;
    public float basePrice = 10f;

    [Header("Prefabs")]
    public GameObject coinPilePrefab;
    public GameObject dollarsPrefab;

    [Header("Puntos para dejar el dinero")]
    private Transform moneyPoint;
    private Transform moneyPointLeft;
    private Transform moneyPointRight;

    private bool clientLost = false;
    private bool hasPaid = false;

    // Obtenemos la refencia del cliente
    private void Awake()
    {
        client = GetComponent<ClientBehavior>();
    }

    // Inicializamos los puntos de dejar el dinero
    private void Start()
    {
        if (client != null && client.assignedTableTransform != null)
        {
            moneyPointLeft = client.assignedTableTransform.Find("moneyPointLeft");
            moneyPointRight = client.assignedTableTransform.Find("moneyPointRight");

            AssignMoneyPoint();
        }
    }

    // Controla los tiempos de espera y controla el enojon del cliente
    private void Update()
    {
        if (clientLost || currentState == ClientState.Leaving)
            return;

        if (currentState == ClientState.WaitingPoint || currentState == ClientState.WaitingOrder || currentState == ClientState.WaitingFood)
        {
            currentTimer += Time.deltaTime;

            if (currentTimer >= 22f && !client.isAngry)
                client.StartAngryEffect();

            if (currentTimer >= maxWaitTime)
            {
                clientLost = true;
                MakeClientLeaveAngry();
            }
        }
    }

    // Cambia el estado del cliente y se registra el tiempo que se ha gastado en atenderlo
    public void OnStateChange(string newState)
    {
        client?.StopAngryEffect();

        switch (currentState)
        {
            case ClientState.WaitingPoint:
                waitingPointTime = currentTimer;
                break;
            case ClientState.WaitingOrder:
                waitingOrderTime = currentTimer;
                break;
            case ClientState.WaitingFood:
                waitingFoodTime = currentTimer;
                break;
            case ClientState.Eating:
                currentTimer = 0f;
                
                break;
            case ClientState.Leaving:
                currentTimer = 0f;
                break;
        }

        if (newState == "WaitingPoint") currentState = ClientState.WaitingPoint;
        else if (newState == "WaitingOrder") currentState = ClientState.WaitingOrder;
        else if (newState == "WaitingFood") currentState = ClientState.WaitingFood;
        else if (newState == "Eating") currentState = ClientState.Eating;
        else if (newState == "Leaving") currentState = ClientState.Leaving;

        currentTimer = 0f;
    }

    // Es llamado cuando el cliente empiza a comer
    public void OnStartEating()
    {
        client?.StopAngryEffect();
        OnStateChange("Eating");
        client?.orderBalloon.SetActive(false);
    }

    // Es llamado cuando el cliente termina de comer para calcular la cantidad de dinero correspondiente
    // con respecto a la media de tiempo de espera
    public void OnFinishedEating()
    {
        if (clientLost || hasPaid)
            return;

        float sum = 0f;
        int count = 0;

        if (waitingPointTime > 0f) { sum += waitingPointTime; count++; }
        if (waitingOrderTime > 0f) { sum += waitingOrderTime; count++; }
        if (waitingFoodTime > 0f) { sum += waitingFoodTime; count++; }

        float avg = count > 0 ? sum / count : 0f;
        float percent = GetPercentFromAverage(avg);
        float reward = basePrice * percent;

        if (reward <= 0f)
        {
            return;
        }

        if (moneyPoint == null)
            AssignMoneyPoint();

        if (moneyPoint == null)
        {
            return;
        }

        bool spawnBills = percent >= 0.75f;
        // Spawneamos billetes si el servicio fue bueno es decir que tuvo una satisfacción mayor o igual que 0.75
        if (spawnBills && dollarsPrefab != null)
        {
            GameObject bills = Instantiate(dollarsPrefab, moneyPoint.position, dollarsPrefab.transform.rotation);
            bills.transform.SetParent(moneyPoint);

            MoneyDrop moneyDrop = bills.GetComponent<MoneyDrop>();
            if (moneyDrop == null)
                moneyDrop = bills.AddComponent<MoneyDrop>();

            moneyDrop.amount = Mathf.RoundToInt(basePrice * percent);
        }
        else if (coinPilePrefab != null)
        {
            // De lo contrario, si la satisfacción está por debajo del 75% (regular), spawneamos una pila de monedas
            GameObject coins = Instantiate(coinPilePrefab, moneyPoint.position, Quaternion.identity);
            coins.transform.SetParent(moneyPoint);

            MoneyDrop moneyDrop = coins.GetComponent<MoneyDrop>();
            if (moneyDrop == null)
                moneyDrop = coins.AddComponent<MoneyDrop>();

            moneyDrop.amount = Mathf.RoundToInt(basePrice * percent);
        }

        // Marcamos que el cliente ha pagado y está en proceso de irse del restaurante
        hasPaid = true;
        clientLost = false;
        currentState = ClientState.Leaving;
        client?.StopAngryEffect();

        GameObject exit = GameObject.Find("ClientPoints/ExitPoint");
        if (exit != null)
            client.LeaveRestaurant(exit.transform.position);
        else
            client.LeaveRestaurant(client.transform.position + Vector3.right * 5f);
    }

    // Obtenemos el porcentaje de satisfacción con respecto a la media de tiempo de espera
    private float GetPercentFromAverage(float avg)
    {
        if (avg <= 7f) return 1f;
        if (avg <= 10f) return 0.75f;
        if (avg <= 15f) return 0.5f;
        if (avg <= 30f) return 0.25f;
        return 0f;
    }

    // Asignamos un punto de dónde dejar el dinero dependiendo de la dirección en la que el cliente está sentado
    private void AssignMoneyPoint()
    {
        if (client == null || client.assignedTableTransform == null)
            return;

        if (moneyPointLeft == null)
            moneyPointLeft = client.assignedTableTransform.Find("moneyPointLeft");
        if (moneyPointRight == null)
            moneyPointRight = client.assignedTableTransform.Find("moneyPointRight");

        if (client.seatSide == ClientBehavior.SeatSide.Right && moneyPointRight != null)
        {
            moneyPoint = moneyPointRight;
            return;
        }
        else if (client.seatSide == ClientBehavior.SeatSide.Left && moneyPointLeft != null)
        {
            moneyPoint = moneyPointLeft;
            return;
        }

        Vector3 relativePos = client.transform.position - client.assignedTableTransform.position;
        bool isRight = relativePos.x > 0f;

        if (isRight && moneyPointRight != null)
        {
            moneyPoint = moneyPointRight;
        }
        else if (!isRight && moneyPointLeft != null)
        {
            moneyPoint = moneyPointLeft;
        }
        else
        {
            moneyPoint = moneyPointLeft ?? moneyPointRight;
        }
    }

    // Hace que el cliente se marche del restaurante enojado cuando se excede el tiempo de espera máximo
    private void MakeClientLeaveAngry()
    {
        if (client != null)
        {
            GameObject exit = GameObject.Find("ClientPoints/ExitPoint");
            if (exit != null)
                client.LeaveRestaurant(exit.transform.position);
            else
                client.LeaveRestaurant(client.transform.position + Vector3.right * 5f);
        }
    }
}
