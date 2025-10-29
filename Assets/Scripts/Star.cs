using UnityEngine;
using UnityEngine.UI;

public class Star : MonoBehaviour
{
   [SerializeField] public Image YellowStar;

    private void Awake()
    {
        YellowStar.transform.localScale = Vector3.zero;
    }
}
