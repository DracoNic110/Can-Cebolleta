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


    Vector3 move;
    KitchenManager kitchenManager;

    private void Start() {
        kitchenManager = FindFirstObjectByType<KitchenManager>();
    }

    private void Update()
    {
        Movement();
        Interaction();
    }

    private void Movement() {
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
            return;
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
