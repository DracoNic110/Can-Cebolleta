using UnityEngine;

public class player : MonoBehaviour
{

    [Header("Movimiento")]
    [SerializeField] Rigidbody2D rb2D;
    [SerializeField] Animator anim;
    [SerializeField] float speed;

    [Header("Interacción con clientes")]
    [SerializeField] KeyCode takeOrderKey = KeyCode.E;
    [SerializeField] float interactionRange = 1.5f;
    [SerializeField] LayerMask clientLayer;
    [SerializeField] LayerMask foodLayer;

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
    Sprite tempFoodData;

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

    private void Interaction() {
        if (Input.GetKeyDown(takeOrderKey)) {
            TryTakeOrder();
            TryTakeFood();
        }
    }

    private void TryTakeOrder() {
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

    private void TryTakeFood() {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRange, foodLayer);

        foreach (var hit in hits) {
            Debug.Log("Tomar comida");
            foodPicked = hit.gameObject;
            SpriteRenderer sr = foodPicked.GetComponent<SpriteRenderer>();
            if (sr != null) tempFoodData = sr.sprite;
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
            Debug.LogWarning("No hay sprite de comida para instanciar en las manos.");
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
        sr.sprite = tempFoodData;
        sr.sortingLayerName = "Foreground";
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

        AdjustFoodSortingOrder(targetPoint);
    }


    private void AdjustFoodSortingOrder(Transform targetPoint)
    {
        SpriteRenderer sr = food.GetComponent<SpriteRenderer>();
        SpriteRenderer playerSr = GetComponent<SpriteRenderer>();
        if (sr == null || playerSr == null) return;
        if (targetPoint == dishPositionDown)
        {
            sr.sortingOrder = playerSr.sortingOrder + 1;
        }
        else
        {
            sr.sortingOrder = playerSr.sortingOrder;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }


    void FixedUpdate()
    {
        rb2D.MovePosition(transform.position + (move * speed * Time.fixedDeltaTime));
    }

}
