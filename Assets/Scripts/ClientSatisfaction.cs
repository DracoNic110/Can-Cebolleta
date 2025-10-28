using UnityEngine;

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

    [Header("Prefabs (asignar en Inspector)")]
    public GameObject coinPilePrefab;
    public GameObject dollarsPrefab;

    private Transform moneyPoint;
    private Transform moneyPointLeft;
    private Transform moneyPointRight;

    private bool clientLost = false;
    private bool hasPaid = false;

    private void Awake()
    {
        client = GetComponent<ClientBehavior>();
    }

    private void Start()
    {
        if (client != null && client.assignedTableTransform != null)
        {
            moneyPointLeft = client.assignedTableTransform.Find("moneyPointLeft");
            moneyPointRight = client.assignedTableTransform.Find("moneyPointRight");

            AssignMoneyPoint();
        }
    }

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
                Debug.Log($"❌ {name} se enfadó durante {currentState} tras {currentTimer:F1}s.");
                MakeClientLeaveAngry();
            }
        }
    }

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

    public void OnStartEating()
    {
        client?.StopAngryEffect();
        OnStateChange("Eating");
    }

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

        Debug.Log($"💵 {name} paga ${reward:F2} (promedio {avg:F2}s -> {percent * 100:F0}%)");

        if (reward <= 0f)
        {
            Debug.Log($"{name} no deja dinero (reward 0).");
            return;
        }

        if (moneyPoint == null)
            AssignMoneyPoint();

        if (moneyPoint == null)
        {
            Debug.LogWarning($"{name} no tiene moneyPoint asignado.");
            return;
        }

        bool spawnBills = percent >= 0.75f;

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
            GameObject coins = Instantiate(coinPilePrefab, moneyPoint.position, Quaternion.identity);
            coins.transform.SetParent(moneyPoint);

            MoneyDrop moneyDrop = coins.GetComponent<MoneyDrop>();
            if (moneyDrop == null)
                moneyDrop = coins.AddComponent<MoneyDrop>();

            moneyDrop.amount = Mathf.RoundToInt(basePrice * percent);
        }

        hasPaid = true;
        clientLost = false;
        currentState = ClientState.Leaving;
        client?.StopAngryEffect();

        GameObject exit = GameObject.Find("ClientPoints/ExitPoint");
        if (exit != null)
            client.LeaveRestaurant(exit.transform.position);
        else
            client.LeaveRestaurant(client.transform.position + Vector3.right * 5f);

        Debug.Log($"😀 {name} terminó de comer y se va feliz del restaurante.");
    }


    private float GetPercentFromAverage(float avg)
    {
        if (avg <= 7f) return 1f;
        if (avg <= 10f) return 0.75f;
        if (avg <= 15f) return 0.5f;
        if (avg <= 30f) return 0.25f;
        return 0f;
    }

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
            Debug.LogWarning($"⚠ {name}: No se pudo determinar lado -> usando moneyPoint por defecto.");
        }
    }


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
