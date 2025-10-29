using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Script para controlar el jugador
public class player : MonoBehaviour
{

    [Header("Movimiento")]
    [SerializeField] Rigidbody2D rb2D;
    [SerializeField] Animator anim;
    [SerializeField] float speed;

    [Header("Interacción con clientes")]
    [SerializeField] KeyCode interactionKey = KeyCode.E;
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

    // Este es el método que utilizamos para mover el jugador
    private void Movement() {
        // Mientras está recogiendo la comida no puede moverse
        if (isPickingUp) {
            anim.SetBool("isMoving", false);
            move = Vector3.zero;
            return;
        }
        // Se obtiene los inputs del jugador
        move = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0f).normalized;

        // Se actualiza la animaciones con el WalkingTree de las animaciones 2D
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

    // Método para que el jugador interactúe con el entorno
    private void Interaction()
    {
        if (Input.GetKeyDown(interactionKey))
        {
            if (isHolding)
            {
                // Interacciones mientras sostiene comida
                TryDeliverFood();
                TryDiscardDish();
            }
            else
            {
                // Interacciones mientras no la sostiene
                TryTakeOrder();
                TryTakeFood();
                TryCollectMoney();
            }
        }
    }

    // Método para tomar la orden al cliente
    private void TryTakeOrder() {

        // Siempre y cuando no tenga un objeto o esté pillando uno
        if (isHolding || isPickingUp)
        {
            return;
        }
        
        // Detecta si hay clientes cercanos con la capa clientLayer y el rango de interacción del jugador
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRange, clientLayer);


        foreach (var hit in hits)
        {
            ClientBehavior client = hit.GetComponent<ClientBehavior>();
            if (client != null && client.IsReadyToTakeOrder())
            {
                client.MarkOrderTaken();

                if (kitchenManager != null)
                    kitchenManager.PrepareOrder(client.GetCurrentOrder());
                return;
            }
        }
    }

    // Método para intentar tomar la comida
    private void TryTakeFood()
    {

        // Al igual que el método anterior 
        if (isHolding || isPickingUp)
        {
            return;
        }

        // Detecta si hay comida con la capa foodLayer y el rango de interacción del jugador
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRange, foodLayer);

        foreach (var hit in hits)
        {
            foodPicked = hit.gameObject;

            Dish dishComp = foodPicked.GetComponent<Dish>();
            if (dishComp != null && dishComp.FoodData != null)
            {
                tempFoodData = dishComp.FoodData;
            }
            else
            {
                return;
            }

            if (kitchenManager != null) kitchenManager.RemoveOrder(foodPicked);

            // Se inicia la animación de PickUp para tomar comida
            isPickingUp = true;
            anim.SetBool("isPickingUp", isPickingUp);
            Invoke(nameof(EndPickUp), 0.8f);
            Invoke(nameof(FinalizePickUp), 0.8f);
            return;
        }
    }


    // Con este método finaliza la animación de pickUp
    private void EndPickUp()
    {
        isPickingUp = false;
        anim.SetBool("isPickingUp", isPickingUp);
    }

    // Coloca la comida en la mano del chef al finalizar pickUp
    private void FinalizePickUp()
    {
        if (tempFoodData == null)
        {
            return;
        }

        float lastH = anim.GetFloat("lastHorizontal");
        float lastV = anim.GetFloat("lastVertical");

        // Esto determina la posición del plato de comida según la última dirección del jugador
        if (Mathf.Abs(lastH) > Mathf.Abs(lastV))
            currentHandPoint = (lastH > 0) ? dishPositionRight : dishPositionLeft;
        else
            currentHandPoint = (lastV > 0) ? dishPositionUp : dishPositionDown;

        // se crea el objeto de la comida y convertimos en objeto padre (dependiendo de la posición calculada anteriormente)
        // la posición en la que debe ir el plato de comida
        GameObject newFood = new GameObject("FoodInHand");
        newFood.transform.SetParent(currentHandPoint);
        newFood.transform.localPosition = Vector3.zero;

        // Con esto controlamos en que capa se renderiza el plato, en nuestro caso la hemos puesto
        // en la capa foreground y en el sortingorder de 1 como el resto de objetos para que se vea bien
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

    // Este método sirve para ver en tiempo real (update) la forma en la que se ve el plato dependiendo de la dirección del chef
    private void UpdateHoldPosition()
    {
        float lastH = anim.GetFloat("lastHorizontal");
        float lastV = anim.GetFloat("lastVertical");
        bool isMoving = anim.GetBool("isMoving");

        Transform targetPoint;

        // si no se mueve por defecto el plato lo posicionamos delante del chef
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

    // Método que sirve para la lógica de entregar comida
    private void TryDeliverFood()
    {
        // Si no está "holdeando" la comida o directamente no tiene comida, este método no tiene efecto alguno
        if (!isHolding || food == null)
            return;

        // nuevamente se detecta si el jugador está cerca de un cliente
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRange, clientLayer);

        foreach (var hit in hits)
        {
            ClientBehavior client = hit.GetComponent<ClientBehavior>();
            if (client == null) continue;

            if (client.assignedTableTransform == null)
            {
                Destroy(food);
                food = null;
                isHolding = false;
                anim.SetBool("isHolding", false);
                return;
            }

            Dish holdDishComp = food.GetComponent<Dish>();
            if (holdDishComp == null || holdDishComp.FoodData == null)
            {
                return;
            }

            // Se verifica si el plato coincide con la orden del cliente
            if (client.HasOrder() && AreFoodsEquivalent(holdDishComp.FoodData, client.GetCurrentOrder()))
            {
                bool placed = PlaceDishOnTable(client, holdDishComp.FoodData);
                if (placed)
                {
                    client.MarkOrderTaken();

                    Destroy(food);
                    food = null;
                    isHolding = false;
                    anim.SetBool("isHolding", false);
                }
                else
                {
                    Destroy(food);
                    food = null;
                    isHolding = false;
                    anim.SetBool("isHolding", false);
                }
                return;
            }
        }
    }

    // Este método verifica si el plato que entregamos es el mismo que pide el cliente, a través de su ID
    private bool AreFoodsEquivalent(Food a, Food b)
    {
        // Evitamos errores si alguno está vacío
        if (a == null || b == null)
            return false;

        // verificamos si el id de ambas comidas es igual, si es igual no es necesario continuar
        if (!string.IsNullOrEmpty(a.id) && !string.IsNullOrEmpty(b.id))
            return a.id.Trim().ToLower() == b.id.Trim().ToLower();

        return false;
    }

    // Este método implementa la lógica de la basura
    private void TryDiscardDish()
    {
        if (!isHolding || food == null)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRange, trashLayer);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Trash"))
            {
                // destruimos la comida que tenemos en mano
                Destroy(food);
                food = null;

                // desactivamos la animación de holdear el plato
                isHolding = false;
                anim.SetBool("isHolding", false);
                return;
            }
        }
    }

    // Método para colocar la comida en la mesa del cliente dependiendo de la posición del mismo
    private bool PlaceDishOnTable(ClientBehavior client, Food foodData)
    {
        // Se verifica si el cliente no está vacío
        if (client == null)
        {
            return false;
        }

        // Se verifica si tiene mesa
        Transform tableTransform = client.assignedTableTransform;
        if (tableTransform == null)
        {
            return false;
        }

        // Buscamos los puntos de las mesas
        Transform leftPoint = tableTransform.Find("dishPointLeft");
        Transform rightPoint = tableTransform.Find("dishPointRight");

        // Si no hay puntos sobre donde poner los platos se retorna falso
        if (leftPoint == null && rightPoint == null)
        {
            return false;
        }

        // Si encuentra determinamos el punto donde hay que colocar el plato dependiendo de la posición del cliente
        Transform chosenPoint = (client.seatSide == ClientBehavior.SeatSide.Left) ? leftPoint : rightPoint;

        if (chosenPoint == null)
        {
            chosenPoint = leftPoint ?? rightPoint ?? tableTransform;
        }

        GameObject servedDish = null;
        try
        {
            servedDish = Instantiate(food, chosenPoint.position, Quaternion.identity, chosenPoint);
        }
        catch (System.Exception e)
        {
            return false;
        }

        if (servedDish == null)
        {
            return false;
        }

        // Configuramos la capa de renderizado de los platos para que se puedan ver por encima de las mesas
        SpriteRenderer servedSr = servedDish.GetComponent<SpriteRenderer>();
        if (servedSr != null)
        {
            servedSr.sortingLayerName = "Foreground";
            servedSr.sortingOrder = 1;
        }

        Dish dishComp = servedDish.GetComponent<Dish>();
        if (dishComp == null)
            dishComp = servedDish.AddComponent<Dish>();

        dishComp.AssignOrder(client);

        GameObject exitGO = GameObject.Find("ExitPoint");
        if (exitGO != null)
            dishComp.exitPoint = exitGO.transform;
        else
            dishComp.exitPoint = null;

        // Se informa al clientSatisfaction que el cliente ya empezó a comer
        client.GetComponent<ClientSatisfaction>()?.OnStartEating();
        dishComp.StartEatingRoutine();

        return true;
    }

    // Con este método el chef intentará pillar dinero de las mesas
    private void TryCollectMoney()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRange);

        bool collectedMoney = false;

        for (int hi = 0; hi < hits.Length; hi++)
        {
            Collider2D hit = hits[hi];
            if (!hit.CompareTag("Table"))
                continue;

            Transform table = hit.transform;

            Transform leftPoint = table.Find("moneyPointLeft");
            Transform rightPoint = table.Find("moneyPointRight");

            int totalCollected = 0;

            // Primero procesamos el dinero del lado izquierdo
            if (leftPoint != null)
            {
                int childCount = leftPoint.childCount;
                Transform[] childs = new Transform[childCount];
                // Con estos dos bucles miramos si hay dinero de este lado
                for (int i = 0; i < childCount; i++)
                    childs[i] = leftPoint.GetChild(i);

                for (int i = 0; i < childs.Length; i++)
                {
                    Transform child = childs[i];
                    if (child == null) continue;

                    MoneyDrop m = child.GetComponent<MoneyDrop>();
                    if (m == null) continue;


                    Vector3 moneyWorldPos = child.position;

                    totalCollected += m.amount;
                    collectedMoney = true;

                    Destroy(child.gameObject);

                    Vector3 spawnPos = new Vector3(moneyWorldPos.x, moneyWorldPos.y + 0.01f, moneyWorldPos.z);

                    // reproducimos el audio CashRegister
                    if (SoundsManager.Instance != null)
                        SoundsManager.Instance.PlaySound("CashRegister");

                    // Colocamos el efecto del texto flotante temporal con la cantidad de dinero que recibimos de este lado
                    if (GameManager.Instance != null && GameManager.Instance.floatingTextPrefab != null)
                    {
                        GameObject floatTxtGO = Instantiate(GameManager.Instance.floatingTextPrefab, spawnPos, Quaternion.identity);
                        floatTxtGO.transform.SetParent(null);
                        floatTxtGO.transform.position = spawnPos;
                        floatTxtGO.transform.rotation = Quaternion.identity;

                        FloatingText ft = floatTxtGO.GetComponent<FloatingText>();
                        if (ft != null)
                        {
                            ft.Initialize($"+ ${m.amount}", Color.green, spawnPos, 0.7f, 1.0f);
                        }
                    }
                }
            }

            // Hacemos lo mismo con el lado derecho
            if (rightPoint != null)
            {
                int childCount2 = rightPoint.childCount;
                Transform[] childs2 = new Transform[childCount2];
                for (int i = 0; i < childCount2; i++)
                    childs2[i] = rightPoint.GetChild(i);

                for (int i = 0; i < childs2.Length; i++)
                {
                    Transform child = childs2[i];
                    if (child == null) continue;

                    MoneyDrop m = child.GetComponent<MoneyDrop>();
                    if (m == null) continue;

                    Vector3 moneyWorldPos = child.position;

                    totalCollected += m.amount;
                    collectedMoney = true;

                    Destroy(child.gameObject);

                    Vector3 spawnPos = new Vector3(moneyWorldPos.x, moneyWorldPos.y + 0.01f, moneyWorldPos.z);

                    if (SoundsManager.Instance != null)
                        SoundsManager.Instance.PlaySound("CashRegister");

                    if (GameManager.Instance != null && GameManager.Instance.floatingTextPrefab != null)
                    {
                        GameObject floatTxtGO = Instantiate(GameManager.Instance.floatingTextPrefab, spawnPos, Quaternion.identity);
                        floatTxtGO.transform.SetParent(null);
                        floatTxtGO.transform.position = spawnPos;
                        floatTxtGO.transform.rotation = Quaternion.identity;

                        FloatingText ft = floatTxtGO.GetComponent<FloatingText>();
                        if (ft != null)
                        {
                            ft.Initialize($"+ ${m.amount}", Color.green, spawnPos, 0.7f, 1.0f);
                        }
                    }
                }
            }

            if (collectedMoney && totalCollected > 0)
            {
                GameManager.Instance.AddMoney(totalCollected);
            }
        }
    }

    // Movimientos fijos físicos del jugador
    void FixedUpdate()
    {
        if (isPickingUp) return;
        rb2D.MovePosition(transform.position + (move * speed * Time.fixedDeltaTime));
    }

}
