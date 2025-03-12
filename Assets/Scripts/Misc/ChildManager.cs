using System;
using UnityEngine;

public class ChildManager : MonoBehaviour {
    [SerializeField] private Color childrenColor = Color.white;
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = 0;

    private void OnValidate()
    {
        foreach (Transform child in transform)
        {
            var spriteRenderer = child.GetComponent<SpriteRenderer>();
            if (spriteRenderer)
            {
                spriteRenderer.color = childrenColor;
                spriteRenderer.sortingLayerName = sortingLayerName;
                spriteRenderer.sortingOrder = sortingOrder;
            }
        }
    }
}
