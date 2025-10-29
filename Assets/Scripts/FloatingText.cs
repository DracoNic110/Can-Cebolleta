using UnityEngine;
using TMPro;
using System.Collections;

public class FloatingText : MonoBehaviour
{
    public float floatDistance = 0.7f; 
    public float lifetime = 1.0f;
    public TextMeshPro textMesh;

    private Color startColor;
    private Vector3 startPos;
    private Vector3 targetPos;

    private void Awake()
    {
        if (textMesh == null)
            textMesh = GetComponent<TextMeshPro>();

        if (textMesh == null)

        startColor = textMesh != null ? textMesh.color : Color.green;
    }

    public void Initialize(string text, Color color, Vector3 worldPosition, float distance = 0.7f, float life = 1.0f)
    {
        if (textMesh == null)
        {
            return;
        }

        textMesh.text = text;
        textMesh.color = color;
        startColor = color;
        startPos = worldPosition;
        transform.position = startPos;
        floatDistance = distance;
        lifetime = life;
        targetPos = startPos + Vector3.up * floatDistance;

        StopAllCoroutines();
        StartCoroutine(FloatAndFadeRoutine());
    }

    private IEnumerator FloatAndFadeRoutine()
    {
        float elapsed = 0f;
        while (elapsed < lifetime)
        {
            float t = elapsed / lifetime;

            float ease = 1f - Mathf.Pow(1f - t, 2f);
            transform.position = Vector3.Lerp(startPos, targetPos, ease);

            Color c = textMesh.color;
            c.a = Mathf.Lerp(startColor.a, 0f, t);
            textMesh.color = c;

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
