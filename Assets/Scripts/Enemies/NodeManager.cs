using UnityEngine;
using System.Collections.Generic;

public class NodeManager : MonoBehaviour
{
    [SerializeField] private int xDistance = 5;
    [SerializeField] private int yDistance = 5;
    [SerializeField] private LayerMask groundLayer;

    private List<GameObject> emptyObjects = new List<GameObject>();

    void Start()
    {
        SnapToGrid();
        CheckTiles();
        FillMissingPositions();
        UpdateGraph();
    }
    
    private void SnapToGrid()
    {
        Vector3 position = transform.position;
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);
        transform.position = position;
    }

    private void CheckTiles()
    {
        for (int y = yDistance; y >= -yDistance; y--)
        {
            for (int x = -xDistance; x <= xDistance; x++)
            {
                Vector2 tileCenter = new Vector2(transform.position.x + x + 0.5f, transform.position.y + y + 0.5f);
                Collider2D groundHit = Physics2D.OverlapPoint(tileCenter, groundLayer);

                if (groundHit != null)
                {
                    Vector2 aboveTileCenter = new Vector2(tileCenter.x, tileCenter.y + 1);
                    Collider2D aboveHit = Physics2D.OverlapPoint(aboveTileCenter, groundLayer);

                    if (aboveHit == null)
                    {
                        GameObject emptyObject = new GameObject("EmptyObject");
                        emptyObject.transform.position = aboveTileCenter;
                        emptyObject.transform.parent = transform; // Set parent to current game object
                        emptyObjects.Add(emptyObject);
                    }
                }
            }
        }
    }

    private void FillMissingPositions()
    {
        List<GameObject> newEmptyObjects = new List<GameObject>();

        foreach (GameObject emptyObject in emptyObjects)
        {
            Vector2 position = emptyObject.transform.position;
            Vector2 leftPosition = new Vector2(position.x - 1, position.y);
            Vector2 rightPosition = new Vector2(position.x + 1, position.y);

            bool hasLeft = emptyObjects.Exists(obj => obj.transform.position == (Vector3)leftPosition);
            bool hasRight = emptyObjects.Exists(obj => obj.transform.position == (Vector3)rightPosition);

            if (!hasLeft)
            {
                Collider2D groundHitLeft = Physics2D.OverlapPoint(leftPosition, groundLayer);
                if (groundHitLeft == null)
                {
                    GameObject newEmptyObject = new GameObject("EmptyObject");
                    newEmptyObject.transform.position = leftPosition;
                    newEmptyObject.transform.parent = transform; // Set parent to current game object
                    newEmptyObjects.Add(newEmptyObject);
                }
            }

            if (!hasRight)
            {
                Collider2D groundHitRight = Physics2D.OverlapPoint(rightPosition, groundLayer);
                if (groundHitRight == null)
                {
                    GameObject newEmptyObject = new GameObject("EmptyObject");
                    newEmptyObject.transform.position = rightPosition;
                    newEmptyObject.transform.parent = transform; // Set parent to current game object
                    newEmptyObjects.Add(newEmptyObject);
                }
            }
        }

        emptyObjects.AddRange(newEmptyObjects);
    }

    private void UpdateGraph()
    {
        AstarPath.active.Scan();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 topLeft = new Vector3(transform.position.x - xDistance + 0.5f, transform.position.y + yDistance + 0.5f, 0);
        Vector3 topRight = new Vector3(transform.position.x + xDistance + 0.5f, transform.position.y + yDistance + 0.5f, 0);
        Vector3 bottomLeft = new Vector3(transform.position.x - xDistance + 0.5f, transform.position.y - yDistance + 0.5f, 0);
        Vector3 bottomRight = new Vector3(transform.position.x + xDistance + 0.5f, transform.position.y - yDistance + 0.5f, 0);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}