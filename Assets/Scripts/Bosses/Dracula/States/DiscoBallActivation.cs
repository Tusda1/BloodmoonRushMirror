using System;
using UnityEngine;

public class DiscoBallActivation : DraculaState
{
    [SerializeField] private GameObject discoBall;

    private void OnEnable()
    {
        discoBall.SetActive(true);
        
        EndState();
    }
}
