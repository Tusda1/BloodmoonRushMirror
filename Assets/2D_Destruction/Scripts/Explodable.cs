using System;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class Explodable : MonoBehaviour {
    public Action<List<GameObject>> OnFragmentsGenerated;

    public bool allowRuntimeFragmentation;
    public int extraPoints;
    public int subshatterSteps;

    public string fragmentLayer = "Default";
    public string sortingLayerName = "Default";
    public int orderInLayer;

    public enum ShatterType { Triangle, Voronoi }

    public ShatterType shatterType;
    public List<GameObject> fragments = new();
    private List<List<Vector2>> polygons = new();

    private bool NeedToGenerateFragments => allowRuntimeFragmentation && fragments.Count == 0;

    /// <summary>
    /// Creates fragments if necessary
    /// </summary>
    public void Explode(Transform parent) {
        if (NeedToGenerateFragments) {
            generateFragments();
        }
        else {
            foreach (var frag in fragments) {
                frag.transform.parent = parent;
                frag.SetActive(true);
            }
        }

        if (fragments.Count > 0) {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Creates fragments and then disables them
    /// </summary>
    public void fragmentInEditor() {
        if (fragments.Count > 0) {
            DeleteFragments();
        }

        generateFragments();
        setPolygonsForDrawing();
        foreach (GameObject frag in fragments) {
            frag.transform.parent = transform;
            frag.SetActive(false);
        }
    }

    public void DeleteFragments() {
        foreach (var frag in fragments) {
            if (Application.isEditor) {
                DestroyImmediate(frag);
            }
            else {
                Destroy(frag);
            }
        }

        fragments.Clear();
        polygons.Clear();
    }

    /// <summary>
    /// Turns Gameobject into multiple fragments
    /// </summary>
    private void generateFragments() {
        fragments = new List<GameObject>();
        switch (shatterType) {
            case ShatterType.Triangle:
                fragments = SpriteExploder.GenerateTriangularPieces(gameObject, extraPoints, subshatterSteps);
                break;
            case ShatterType.Voronoi:
                fragments = SpriteExploder.GenerateVoronoiPieces(gameObject, extraPoints, subshatterSteps);
                break;
            default:
                Debug.Log("invalid choice");
                break;
        }

        //sets additional aspects of the fragments
        foreach (GameObject p in fragments) {
            if (p != null) {
                p.layer = LayerMask.NameToLayer(fragmentLayer);
                p.GetComponent<Renderer>().sortingLayerName = sortingLayerName;
                p.GetComponent<Renderer>().sortingOrder = orderInLayer;
            }
        }

        foreach (ExplodableAddon addon in GetComponents<ExplodableAddon>()) {
            if (addon.enabled) {
                addon.OnFragmentsGenerated(fragments);
            }
        }
    }

    private void setPolygonsForDrawing() {
        polygons.Clear();
        List<Vector2> polygon;

        foreach (GameObject frag in fragments) {
            polygon = new List<Vector2>();
            foreach (Vector2 point in frag.GetComponent<PolygonCollider2D>().points) {
                Vector2 offset = rotateAroundPivot((Vector2)frag.transform.position, (Vector2)transform.position,
                    Quaternion.Inverse(transform.rotation)) - (Vector2)transform.position;
                offset.x /= transform.localScale.x;
                offset.y /= transform.localScale.y;
                polygon.Add(point + offset);
            }

            polygons.Add(polygon);
        }
    }

    private Vector2 rotateAroundPivot(Vector2 point, Vector2 pivot, Quaternion angle) {
        Vector2 dir = point - pivot;
        dir = angle * dir;
        point = dir + pivot;
        return point;
    }

    void OnDrawGizmos() {
        if (Application.isEditor) {
            if (polygons.Count == 0 && fragments.Count != 0) {
                setPolygonsForDrawing();
            }

            Gizmos.color = Color.blue;
            Gizmos.matrix = transform.localToWorldMatrix;
            Vector2 offset = (Vector2)transform.position * 0;
            foreach (List<Vector2> polygon in polygons) {
                for (int i = 0; i < polygon.Count; i++) {
                    if (i + 1 == polygon.Count) {
                        Gizmos.DrawLine(polygon[i] + offset, polygon[0] + offset);
                    }
                    else {
                        Gizmos.DrawLine(polygon[i] + offset, polygon[i + 1] + offset);
                    }
                }
            }
        }
    }
}