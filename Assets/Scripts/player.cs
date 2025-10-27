using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class player : MonoBehaviour
{

    [Header("Movimiento")]
    [SerializeField] Rigidbody2D rb2D;
    [SerializeField] Animator anim;
    [SerializeField] float speed;

    [Header("Interacción con clientes")]
    [SerializeField] KeyCode takeOrderKey = KeyCode.E;
    [SerializeField] float interactionRange = 1f;
    [SerializeField] LayerMask clientLayer;
    [SerializeField] LayerMask foodLayer;
    [SerializeField] LayerMask trashLayer;

    [Header("Puntos de las manos del chef")]
    [SerializeField] Transform dishPositionUp;
    [SerializeField] Transform dishPositionDown;
    [SerializeField] Transform dishPositionLeft;
    [SerializeField] Transform dishPositionRight;
    Transform currentHandPoint;
    GameObject food;

    Vector3 move;
    KitchenManager kitchenManager;

    bool isPickingUp = false;
    bool isHolding = false;
    GameObject foodPicked;
    Food tempFoodData;

    private void Start() {
        kitchenManager = FindFirstObjectByType<KitchenManager>();
    }

    private void Update()
    {
        Movement();
        Interaction();
        if (isHolding && food != null) UpdateHoldPosition();
    }

    private void Movement() {


        if (isPickingUp) {
            anim.SetBool("isMoving", false);
            move = Vector3.zero;
            return;
        }
        move = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0f).normalized;

        anim.SetFloat("horizontal", move.x);
        anim.SetFloat("vertical", move.y);

        bool isMoving = move.magnitude > 0;
        anim.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            anim.SetFloat("lastHorizontal", move.x);
            anim.SetFloat("lastVertical", move.y);
        }
    }

    private void Interaction()
    {
        if (Input.GetKeyDown(takeOrderKey))
        {
            if (isHolding)
            {
                TryDeliverFood();
                TryDiscardDish();
            }
            else
            {
                TryTakeOrder();
                TryTakeFood();
                TryCollectMoney();
            }
        }
    }


    private void TryTakeOrder() {
        if (isHolding || isPickingUp)
        {
            Debug.Log("No puedes tomar pedidos mientras sostienes un plato.");
            return;
        }


        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRange, clientLayer);


        foreach (var hit in hits)
        {
            ClientBehavior client = hit.GetComponent<ClientBehavior>();
            if (client != null && client.IsReadyToTakeOrder())
            {
                Debug.Log("Pedido tomado del cliente {client.name}");

                client.MarkOrderTaken();

                if (kitchenManager != null)
                    kitchenManager.PrepareOrder(client.GetCurrentOrder());
                return;
            }
        }

        Debug.Log("No hay clientes cercanos con pedidos disponibles.");
    }

    private void TryTakeFood()
    {
        if (isHolding || isPickingUp)
        {
            Debug.Log("No puedes recoger otro plato mientras sostienes uno o estás recogiendo.");
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRange, foodLayer);

        foreach (var hit in hits)
        {
            foodPicked = hit.gameObject;

            Dish dishComp = foodPicked.GetComponent<Dish>();
            if (dishComp != null && dishComp.FoodData != null)
            {
                tempFoodData = dishComp.FoodData;
                Debug.Log($"Datos de comida asignados correctamente: {tempFoodData.name}");
            }
            else
            {
                Debug.LogWarning("El plato no tiene datos de comida asignados en Dish.");
                return;
            }

            if (kitchenManager != null) kitchenManager.RemoveOrder(foodPicked);

            isPickingUp = true;
            anim.SetBool("isPickingUp", isPickingUp);
            Invoke(nameof(EndPickUp), 0.8f);
            Invoke(nameof(FinalizePickUp), 0.8f);
            return;
        }
    }



    private void EndPickUp()
    {
        isPickingUp = false;
        anim.SetBool("isPickingUp", isPickingUp);
    }


    private void FinalizePickUp()
    {
        if (tempFoodData == null)
        {
            Debug.LogWarning("No hay datos de comida para instanciar en las manos.");
            return;
        }

        float lastH = anim.GetFloat("lastHorizontal");
        float lastV = anim.GetFloat("lastVertical");

        if (Mathf.Abs(lastH) > Mathf.Abs(lastV))
            currentHandPoint = (lastH > 0) ? dishPositionRight : dishPositionLeft;
        else
            currentHandPoint = (lastV > 0) ? dishPositionUp : dishPositionDown;

        GameObject newFood = new GameObject("FoodInHand");
        newFood.transform.SetParent(currentHandPoint);
        newFood.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = newFood.AddComponent<SpriteRenderer>();
        sr.sprite = tempFoodData.dishSprite;
        sr.sortingLayerName = "Foreground";
        sr.sortingOrder = 1;

        Dish dishComp = newFood.AddComponent<Dish>();
        dishComp.FoodData = tempFoodData;

        food = newFood;
        isHolding = true;
        anim.SetBool("isHolding", isHolding);

        foodPicked = null;
        tempFoodData = null;
    }

    private void UpdateHoldPosition()
    {
        float lastH = anim.GetFloat("lastHorizontal");
        float lastV = anim.GetFloat("lastVertical");
        bool isMoving = anim.GetBool("isMoving");

        Transform targetPoint;

        if (!isMoving)
        {
            targetPoint = dishPositionDown;
        }
        else if (Mathf.Abs(lastH) > Mathf.Abs(lastV))
        {
            targetPoint = (lastH > 0) ? dishPositionRight : dishPositionLeft;
        }
        else
        {
            targetPoint = (lastV > 0) ? dishPositionUp : dishPositionDown;
        }

        if (currentHandPoint != targetPoint)
        {
            currentHandPoint = targetPoint;
            food.transform.SetParent(currentHandPoint);
            food.transform.localPosition = Vector3.zero;
        }
    }

    private void TryDeliverFood()
    {
        if (!isHolding || food == null)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRange, clientLayer);

        foreach (var hit in hits)
        {
            ClientBehavior client = hit.GetComponent<ClientBehavior>();
            if (client == null) continue;

            Dish holdDishComp = food.GetComponent<Dish>();
            if (holdDishComp == null || holdDishComp.FoodData == null)
            {
                Debug.LogWarning("El plato en mano no tiene datos de comida válidos.");
                return;
            }

            if (client.HasOrder() && holdDishComp.FoodData == client.GetCurrentOrder())
            {
                PlaceDishOnTable(client, holdDishComp.FoodData);
                client.MarkOrderTaken();

                Destroy(food);
                food = null;
                isHolding = false;
                anim.SetBool("isHolding", false);

                Debug.Log($"✅ Plato '{holdDishComp.FoodData.name}' entregado correctamente a {client.name}");
                return;
            }
            else
            {
                Debug.Log($"❌ El cliente {client.name} pidió {client.GetCurrentOrder().name}, pero tienes {holdDishComp.FoodData.name}");
            }
        }
    }

    private void TryDiscardDish()
    {
        if (!isHolding || food == null)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRange, trashLayer);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Trash"))
            {
                Destroy(food);
                food = null;
                isHolding = false;
                anim.SetBool("isHolding", false);
                return;
            }
        }
    }


    private void PlaceDishOnTable(ClientBehavior client, Food foodData)
    {
        Transform tableTransform = client.assignedTableTransform;
        if (tableTransform == null)
        {
            Debug.LogWarning($"El cliente {client.name} no tiene mesa asignada.");
            return;
        }

        Transform leftPoint = tableTransform.Find("dishPointLeft");
        Transform rightPoint = tableTransform.Find("dishPointRight");

        if (leftPoint == null && rightPoint == null)
        {
            Debug.LogWarning($"La mesa de {client.name} no tiene dishPoints asignados.");
            return;
        }

        Transform chosenPoint = client.seatSide == ClientBehavior.SeatSide.Left ? leftPoint : rightPoint;
        if (chosenPoint == null)
        {
            Debug.LogWarning($"No se encontró el dishPoint correspondiente para {client.name}");
            return;
        }

        GameObject servedDish = Instantiate(food, chosenPoint.position, Quaternion.identity, chosenPoint);
        SpriteRenderer dishSr = food.GetComponent<SpriteRenderer>();
        dishSr.sortingLayerName = "Foreground";
        dishSr.sortingOrder = 1;

        Dish dishComp = servedDish.GetComponent<Dish>();
        if (dishComp == null)
            dishComp = servedDish.AddComponent<Dish>();

        dishComp.AssignOrder(client);
        dishComp.exitPoint = GameObject.Find("ExitPoint").transform;
        client.GetComponent<ClientSatisfaction>()?.OnStartEating();
        dishComp.StartEatingRoutine();

        Debug.Log($"🍽️ Plato '{foodData.name}' colocado correctamente en la mesa de {client.name}");
    }


    private void TryCollectMoney()
    {
        float radius = interactionRange;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRange);

        bool foundTable = false;
        bool collectedMoney = false;

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Table"))
                continue;

            foundTable = true;
            Transform table = hit.transform;

            Transform leftPoint = table.Find("moneyPointLeft");
            Transform rightPoint = table.Find("moneyPointRight");

            int totalCollected = 0;

            if (leftPoint != null)
            {
                foreach (Transform child in leftPoint)
                {
                    MoneyDrop m = child.GetComponent<MoneyDrop>();
                    if (m != null)
                    {
                        totalCollected += m.amount;
                        Destroy(child.gameObject);
                    }
                }
            }

            if (rightPoint != null)
            {
                foreach (Transform child in rightPoint)
                {
                    MoneyDrop m = child.GetComponent<MoneyDrop>();
                    if (m != null)
                    {
                        totalCollected += m.amount;
                        Destroy(child.gameObject);
                    }
                }
            }

            if (totalCollected > 0)
            {
                collectedMoney = true;
                Debug.Log($"💰 El chef recogió {totalCollected}$ de la mesa '{table.name}'.");

                GameManager.Instance.AddMoney(totalCollected);
            }
        }

        if (!foundTable)
        {
            Debug.Log("⚠ No hay ninguna mesa dentro del rango de interacción.");
        }
        else if (!collectedMoney)
        {
            Debug.Log("💨 No hay dinero en los moneyPoints de la mesa.");
        }
    }



    void FixedUpdate()
    {
        if (isPickingUp) return;
        rb2D.MovePosition(transform.position + (move * speed * Time.fixedDeltaTime));
    }

}
