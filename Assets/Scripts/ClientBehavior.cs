using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

// Script para controlar el comportamiento del cliente durante el juego, donde tendremos en cuenta los estados posibles del cliente
// y qué hacer en cada estado
[RequireComponent(typeof(Collider2D))]
public class ClientBehavior : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private LayerMask tableLayerMask;
    private Coroutine queueMoveCoroutine = null;

    [Header("Sentarse en mesas")]
    public Table assignedTable;
    public Table assignedSeat;
    public Transform assignedTableTransform;
    public enum SeatSide { Left, Right }
    public SeatSide seatSide;

    [Header("Pedido y el globito del pedido")]
    [SerializeField] private GameObject orderBalloon;
    [SerializeField] private List<Food> possibleFoods;
    [SerializeField] private float thinkingOrderIntervalStart;
    [SerializeField] private float thinkingOrderIntervalEnd;
    private SpriteRenderer balloonRenderer;
    private SpriteRenderer foodRenderer;
    public Food CurrentOrder;

    [Header("Posiciones clave del cliente")]
    private Vector3 targetPosition;
    private Vector3 waitPositionClient;

    private Animator anim;
    
    private Coroutine moveCoroutine = null;
    private ClientSpawner spawner = null;

    private Collider2D col;
    private Vector3 startDragPosition;

    [Header("Estados del cliente")]
    private bool isDragging = false;
    private bool isWaiting = false;
    public bool isAngry = false;
    private bool hasOrdered = false;
    public bool orderTaken = false;
    public bool HasPendingOrder => CurrentOrder != null && !orderTaken;
    private bool isSeated = false;
    private bool isLeaving = false;

    [Header("Satisfacción del cliente")]
    private ClientSatisfaction satisfaction;
    private Color originalColor;
    private SpriteRenderer sr;
    public Color angryColor = Color.red;
    private Coroutine angryCoroutine = null;


    // Se inicializa las referencias como el globito y la satisfacción
    private void Awake()
    {
        anim = GetComponent<Animator>();
        satisfaction = GetComponent<ClientSatisfaction>();
        sr = GetComponent<SpriteRenderer>();

        if (sr != null)
            originalColor = sr.color;

        if (orderBalloon != null)
        {
            balloonRenderer = orderBalloon.GetComponent<SpriteRenderer>();
            Transform foodTransform = orderBalloon.transform.Find("foodSprite");
            if (foodTransform != null)
                foodRenderer = foodTransform.GetComponent<SpriteRenderer>();

            orderBalloon.SetActive(false);
        }
    }

    // Se inicializa el collider y el spriteRenderer
    private void Start()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            sr.material = new Material(sr.material);

            sr.color = Color.white;
            originalColor = sr.color;
        }
    }

    // Inicializa el cliente y su punto de espera
    public void Initialize(Vector3 waitPosition, ClientSpawner spawnerRef = null)
    {
        spawner = spawnerRef;
        targetPosition = waitPosition;
        waitPositionClient = waitPosition;

        // desactivamos el pathfinding
        var aiPath = GetComponent<AIPath>();
        var destSetter = GetComponent<AIDestinationSetter>();

        if (aiPath != null)
        {
            aiPath.canMove = false;
            aiPath.enabled = false;
        }

        if (destSetter != null)
            destSetter.enabled = false;

        // Cambiamos el estado de satisfacción para empezar a contar los segundos de espera
        satisfaction?.OnStateChange("WaitingPoint"); 
        // Inicia el movimiento desde el spawn hasta el WaitingPoint
        StartMoveToTarget();
    }

    // Método para empezar la rutina en la que se mueve el cliente hacia el targetPosition
    private void StartMoveToTarget()
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);

        // Activamos la animación de caminar
        anim.SetBool("isWalking", true);
        anim.SetFloat("Horizontal", 0f);
        anim.SetFloat("Vertical", 1f);

        moveCoroutine = StartCoroutine(MoveToTargetRoutine());
    }

    // Rutina que mueve al cliente suavemente hacia el targetPosition
    private IEnumerator MoveToTargetRoutine()
    {
        Vector3 lastPosition = transform.position;
        yield return null;

        while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
        {
            if (isDragging) yield break;

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            Vector3 dir = (transform.position - lastPosition).normalized;
            lastPosition = transform.position;

            // Se actualiza la animación
            if (anim != null)
            {
                anim.SetFloat("Horizontal", dir.x);
                anim.SetFloat("Vertical", dir.y);
            }

            yield return null;
        }

        yield return new WaitForSeconds(0.05f);

        // Se detiene y activamos isWaiting
        anim?.SetBool("isWalking", false);
        isWaiting = true;
    }

    // Método para detectar el click sobre el cliente
    private void OnMouseDown()
    {
        // Si el cliente está en uno de estos estados, este método no tiene efecto alguno
        if (!isWaiting || isLeaving || isSeated) return;
        if (assignedSeat != null) return;

        // Activamos que se está arrastrando el cliente y desactivamos el collider para mejor control sobre él en el mapa
        isDragging = true;
        col.enabled = false;

        startDragPosition = transform.position;
        transform.position = GetMouseWorldPos();
    }

    // Se actualiza la posición mientras es arrastrado
    private void OnMouseDrag()
    {
        if (!isDragging || !isWaiting || isLeaving) return;
        transform.position = GetMouseWorldPos();
    }

    // Método para ver si se ha dropeado correctamente el cliente
    private void OnMouseUp()
    {
        if (isLeaving) return;
        if (!isDragging) return;

        isDragging = false;
        col.enabled = true;

        // Se comprueba si realmente se soltó sobre una de las sillas
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(col.bounds.center, col.bounds.size, 0f);
        bool foundSeat = false;
        bool seatedSuccessfully = false;

        foreach (var hit in hitColliders)
        {
            if (hit != null && hit.TryGetComponent(out Table seatDropArea))
            {
                foundSeat = true;
                int oldSeatCount = (assignedSeat != null) ? 1 : 0;
                seatDropArea.OnClientDrop(this);

                if (assignedSeat != null && assignedSeat.IsOccupied)
                {
                    seatedSuccessfully = true;
                }

                break;
            }
        }
        // si la ha encontrado desactivamos que esté en espera
        if (foundSeat && seatedSuccessfully)
        {
            isWaiting = false;
        }
        else
        {
            // por el contrario si no la ha encontrado o simplemente no se dropeó en una, el cliente vuelve al waitingPoint
            transform.position = waitPositionClient;
            isWaiting = true;
            isDragging = false;
            col.enabled = true;
            assignedSeat = null;
            assignedTable = null;
        }
    }


    // Devuelve al cliente a su punto de espera
    public void ReturnToWaitingPoint()
    {
        transform.position = waitPositionClient;
        isWaiting = true;
        isDragging = false;
        anim?.SetBool("isWalking", false);
    }

    // Emplea la lógica con la cuál el cliente se sienta
    public void SitDown(string direction, Table table)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        if (queueMoveCoroutine != null)
        {
            StopCoroutine(queueMoveCoroutine);
            queueMoveCoroutine = null;
        }

        assignedTable = table;

        seatSide = direction.ToLower() == "right" ? SeatSide.Right : SeatSide.Left;

        // se anima dependiendo de la dirección que se haya sentado
        anim?.SetTrigger(direction);
        // notificamos al ClientSatisfaction que el cliente se ha sentado
        GetComponent<ClientSatisfaction>()?.OnStateChange("WaitingOrder");
        StartCoroutine(GiveOrder());

        // notificamos al spawner que el cliente se ha sentado
        spawner?.NotifyClientSeated(gameObject);

        isWaiting = false;
        isSeated = true;
    }

    // Lógica para ver los pedidos del cliente
    private IEnumerator GiveOrder()
    {
        // se espera un tiempo aleatorio defindido para pedir la orden
        float waitTime = Random.Range(thinkingOrderIntervalStart, thinkingOrderIntervalEnd);
        yield return new WaitForSeconds(waitTime);

        // escogemos la comida que va a pedir el cliente
        if (possibleFoods.Count > 0)
        {
            CurrentOrder = possibleFoods[Random.Range(0, possibleFoods.Count)];

            // activamos el globito del pedido
            if (CurrentOrder != null && foodRenderer != null)
            {
                orderBalloon.SetActive(true);
                foodRenderer.sprite = CurrentOrder.orderSprite;
            }

            hasOrdered = true;
        }
    }

    // Métodos para verificar el estado de la orden del pedido
    public bool IsReadyToTakeOrder() => hasOrdered && CurrentOrder != null && !orderTaken;
    public bool HasOrder() => CurrentOrder != null;
    public bool IsOrderTaken() => orderTaken;

    // Obtenemos la orden del cliente
    public Food GetCurrentOrder() => CurrentOrder;

    // Cuando se acepta el pedido de un cliente utilizamos este método para cambiar de estado a WaitingFood
    public void MarkOrderTaken()
    {
        orderTaken = true;
        satisfaction?.OnStateChange("WaitingFood");
    }
    
    // Método que indica que el pedido ha llegado al cliente y le avisa al ClientSatisfaction que está comiendo
    public void OnDishPlaced()
    {
        satisfaction?.OnStartEating();
    }

    // Oscila el sprite entre los sprites normales y enrojecerlos para el efecto de enjoarse del cliente
    public IEnumerator changeAngry()
    {
        float duration = 1.0f;

        while (true)
        {
            float pingPongTime = Mathf.PingPong(Time.time, duration);
            sr.color = Color.Lerp(originalColor, angryColor, pingPongTime / duration);
            yield return null;
        }
    }

    // Este método indica el momento en la que empezar la rutina de enojarse del cliente
    public void StartAngryEffect()
    {
        if (isAngry) return;
        isAngry = true;
        angryCoroutine = StartCoroutine(changeAngry());
    }

    // Cuando ya es atendido un cliente que estaba enojado, para desactivar el efecto tenemos este método para restaurar su color original
    public void StopAngryEffect()
    {
        if (angryCoroutine != null)
        {
            StopCoroutine(angryCoroutine);
            angryCoroutine = null;
        }

        isAngry = false;

        if (sr != null)
            sr.color = originalColor;
    }

    // Con este método realizamos la lógica de irse del restaurante, activando el pathfinding y liberamos los asientos
    // par que otros clientes puedan sentarse en la silla que se sentó este cliente
    public void LeaveRestaurant(Vector3 exitPosition)
    {
        var aiPath = GetComponent<AIPath>();
        var destSetter = GetComponent<AIDestinationSetter>();

        if (aiPath == null || destSetter == null)
        {
            return;
        }

        if (orderBalloon != null)
            orderBalloon.SetActive(false);
        CurrentOrder = null;
        hasOrdered = false;
        orderTaken = false;
        isSeated = false;
        isLeaving = true;

        aiPath.enabled = true;
        destSetter.enabled = true;
        aiPath.canMove = true;

        GameObject exitTarget = new GameObject("ExitTarget");
        exitTarget.transform.position = exitPosition;

        destSetter.target = exitTarget.transform;

        anim?.SetBool("isWalking", true);

        if (assignedSeat != null)
        {
            if (assignedSeat is leftChairDropArea leftChair)
                leftChair.FreeSeat();
            else if (assignedSeat is rightChairDropArea rightChair)
                rightChair.FreeSeat();

            assignedSeat = null;
        }

        StartCoroutine(CheckIfArrived(exitTarget.transform));
    }

    // Método que veifica si el cliente ha llegado al exitPoint, de modo que si llega notificamos al spawner y destruimos este objeto
    private IEnumerator CheckIfArrived(Transform target)
    {
        var aiPath = GetComponent<AIPath>();

        while (Vector3.Distance(transform.position, target.position) > 0.5f)
        {
            yield return null;
        }

        anim?.SetBool("isWalking", false);
        StopAngryEffect();

        spawner?.NotifyClientLeft(gameObject);

        Destroy(target.gameObject);
        Destroy(gameObject);
    }


    // Convertimos la posición del ratón para que se pueda adaptar al mundo 2D
    // por eso congelamos la profundidad Z y retornamos la posición del ratón
    private Vector3 GetMouseWorldPos()
    {
        Vector3 p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        p.z = 0f;
        return p;
    }

    // Actualiza el movimiento del cliente con el pathfinding junto con sus animaciones
    public void Update()
    {
        anim?.SetBool("isDragging", isDragging);

        var aiPath = GetComponent<AIPath>();
        if (aiPath != null && aiPath.enabled && anim != null)
        {
            Vector2 velocity = aiPath.desiredVelocity;
            bool isMoving = velocity.magnitude > 0.05f;

            AnimatorStateInfo currentState = anim.GetCurrentAnimatorStateInfo(0);

            if (isMoving)
            {
                Vector2 dir = velocity.normalized;
                anim.SetFloat("Horizontal", dir.x);
                anim.SetFloat("Vertical", dir.y);

                if (!currentState.IsName("Walking Tree") && !anim.GetBool("isWalking"))
                {
                    anim.SetBool("isWalking", true);
                }
            }
            else
            {
                if (anim.GetBool("isWalking"))
                {
                    anim.SetBool("isWalking", false);
                }
            }
        }
    }

    // Actualiza las posiciones de movimiento del cliente
    public void MoveTo(Vector3 newPosition)
    {
        if (isDragging) return;
        if (queueMoveCoroutine != null)
        {
            StopCoroutine(queueMoveCoroutine);
            queueMoveCoroutine = null;
        }

        waitPositionClient = newPosition;
        targetPosition = newPosition;

        queueMoveCoroutine = StartCoroutine(MoveToTargetSmooth(newPosition));
    }

    // Activamos la animación de caminar hacia algún punto
    private IEnumerator MoveToTargetSmooth(Vector3 newPos)
    {
        if (isDragging) yield break;

        anim?.SetBool("isWalking", true);
        anim?.SetFloat("Horizontal", 0f);
        anim?.SetFloat("Vertical", 1f);

        while (Vector3.Distance(transform.position, newPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, newPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = newPos;
        waitPositionClient = newPos;
        anim?.SetBool("isWalking", false);
        queueMoveCoroutine = null;
    }

}