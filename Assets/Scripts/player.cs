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

    private void Start() {
        kitchenManager = FindFirstObjectByType<KitchenManager>();
    }

    private void Update()
    {
        Movement();
        Interaction();
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
            isPickingUp = true;
            anim.SetBool("isPickingUp", isPickingUp);
            Invoke(nameof(EndPickUp), 0.8f);
            return;
        }
    }

    private void pickUpFood() {
        if (food != null) Destroy(food);

        float lastH = anim.GetFloat("lastHorizontal");
        float lastV = anim.GetFloat("lastVertical");

        if (Mathf.Abs(lastH) > Mathf.Abs(lastV))
        {
            currentHandPoint = (lastH > 0) ? dishPositionRight : dishPositionLeft;
        }
        else
        {
            currentHandPoint = (lastV > 0) ? dishPositionUp : dishPositionDown;
        }

        food = Instantiate(food, currentHandPoint.position, Quaternion.identity, currentHandPoint.transform);
        food.GetComponent<Collider2D>().enabled = false;
        food.GetComponent<Rigidbody2D>().simulated = false;

        SpriteRenderer foodSprite = food.GetComponent<SpriteRenderer>();
        if (foodSprite != null)
        {
            foodSprite.sortingOrder = 10;
        }
    }

    private void EndPickUp()
    {
        isPickingUp = false;
        anim.SetBool("isPickingUp", isPickingUp);
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
