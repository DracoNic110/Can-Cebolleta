using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class ClientBehavior : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 2f;
    [Tooltip("Capa(s) de las mesas (LayerMask)")]
    [SerializeField] private LayerMask tableLayerMask;

    private Vector3 targetPosition;
    private Vector3 waitPositionClient;
    private Animator anim;
    private bool isDragging = false;
    private bool isWaiting = false;
    private bool isSeated = false;
    private Coroutine moveCoroutine = null;

    private Table currentTable = null;
    private ClientSpawner spawner = null;

    private Collider2D col;
    private Vector3 startDragPosition;

    private void Start() {
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
        else {
            // Si no hay mesa válida, queda en la posición de espera
            transform.position = waitPositionClient;
            isWaiting = true;
        }
    }

    public void SitDown(string direction) {
        isSeated = true;
        anim?.SetTrigger(direction);
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        p.z = 0f;
        return p;
    }

    public void Update() {
        anim?.SetBool("isDragging", isDragging);
    }
}
