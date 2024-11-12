using System;
using UnityEngine;

public class MovingPlatformScript : MonoBehaviour {
    #region ---Fields---
    [SerializeField] private GameObject movingPlatform;
    [SerializeField] private GameObject start;
    [SerializeField] private GameObject end;
    private Vector2 _velocity = Vector2.zero;
    [SerializeField] private float speed;
    [SerializeField] private float smoothTime;
    #endregion

    private void Start()
    {
        movingPlatform.transform.position = start.transform.position;
    }

    private void Update()
    {
        transform.position = Vector2.SmoothDamp(transform.position, end.transform.position, ref _velocity, smoothTime);
    }
}
