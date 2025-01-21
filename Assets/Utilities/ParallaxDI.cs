using System;
using UnityEngine;
using Zenject;

public class ParallaxDI : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private ParallaxElement[] parallaxElements;
    
    [Inject(Id = "MainCamera")] private Transform _mainCameraTransform;
    
    private Vector3 _previousCameraPosition;
    private Vector3 _cameraDeltaMovement;
    #endregion

    private void OnEnable() {
        _previousCameraPosition = _mainCameraTransform.position;

        foreach (var parallaxElement in parallaxElements) {
            if (parallaxElement.gameObject == null) {
                Debug.LogError("Parallax element has no gameObject!", this);
                continue;
            }

            parallaxElement.objectTransform = parallaxElement.gameObject.transform;
            parallaxElement.spriteRenderer = parallaxElement.objectTransform.GetComponent<SpriteRenderer>();

            if (parallaxElement.gameObject == null) {
                Debug.LogError("Parallax gameObject has no spriteRenderer!", this);
                continue;
            }


            if (parallaxElement.xAxisParallaxEnabled) {
                parallaxElement.ParallaxEffectActions += ApplyParallaxMovementXAxis;

                if (parallaxElement.infiniteScrollingXAxis) {
                    SetInfiniteScrollingEffectXAxis(parallaxElement);
                    parallaxElement.ParallaxEffectActions += ApplyInfiniteScrollingMovementXAxis;
                }
            }

            if (parallaxElement.yAxisParallaxEnabled) {
                parallaxElement.ParallaxEffectActions += ApplyParallaxMovementYAxis;

                if (parallaxElement.infiniteScrollingYAxis) {
                    SetInfiniteScrollingEffectYAxis(parallaxElement);
                    parallaxElement.ParallaxEffectActions += ApplyInfiniteScrollingMovementYAxis;
                }
            }
        }
    }

    private void OnDisable() {
        foreach (var parallaxElement in parallaxElements) {
            parallaxElement.ResetEffectActions();

            if (parallaxElement.infiniteScrollingXAxis) {
                UnsetInfiniteScrollingEffectXAxis(parallaxElement);
            }

            if (parallaxElement.infiniteScrollingYAxis) {
                UnsetInfiniteScrollingEffectYAxis(parallaxElement);
            }
        }
    }

    private void LateUpdate() {
        _cameraDeltaMovement = _mainCameraTransform.position - _previousCameraPosition;

        foreach (var parallaxElement in parallaxElements) {
            parallaxElement.InvokeParallaxEffectActions();
        }

        _previousCameraPosition = _mainCameraTransform.position;
    }
    
    #region ---XAxis---
    private static void SetInfiniteScrollingEffectXAxis(ParallaxElement parallaxElement) {
        parallaxElement.spriteRenderer.drawMode = SpriteDrawMode.Tiled;
        parallaxElement.spriteRenderer.size = new Vector2(parallaxElement.spriteRenderer.size.x * 3,
            parallaxElement.spriteRenderer.size.y);
    }

    private static void UnsetInfiniteScrollingEffectXAxis(ParallaxElement parallaxElement) {
        parallaxElement.spriteRenderer.size = new Vector2(parallaxElement.spriteRenderer.size.x / 3,
            parallaxElement.spriteRenderer.size.y);
    }

    private void ApplyInfiniteScrollingMovementXAxis(ParallaxElement parallaxElement) {
        var parallaxElementSprite = parallaxElement.spriteRenderer.sprite;
        var width = parallaxElementSprite.texture.width / parallaxElementSprite.pixelsPerUnit;

        if (_mainCameraTransform.position.x - parallaxElement.objectTransform.position.x >= width) {
            parallaxElement.objectTransform.position += new Vector3(width, 0, 0);
        }

        if (_mainCameraTransform.position.x - parallaxElement.objectTransform.position.x <= -width) {
            parallaxElement.objectTransform.position -= new Vector3(width, 0, 0);
        }
    }


    private void ApplyParallaxMovementXAxis(ParallaxElement parallaxElement) {
        var parallaxMovementX = _cameraDeltaMovement.x + _cameraDeltaMovement.x * parallaxElement.xAxisEffectMultiplier;

        parallaxElement.objectTransform.position += new Vector3(parallaxMovementX, 0, 0);
    }
    #endregion

    #region ---YAxis---
    private static void SetInfiniteScrollingEffectYAxis(ParallaxElement parallaxElement) {
        parallaxElement.spriteRenderer.drawMode = SpriteDrawMode.Tiled;
        parallaxElement.spriteRenderer.size = new Vector2(parallaxElement.spriteRenderer.size.x,
            parallaxElement.spriteRenderer.size.y * 3);
    }

    private static void UnsetInfiniteScrollingEffectYAxis(ParallaxElement parallaxElement) {
        parallaxElement.spriteRenderer.size = new Vector2(parallaxElement.spriteRenderer.size.x,
            parallaxElement.spriteRenderer.size.y / 3);
    }

    private void ApplyInfiniteScrollingMovementYAxis(ParallaxElement parallaxElement) {
        var parallaxElementSprite = parallaxElement.spriteRenderer.sprite;
        var height = parallaxElementSprite.texture.height / parallaxElementSprite.pixelsPerUnit;

        if (_mainCameraTransform.position.y - parallaxElement.objectTransform.position.y >= height) {
            parallaxElement.objectTransform.position += new Vector3(0, height, 0);
        }

        if (_mainCameraTransform.position.y - parallaxElement.objectTransform.position.y <= -height) {
            parallaxElement.objectTransform.position -= new Vector3(0, height, 0);
        }
    }

    private void ApplyParallaxMovementYAxis(ParallaxElement parallaxElement) {
        var parallaxMovementY = _cameraDeltaMovement.y + _cameraDeltaMovement.y * parallaxElement.yAxisEffectMultiplier;

        parallaxElement.objectTransform.position += new Vector3(0, parallaxMovementY, 0);
    }
    #endregion

    [Serializable] public class ParallaxElement {
        [Header("Parallax Object")] public GameObject gameObject;
        [HideInInspector] public Transform objectTransform;
        [HideInInspector] public SpriteRenderer spriteRenderer;

        [Header("X Axis Parallax")] [Tooltip("Set to 0 to move equal to the camera and -1 to not move in the X axis")]
        public bool xAxisParallaxEnabled;

        public float xAxisEffectMultiplier;
        public bool infiniteScrollingXAxis;

        [Header("Y Axis Parallax")] [Tooltip("Set to 0 to move equal to the camera and -1 to not move in the Y axis")]
        public bool yAxisParallaxEnabled;

        public float yAxisEffectMultiplier;
        public bool infiniteScrollingYAxis;

        public event Action<ParallaxElement> ParallaxEffectActions = delegate { };

        public void InvokeParallaxEffectActions() {
            ParallaxEffectActions.Invoke(this);
        }

        public void ResetEffectActions() {
            ParallaxEffectActions = delegate { };
        }
    }
}