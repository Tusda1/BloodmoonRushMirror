using System;
using Attributes;
using UnityEngine;

public class CameraSizeController : MonoBehaviour {
    #region ---Fields---
    [GetOrAddComponent, SerializeField, HideInInspector]
    private Camera renderTextureCamera;
    #endregion

    private void Awake() {
        var resolution = new Vector2Int(Screen.width, Screen.height);
        
        var aspectRatio = GetAspectRatio(resolution);

        renderTextureCamera.orthographicSize = aspectRatio switch {
            { x: 16, y: 9 } => 11.25f,
            { x: 8, y: 5 } => 12.5f,
            _ => renderTextureCamera.orthographicSize
        };

        Debug.Log(renderTextureCamera.orthographicSize);
    }

    private void Start() {
        renderTextureCamera.rect = new Rect(0.0f, 0.0f, 1, 1);
    }

    private void OnDestroy() {
        renderTextureCamera.orthographicSize = 11.25f;
    }

    private static Vector2Int GetAspectRatio(Vector2Int resolution) {
        var gcd = GreatestCommonDivisor(resolution.x, resolution.y);

        return new Vector2Int(resolution.x / gcd, resolution.y / gcd);
    }

    private static int GreatestCommonDivisor(int width, int height) {
        while (height != 0) {
            var remainder = width % height;
            width = height;
            height = remainder;
        }

        return width;
    }
}