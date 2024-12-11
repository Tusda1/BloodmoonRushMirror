using UnityEngine;
using UnityEngine.UIElements;

public class UITK_WorldSpaceInteractive : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private PanelSettings panelSettings;
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private Camera uiCamera;
    #endregion

    private void Start() {
        uiCamera ??= GameObject.FindWithTag("UICamera").GetComponent<Camera>();
        
        panelSettings.SetScreenToPanelSpaceFunction(_ => {
            var invalidPosition = new Vector2(float.NaN, float.NaN);

            var cameraRay = uiCamera.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(cameraRay.origin, cameraRay.direction * 100f, Color.magenta);

            if (!Physics.Raycast(cameraRay, out var hit, 100f, LayerMask.GetMask("RenderTextures"))) {
                Debug.Log("invalid position");
                return invalidPosition;
            }

            var pixelUV = hit.textureCoord;

            pixelUV.y = 1 - pixelUV.y;
            pixelUV.x *= panelSettings.targetTexture.width;
            pixelUV.y *= panelSettings.targetTexture.height;

            return pixelUV;
        });

    }
}
