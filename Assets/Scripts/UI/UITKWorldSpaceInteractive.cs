using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

public class UITKWorldSpaceInteractive : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private UIDocument uiDocument;

    private Camera _renderTextureCamera;
    #endregion
    
    [Inject]
    public void Construct(IObjectResolver container) {
        _renderTextureCamera = container.ResolveInstanceID<Camera>(PersistentManagersInstaller.RenderTextureCameraID);
    }

    private void Start() {
        uiDocument.panelSettings.SetScreenToPanelSpaceFunction(_ => {
            var invalidPosition = new Vector2(float.NaN, float.NaN);
            if (_renderTextureCamera == null) return invalidPosition;

            var cameraRay = _renderTextureCamera.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(cameraRay.origin, cameraRay.direction * 100f, Color.magenta);

            if (!Physics.Raycast(cameraRay, out var hit, 100f, LayerMask.GetMask("RenderTextures"))) {
                return invalidPosition;
            }

            var pixelUV = hit.textureCoord;

            pixelUV.y = 1 - pixelUV.y;
            pixelUV.x *= uiDocument.panelSettings.targetTexture.width;
            pixelUV.y *= uiDocument.panelSettings.targetTexture.height;

            return pixelUV;
        });
    }
}