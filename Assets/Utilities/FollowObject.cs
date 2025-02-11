using UnityEngine;

public class FollowObject : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private Transform followObject;
    
    private Vector3 _followObjectPreviousPosition;
    #endregion

    private void Start() {
        _followObjectPreviousPosition = followObject.position;
    }

    private void FixedUpdate() {
        var deltaMovement = followObject.position - _followObjectPreviousPosition;
        
        transform.position += deltaMovement;
        
        _followObjectPreviousPosition = followObject.position;
    }
}
