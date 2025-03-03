using UnityEngine;

public class FollowObject : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private Transform followObject;

    private Vector3 _followObjectPreviousPosition;
    #endregion

    #region ---EventFunctions---
    private void Start() {
        _followObjectPreviousPosition = followObject.position;
    }

    private void Update() {
        if (followObject == null) return;

        Move();
    }
    #endregion

    public void SetFollowObject(Transform gameObjectTransform) {
        followObject = gameObjectTransform;
        _followObjectPreviousPosition = followObject.position;
    }

    private void Move() {
        var deltaMovement = followObject.position - _followObjectPreviousPosition;

        transform.position += deltaMovement;

        _followObjectPreviousPosition = followObject.position;
    }
}