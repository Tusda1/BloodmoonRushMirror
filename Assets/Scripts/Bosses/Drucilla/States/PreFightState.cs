using System;
using UnityEngine;

public class PreFightState : DrucillaState
{
    private void OnEnable()
    {
        Boss.AnimancerComponent.Play(idleAnimation);
    }
}
