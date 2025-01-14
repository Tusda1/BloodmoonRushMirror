using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class UITKWorldSpaceInteractive : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private Camera uiCamera;
    #endregion

    private void Start() {
        var camObj = GameObject.FindWithTag("UICamera");
        Assert.IsNotNull(camObj);
        
        uiCamera = camObj.GetComponent<Camera>();
        Assert.IsNotNull(uiCamera);
        
        uiDocument.panelSettings.SetScreenToPanelSpaceFunction(_ => {
            var invalidPosition = new Vector2(float.NaN, float.NaN);
            if (uiCamera == null) return invalidPosition;

            var cameraRay = uiCamera.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(cameraRay.origin, cameraRay.direction * 100f, Color.magenta);

            if (!Physics.Raycast(cameraRay, out var hit, 100f, LayerMask.GetMask("RenderTextures"))) {
                // Debug.Log("invalid position");
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
