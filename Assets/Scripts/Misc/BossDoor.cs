using System.Collections;
using UnityEngine;

public class BossDoor : MonoBehaviour
{
    [SerializeField] private Collider2D bottomCollider;
    [SerializeField] private float closeSpeed = 5f;

    public void CloseDoor()
    {
        StartCoroutine(CloseDoorCoroutine());
    }

    private IEnumerator CloseDoorCoroutine()
    {
        while (!bottomCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            transform.position += Vector3.down * closeSpeed * Time.deltaTime;
            yield return null;
        }
    }
}
