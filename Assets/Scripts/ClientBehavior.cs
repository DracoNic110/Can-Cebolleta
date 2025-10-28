using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

[RequireComponent(typeof(Collider2D))]
public class ClientBehavior : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private LayerMask tableLayerMask;
    private Coroutine queueMoveCoroutine = null;

    [Header("Sentarse en mesas")]
    public Table assignedTable;
    public Transform assignedTableTransform;
    public enum SeatSide { Left, Right }
    public SeatSide seatSide;

    [Header("Pedido")]
    [SerializeField] private GameObject orderBalloon;
    [SerializeField] private List<Food> possibleFoods;
    [SerializeField] private float thinkingOrderIntervalStart;
    [SerializeField] private float thinkingOrderIntervalEnd;
    private SpriteRenderer balloonRenderer;
    private SpriteRenderer foodRenderer;
    public Food CurrentOrder;

    private Vector3 targetPosition;
    private Vector3 waitPositionClient;
    private Animator anim;

    private bool isDragging = false;
    private bool isWaiting = false;
    public bool isAngry = false;
    private Coroutine moveCoroutine = null;
    private ClientSpawner spawner = null;

    private Collider2D col;
    private Vector3 startDragPosition;

    private bool hasOrdered = false;
    public bool orderTaken = false;
    public bool HasPendingOrder => CurrentOrder != null && !orderTaken;

    private ClientSatisfaction satisfaction;
    private Color originalColor;
    private SpriteRenderer sr;
    public Color angryColor = Color.red;
    private Coroutine angryCoroutine = null;


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

    public void Initialize(Vector3 waitPosition, ClientSpawner spawnerRef = null)
    {
        spawner = spawnerRef;
        targetPosition = waitPosition;
        waitPositionClient = waitPosition;

        var aiPath = GetComponent<AIPath>();
        var destSetter = GetComponent<AIDestinationSetter>();

        if (aiPath != null)
        {
            aiPath.canMove = false;
            aiPath.enabled = false;
        }

        if (destSetter != null)
            destSetter.enabled = false;

        satisfaction?.OnStateChange("WaitingPoint");

        StartMoveToTarget();
    }

    private void StartMoveToTarget()
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);

        anim.SetBool("isWalking", true);
        anim.SetFloat("Horizontal", 0f);
        anim.SetFloat("Vertical", 1f);

        moveCoroutine = StartCoroutine(MoveToTargetRoutine());
    }


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

            if (anim != null)
            {
                anim.SetFloat("Horizontal", dir.x);
                anim.SetFloat("Vertical", dir.y);
            }

            yield return null;
        }

        yield return new WaitForSeconds(0.05f);

        anim?.SetBool("isWalking", false);
        isWaiting = true;
    }


    private void OnMouseDown()
    {
        if (!isWaiting) return;
        startDragPosition = transform.position;
        transform.position = GetMouseWorldPos();
        isDragging = true;
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;
        transform.position = GetMouseWorldPos();
    }

    private void OnMouseUp()
    {
        if (!isDragging) return;
        col.enabled = false;
        Collider2D hitCollider = Physics2D.OverlapPoint(transform.position);
        col.enabled = true;
        isDragging = false;
        isWaiting = false;

        if (hitCollider != null && hitCollider.TryGetComponent(out Table seatDropArea))
        {
            seatDropArea.OnClientDrop(this);
        }
        else
        {
            transform.position = waitPositionClient;
            isWaiting = true;
        }
    }

    public void ReturnToWaitingPoint()
    {
        Debug.Log($"⏳ {name} regresa a su punto de espera (mesa ocupada con dinero).");
        transform.position = waitPositionClient;
        isWaiting = true;
        isDragging = false;
        anim?.SetBool("isWalking", false);
    }


    public void SitDown(string direction, Table table)
    {
        assignedTable = table;

        seatSide = direction.ToLower() == "right" ? SeatSide.Right : SeatSide.Left;

        anim?.SetTrigger(direction);
        GetComponent<ClientSatisfaction>()?.OnStateChange("WaitingOrder");
        StartCoroutine(GiveOrder());

        spawner?.NotifyClientSeated(gameObject);
        isWaiting = false;
    }




    private IEnumerator GiveOrder()
    {
        float waitTime = Random.Range(thinkingOrderIntervalStart, thinkingOrderIntervalEnd);
        yield return new WaitForSeconds(waitTime);

        if (possibleFoods.Count > 0)
        {
            CurrentOrder = possibleFoods[Random.Range(0, possibleFoods.Count)];

            if (CurrentOrder != null && foodRenderer != null)
            {
                orderBalloon.SetActive(true);
                foodRenderer.sprite = CurrentOrder.orderSprite;
            }

            hasOrdered = true;
            Debug.Log($"{this.name} ha pedido: {CurrentOrder.name}");
        }
    }

    public bool IsReadyToTakeOrder() => hasOrdered && CurrentOrder != null && !orderTaken;
    public bool HasOrder() => CurrentOrder != null;
    public bool IsOrderTaken() => orderTaken;
    public Food GetCurrentOrder() => CurrentOrder;
    public void MarkOrderTaken()
    {
        orderTaken = true;
        satisfaction?.OnStateChange("WaitingFood");
    }

    public void OnDishPlaced()
    {
        satisfaction?.OnStartEating();
    }

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

    public void StartAngryEffect()
    {
        if (isAngry) return;
        isAngry = true;
        angryCoroutine = StartCoroutine(changeAngry());
    }

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


    public void LeaveRestaurant(Vector3 exitPosition)
    {
        var aiPath = GetComponent<AIPath>();
        var destSetter = GetComponent<AIDestinationSetter>();

        if (aiPath == null || destSetter == null)
        {
            Debug.LogWarning($"{name} no tiene los componentes necesarios para moverse con A* Pathfinding.");
            return;
        }

        if (orderBalloon != null)
            orderBalloon.SetActive(false);
        CurrentOrder = null;
        hasOrdered = false;
        orderTaken = false;

        aiPath.enabled = true;
        destSetter.enabled = true;
        aiPath.canMove = true;

        GameObject exitTarget = new GameObject("ExitTarget");
        exitTarget.transform.position = exitPosition;

        destSetter.target = exitTarget.transform;

        anim?.SetBool("isWalking", true);

        StartCoroutine(CheckIfArrived(exitTarget.transform));
    }


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



    private Vector3 GetMouseWorldPos()
    {
        Vector3 p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        p.z = 0f;
        return p;
    }

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