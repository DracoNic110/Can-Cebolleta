using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class ClientBehavior : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private LayerMask tableLayerMask;

    [Header("Pedido")]
    [SerializeField] private GameObject orderBalloon;
    [SerializeField] private List<Food> possibleFoods;
    private SpriteRenderer balloonRenderer;
    private SpriteRenderer foodRenderer;
    public Food CurrentOrder;

    private Vector3 targetPosition;
    private Vector3 waitPositionClient;
    private Animator anim;

    private bool isDragging = false;
    private bool isWaiting = false;
    private Coroutine moveCoroutine = null;

    private ClientSpawner spawner = null;

    private Collider2D col;
    private Vector3 startDragPosition;

    private bool hasOrdered = false;
    public bool orderTaken = false;
    public bool HasPendingOrder => CurrentOrder != null && !orderTaken;

    private void Start()
    {
        col = GetComponent<Collider2D>();
    }

    public void Initialize(Vector3 waitPosition, ClientSpawner spawnerRef = null)
    {
        spawner = spawnerRef;
        targetPosition = waitPosition;
        waitPositionClient = waitPosition;
        StartMoveToTarget();
    }

    private void Awake()
    {
        anim = GetComponent<Animator>();

        if (orderBalloon != null)
        {
            balloonRenderer = orderBalloon.GetComponent<SpriteRenderer>();
            Transform foodTransform = orderBalloon.transform.Find("foodSprite");
            if (foodTransform != null)
                foodRenderer = foodTransform.GetComponent<SpriteRenderer>();

            orderBalloon.SetActive(false);
        }
    }

    private void StartMoveToTarget()
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveToTargetRoutine());
    }

    private IEnumerator MoveToTargetRoutine()
    {
        anim?.SetBool("isWalking", true);

        while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
        {
            if (isDragging) yield break;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        anim?.SetBool("isWalking", false);
        isWaiting = true;
    }

    private void OnMouseDown()
    {
        if (!isWaiting) return;
        startDragPosition = transform.position;
        transform.position = GetMouseWorldPos();
        isDragging = true;
        anim?.SetBool("isWalking", false);
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

    public void SitDown(string direction)
    {
        anim?.SetTrigger(direction);
        StartCoroutine(giveOrder());
    }

    private IEnumerator giveOrder()
    {
        float waitTime = Random.Range(5f, 10f);
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
            Debug.Log($"{name} ha pedido: {CurrentOrder.name}");
        }
    }

    public bool IsReadyToTakeOrder()
    {
        return hasOrdered && CurrentOrder != null && !orderTaken;
    }

    public bool HasOrder()
    {
        return CurrentOrder != null;
    }

    public bool IsOrderTaken()
    {
        return orderTaken;
    }

    public Food GetCurrentOrder()
    {
        return CurrentOrder;
    }

    public void MarkOrderTaken()
    {
        orderTaken = true;
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
    }
}