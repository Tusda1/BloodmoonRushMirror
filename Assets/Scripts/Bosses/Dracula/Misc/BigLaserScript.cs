using System.Collections;
using UnityEngine;

public class BigLaserScript : MonoBehaviour
{
    [HideInInspector] public Vector2 direction;
    [SerializeField] private float speed = 1f;

    private void OnEnable()
    {
        StartCoroutine(Fire());
    }

    private IEnumerator Fire()
    {
        float targetScaleY = 0f;
        
        // Send a raycast in direction to determine distance
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 100f, LayerMask.GetMask("Ground"));
        targetScaleY = hit.distance;
        
        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = new Vector3(initialScale.x, targetScaleY, initialScale.z);
        float distance = Mathf.Abs(targetScaleY - initialScale.y);
        float duration = distance / speed;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        var localScale = transform.localScale;
        transform.localScale = new Vector3(localScale.x, 0, localScale.z);
    }
}