using System;
using UnityEngine;
using VContainer;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Parallax : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private bool startEnabled;
    [SerializeField] private ParallaxElement[] parallaxElements;

    private Camera _mainCamera;
    private Transform _mainCameraTransform;
    private Vector3 _previousCameraPosition;
    private Vector3 _cameraDeltaMovement;
    private bool _enabled;
    #endregion
    
    [Inject]
    public void Construct(IObjectResolver container) {
        _mainCamera = container.ResolveInstanceID<Camera>(LevelInstaller.MainCameraID);
    }

    private void Start() {
        _mainCameraTransform = _mainCamera.transform;
        
        _previousCameraPosition = _mainCameraTransform.position;
        
        foreach (var parallaxElement in parallaxElements) {
            if (parallaxElement.gameObject == null) {
                Debug.LogError("Parallax element has no gameObject!", this);
                continue;
            }

            parallaxElement.objectTransform ??= parallaxElement.gameObject.transform;
            parallaxElement.spriteRenderer ??= parallaxElement.objectTransform.GetComponent<SpriteRenderer>();
            parallaxElement.originalSize = parallaxElement.spriteRenderer.bounds.size;

            if (parallaxElement.spriteRenderer == null) {
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
        
        if (startEnabled) Enable();
    }
    
    

    public void Enable() {
        if (_enabled)   return;
        _enabled = true;
        
        _previousCameraPosition = _mainCameraTransform.position;
    }

    public void Disable() {
        if (!_enabled) return;
        _enabled = false;
    }

    private void LateUpdate() {
        if (!_enabled) return;
        
        _cameraDeltaMovement = _mainCameraTransform.position - _previousCameraPosition;

        foreach (var parallaxElement in parallaxElements) {
            parallaxElement.InvokeParallaxEffectActions();
        }

        _previousCameraPosition = _mainCameraTransform.position;
    }

#if UNITY_EDITOR
    [ContextMenu(nameof(SetParallaxElementsToChildren))]
    public void SetParallaxElementsToChildren() {
        var children = transform.GetComponentsInChildren<SpriteRenderer>();
        
        parallaxElements = new ParallaxElement[children.Length];
        
        for (var i = 0; i < children.Length; i++) {
            var spriteRenderer =children[i];
            var objectTransform = spriteRenderer.transform;
            
            parallaxElements[i] = new ParallaxElement {
                objectTransform = objectTransform,
                gameObject = objectTransform.gameObject,
                spriteRenderer = spriteRenderer,
                originalSize = spriteRenderer.size
            };
        }
        
        EditorUtility.SetDirty(this);
    }
#endif
    
    #region ---XAxis---
    private static void SetInfiniteScrollingEffectXAxis(ParallaxElement parallaxElement) {
        parallaxElement.spriteRenderer.drawMode = SpriteDrawMode.Tiled;
        parallaxElement.spriteRenderer.size = new Vector2(parallaxElement.spriteRenderer.size.x * 3,
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
        [Header("Parallax Object")] 
        public GameObject gameObject;
        public Transform objectTransform;
        public SpriteRenderer spriteRenderer;
        
        [HideInInspector] public Vector2 originalSize;

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