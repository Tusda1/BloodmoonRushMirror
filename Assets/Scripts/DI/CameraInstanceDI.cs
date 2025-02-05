using UnityEngine;

public class CameraInstanceDI : InstanceDI<Camera> {
    #region ---Consts---
    public const string RenderTextureCameraID = "RenderTextureCamera";
    public const string MainCameraID = "MainCamera";
    #endregion

    public CameraInstanceDI(string id, Camera instance) : base(id, instance) {
    }
}