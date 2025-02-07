using System;
using UnityEngine;

public class VineAttack : DrucillaState
{
    [SerializeField] private VineScript[] vines;

    private void OnEnable()
    {
        EndState();
    }
}
