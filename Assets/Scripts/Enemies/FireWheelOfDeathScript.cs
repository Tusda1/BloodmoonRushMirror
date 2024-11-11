using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class FireWheelOfDeathScript : MonoBehaviour
{
    [SerializeField] private Transform centerPoint; // The center point around which the object will move
    [SerializeField] private float radius = 5f; // The radius of the circular path
    [SerializeField] private float speed = 1f; // The speed of the circular motion
    [SerializeField] private DamageInstance damageInstance; // The damage instance to apply to the player

    private float angle = 0f; // Initialize the angle to 0
    private LineRenderer lineRenderer;
    private int positionCount = 0;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
    }

    void Update()
    {
        if (centerPoint == null)
        {
            Debug.LogError("Center point is not assigned.");
            return;
        }

        // Increment the angle based on the speed and time
        angle += speed * Time.deltaTime;

        // Calculate the new position
        float x = centerPoint.position.x + Mathf.Cos(angle) * radius;
        float y = centerPoint.position.y + Mathf.Sin(angle) * radius;

        // Update the position of the object
        transform.position = new Vector2(x, y);

        // Update the LineRenderer to draw the path
        positionCount++;
        lineRenderer.positionCount = positionCount;
        lineRenderer.SetPosition(positionCount - 1, transform.position);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // If the player collides with the fire wheel, deal damage to the player
            IDamageable player = other.GetComponent<IDamageable>();
            if (player != null)
            {
                // Set the knockback direction based on the position of the player relative to the fire wheel
                Vector2 directionToPlayer = (other.transform.position - transform.position).normalized;
                player.TakeDamage(damageInstance);
            }
        }
    }
}