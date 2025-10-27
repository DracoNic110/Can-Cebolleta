using UnityEngine;

public class WalkvaroWalk : MonoBehaviour
{
    private bool moovingRight;

    private Vector3 startPoint;

    private Vector3 endPoint;

    private Vector3 move = new Vector3(1f,0f,0f);

    [SerializeField] float speed;

    [SerializeField] private Animator animator;

    void Start()
    {
        startPoint = new Vector3(-2.55f, 3.367f, 0.0f);
        endPoint = new Vector3(2.55f, 3.367f, 0.0f);
        moovingRight = true;
    }

    void Update()
    {

        if (moovingRight)
        {
            transform.Translate(move * speed * Time.deltaTime);
        }

        if (transform.position.x > endPoint.x)
        {
            moovingRight = false;
            animator.SetBool("moovingRight", false);
            move.x = -1;
        }
        if (!moovingRight) {
            transform.Translate(move * speed * Time.deltaTime);
        }
        if (transform.position.x < startPoint.x) {
            moovingRight = true;
            animator.SetBool("moovingRight", true);
            move.x = 1;
        }

    }
}
